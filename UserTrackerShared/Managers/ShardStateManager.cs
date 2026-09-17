using System.Collections.Concurrent;
using System.Diagnostics;
using UserTrackerShared.DBClients;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerShared.States;
using UserTrackerShared.Utilities;
using Timer = System.Timers.Timer;

namespace UserTrackerShared.Managers
{
    public class ShardStateManager
    {
        private readonly Serilog.ILogger _shardLogger = Logger.GetLogger(LogCategory.Shard);
        private readonly Serilog.ILogger _performanceLogger = Logger.GetLogger(LogCategory.PullPerformance);
        private Timer? _setTimeTimer;

        public ShardStateManager(string Name)
        {
            _shardLogger.Information($"Creating ShardStateManager for {Name}");
            this.Name = Name;
        }
        public async void Start()
        {
            while (true)
            {
                try
                {
                    _shardLogger.Information($"Starting ShardStateManager for {Name}");
                    var response = await ScreepsAPI.GetAllMapStats(Name, "claim0");
                    if (response == null)
                    {
                        _shardLogger.Warning("Unable to load map stats for shard {Shard}; retrying in 30 seconds", Name);
                        await Task.Delay(TimeSpan.FromSeconds(30));
                        continue;
                    }

                    foreach (var room in response.Rooms)
                    {
                        if (!Rooms.Contains(room.Key))
                        {
                            Rooms.Add(room.Key);
                            _shardLogger.Information($"Added room {room.Key} to shard {Name}");
                        }
                    }
                    foreach (var (userId, user) in response.Users)
                    {
                        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(user.Username))
                        {
                            continue;
                        }

                        user.Id = userId;
                        GameState.Users.TryAdd(userId, user);
                    }
                    _ = MergeInitialUsersAsync(response.Users);

                    var message = $"Loaded Shard {Name} with rooms {response.Rooms.Count}";
                    _shardLogger.Information(message);
                    _ = StartUpdate();

                    _setTimeTimer = new Timer(300000);
                    _setTimeTimer.Elapsed += (s, e) => _ = StartUpdate();
                    _setTimeTimer.AutoReset = true;
                    _setTimeTimer.Enabled = true;
                    return;
                }
                catch (Exception ex)
                {
                    _shardLogger.Error(ex, "Error starting ShardStateManager for {Shard}; retrying in 30 seconds", Name);
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
            }
        }

        private async Task MergeInitialUsersAsync(IReadOnlyDictionary<string, ScreepsUser> users)
        {
            try
            {
                var mergedUsers = await GameState.MergeUsersRefetchingAsync(users);
                _shardLogger.Information("Merged {UserCount} users from initial map stats for shard {Shard}", mergedUsers, Name);
            }
            catch (Exception ex)
            {
                _shardLogger.Error(ex, "Error merging users from initial map stats for shard {Shard}", Name);
            }
        }

        public string Name { get; set; }
        private long? LastSyncTime { get; set; }
        public long Time { get; set; }
        public List<string> Rooms { get; set; } = [];
        public bool IsSyncing = false;
        private readonly ConcurrentDictionary<long, ConcurrentDictionary<string, ScreepsRoomHistoryDto>> dataByWindow = new();

        internal Func<string, string, long, long, ScreepsRoomHistoryDto, Task> RoomHistoryWriter { get; set; } = DBClient.WriteScreepsRoomHistory;
        internal Action<string, string, long, long, ScreepsRoomHistoryDto> UserHistoryWriter { get; set; } = DBClient.WriteScreepsUserHistory;
        internal Action<string, long, long, ScreepsRoomHistoryDto> GlobalHistoryWriter { get; set; } = DBClient.WriteScreepsGlobalHistory;


        public async Task StartUpdate()
        {
            var timeResponse = await ScreepsAPI.GetTimeOfShard(Name);
            if (timeResponse != null && Time != timeResponse.Time)
            {
                Time = timeResponse.Time;
                _ = StartSync();
            }
        }

        internal long GetSyncTime()
        {
            var availableTime = Math.Max(0, Time - 500);
            return availableTime - availableTime % ConfigSettingsState.TicksInFile;
        }

        internal async Task StartSync()
        {
            if (IsSyncing) return;
            ConfigSettingsState.ValidateTickWindows();
            var syncTime = GetSyncTime();
            if (LastSyncTime == null)
            {
                // Begin on both a file and an output-window boundary, including at startup.
                var start = Math.Max(0, syncTime - ConfigSettingsState.PullBackwardsTickAmount);
                var alignment = Math.Max(ConfigSettingsState.TicksInFile, ConfigSettingsState.TicksInObject);
                LastSyncTime = start - start % alignment;
            }

            var ticksToBeSynced = syncTime - LastSyncTime.Value;
            if (ticksToBeSynced <= 0) return;
            IsSyncing = true;

            var message = $"Syncing Shard {Name} for {ticksToBeSynced} ticks and {Rooms.Count} rooms, last sync time was {LastSyncTime}, current sync time is {syncTime}";
            _shardLogger.Warning(message);

            try
            {
                for (long i = LastSyncTime.Value; i < syncTime; i += ConfigSettingsState.TicksInFile)
                {
                    var resultCodes = new ConcurrentDictionary<int, int>();

                    var mainStopwatch = Stopwatch.StartNew();
                    var tasks = new List<Task>();

                    // ScreepsAPI bounds active HTTP requests. Do not hold another slot
                    // across retry delays: missing rooms would block subsequent rooms.
                    foreach (var room in Rooms)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                var statusResult = await RoomDataHelper.GetAndHandleRoomData(Name, room, i, dataByWindow);
                                resultCodes.AddOrUpdate(statusResult, 1, (key, value) => value + 1);
                            }
                            catch (Exception ex)
                            {
                                _shardLogger.Error(ex, "Error processing room {Room} for tick {Tick}", room, i);
                                resultCodes.AddOrUpdate(500, 1, (key, value) => value + 1); // Error code
                            }
                        }));
                    }
                    await Task.WhenAll(tasks);

                    var fileEnd = i + ConfigSettingsState.TicksInFile;
                    foreach (var windowStart in dataByWindow.Keys.OrderBy(tick => tick))
                    {
                        if (windowStart + ConfigSettingsState.TicksInObject > fileEnd) break;
                        if (!dataByWindow.TryRemove(windowStart, out var dataByRoom)) continue;
                        var globalData = new ScreepsRoomHistoryDto();
                        var dataByUser = new Dictionary<string, ScreepsRoomHistoryDto>();

                        var roomDataSnapshot = dataByRoom.ToArray();
                        foreach (var kvp in roomDataSnapshot)
                        {
                            try
                            {
                                var roomData = kvp.Value;
                                await RoomHistoryWriter(Name, kvp.Key, windowStart, roomData.TimeStamp, roomData);

                                if (!string.IsNullOrEmpty(roomData.UserId) && GameState.Users.TryGetValue(roomData.UserId, out ScreepsUser? user))
                                {
                                    var username = user.Username;
                                    if (!dataByUser.TryGetValue(username, out var userData))
                                    {
                                        userData = new ScreepsRoomHistoryDto();
                                        dataByUser[username] = userData;
                                    }
                                    userData.Combine(roomData);
                                }
                            }
                            catch (Exception ex)
                            {
                                _shardLogger.Error(ex, "Error uploading room data for {Room}", kvp.Key);
                            }
                        }

                        foreach (var userKvp in dataByUser)
                        {
                            try
                            {
                                UserHistoryWriter(Name, userKvp.Key, windowStart, userKvp.Value.TimeStamp, userKvp.Value);
                                globalData.Combine(userKvp.Value);
                            }
                            catch (Exception ex)
                            {
                                _shardLogger.Error(ex, "Error uploading user data for {User}", userKvp.Key);
                            }
                        }

                        if (dataByUser.Count > 0)
                        {
                            GlobalHistoryWriter(Name, windowStart, globalData.TimeStamp, globalData);
                        }
                    }
                    LastSyncTime = fileEnd;

                    mainStopwatch.Stop();
                    var totalMilliseconds = mainStopwatch.ElapsedMilliseconds;
                    var ticksBehind = GetSyncTime() - i;

                    DBClient.WritePerformanceData(new PerformanceClassDto
                    {
                        Shard = Name,
                        TicksBehind = ticksBehind,
                        TimeTakenMs = totalMilliseconds,
                        TotalRooms = Rooms.Count,
                        ResultCodes = resultCodes
                    });
                    try
                    {
                        var totalMicroSeconds = totalMilliseconds * 1000;
                        var performanceLogMessage = $"{Name}:{i} took {totalMilliseconds} milliseconds, is {ticksBehind} ticks behind and took {Math.Round(Convert.ToDouble(totalMicroSeconds / Rooms.Count), 2)} microseconds per room on average";
                        _performanceLogger.Information(performanceLogMessage);
                        Screen.AddLog(performanceLogMessage);
                    }
                    catch (Exception)
                    {
                        // Accepted
                    }
                }
                LastSyncTime = syncTime;
            }
            catch (Exception ex)
            {
                _shardLogger.Error(ex, $"Error syncing shard {Name}: {ex.Message}");
            }
            finally
            {
                IsSyncing = false;
            }
        }
    }
}
