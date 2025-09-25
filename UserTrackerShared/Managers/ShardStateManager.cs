using Newtonsoft.Json.Linq;
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
        private readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.States);

        public ShardStateManager(string Name)
        {
            _logger.Information($"Creating ShardStateManager for {Name}");
            this.Name = Name;
        }
        public async Task StartAsync()
        {
            _logger.Information($"Starting ShardStateManager for {Name}");
            var response = await ScreepsApi.GetAllMapStats(Name, "claim0");
            foreach (var room in response.Rooms)
            {
                Rooms.Add(room.Key);
            }

            var message = $"Loaded Shard {Name} with rooms {response.Rooms.Count}";
            _logger.Information(message);
            Screen.AddLog(message);

            _ = StartUpdate();

            var setTimeTimer = new Timer(300000);
            setTimeTimer.Elapsed += (s, e) => _ = StartUpdate();
            setTimeTimer.AutoReset = true;
            setTimeTimer.Enabled = true;

            if (ConfigSettingsState.KeepTrackOfOrderBook)
            {
                var onSyncOrderBookTimer = new Timer(1000);
                onSyncOrderBookTimer.AutoReset = true;
                onSyncOrderBookTimer.Enabled = true;
                onSyncOrderBookTimer.Elapsed += async (s, e) => await OnSyncOrderBookTimer();
            }
        }

        public string Name { get; set; }
        private long LastSyncTime { get; set; }
        public long Time { get; set; }
        public List<string> Rooms { get; set; } = [];
        private bool isSyncing = false;
        private long lastTickUploaded = 0;
        private ConcurrentDictionary<string, ScreepsRoomHistoryDto> dataByRoom = new();
        private long lastSyncedOrderBookTick = 0;
        private bool isSyncingOrderBook;

        private async Task<long?> GetTime()
        {
            var timeResponse = await ScreepsApi.GetTimeOfShard(Name);
            return (timeResponse != null && Time != timeResponse.Time) ? timeResponse.Time : null;
        }

        public async Task StartUpdate()
        {
            var time = await GetTime();
            if (time != null && Time != time)
            {
                Time = (long)time;
                if (isSyncing) return;
                isSyncing = true;
                _ = StartSync();
            }
        }

        public async Task OnSyncOrderBookTimer()
        {
            try
            {
                var time = await GetTime();
                if (time == null || isSyncingOrderBook) return;

                if (lastSyncedOrderBookTick < time)
                {
                    isSyncingOrderBook = true;
                    var orderBook = await ScreepsApi.GetMarketOrderbook(Name);
                    if (orderBook != null)
                    {
                        CentralOrderBookTrackerState.UpdateMarketOrderBook(orderBook);
                        lastSyncedOrderBookTick = (long)time;
                    }
                    isSyncingOrderBook = false;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"{e.Message}: {e.StackTrace}");
                isSyncingOrderBook = false;
            }
        }

        private long GetSyncTime()
        {
            var syncTime = Convert.ToInt32(Math.Round(Convert.ToDouble((Time - 500) / 100)) * 100);
            return syncTime;
        }

        private async Task StartSync()
        {
            try
            {
                var syncTime = GetSyncTime();
                if (LastSyncTime == 0) LastSyncTime = syncTime - ConfigSettingsState.PullBackwardsTickAmount;
                if (lastTickUploaded == 0) lastTickUploaded = LastSyncTime - 100;

                var ticksToBeSynced = syncTime - LastSyncTime;
                if (ticksToBeSynced <= 0)
                {
                    isSyncing = false;
                    return;
                }
                var message = $"Syncing Shard {Name} for {ticksToBeSynced} ticks and {Rooms.Count} rooms, last sync time was {LastSyncTime}, current sync time is {syncTime}";
                _logger.Information(message);
                Screen.AddLog(message);

                for (long i = LastSyncTime; i < syncTime; i += 100)
                {
                    var resultCodes = new ConcurrentDictionary<int, int>();

                    var shouldUploadAllData = i - lastTickUploaded >= ConfigSettingsState.TicksInObject;
                    var mainStopwatch = Stopwatch.StartNew();
                    var tasks = new List<Task>();

                    var userLocks = new ConcurrentDictionary<string, object>();
                    var semaphore = new SemaphoreSlim(1000);
                    foreach (var room in Rooms)
                    {
                        await semaphore.WaitAsync();
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                var statusResult = await RoomDataHelper.GetAndHandleRoomData(Name, room, i, dataByRoom, userLocks);
                                resultCodes.AddOrUpdate(statusResult, 1, (key, value) => value + 1);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex, "Error processing room {Room} for tick {Tick}", room, i);
                                resultCodes.AddOrUpdate(500, 1, (key, value) => value + 1); // Error code
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }));
                    }
                    await Task.WhenAll(tasks);

                    if (shouldUploadAllData)
                    {
                        var globalData = new ScreepsRoomHistoryDto();
                        var dataByUser = new ConcurrentDictionary<string, ScreepsRoomHistoryDto>();

                        var roomDataSnapshot = dataByRoom.ToArray();
                        foreach (var kvp in roomDataSnapshot)
                        {
                            try
                            {
                                var roomData = kvp.Value;
                                DBClient.WriteScreepsRoomHistory(Name, kvp.Key, i, roomData.TimeStamp, roomData);

                                if (!string.IsNullOrEmpty(roomData.UserId) && GameState.Users.TryGetValue(roomData.UserId, out ScreepsUser? user))
                                {
                                    var username = user.Username;
                                    dataByUser.AddOrUpdate(username, roomData, (key, existingData) =>
                                    {
                                        existingData.Combine(roomData);
                                        return existingData;
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex, "Error uploading room data for {Room}", kvp.Key);
                            }
                        }
                        dataByRoom.Clear();

                        foreach (var userKvp in dataByUser)
                        {
                            try
                            {
                                DBClient.WriteScreepsUserHistory(Name, userKvp.Key, i, userKvp.Value.TimeStamp, userKvp.Value);
                                globalData.Combine(userKvp.Value);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex, "Error uploading user data for {User}", userKvp.Key);
                            }
                        }

                        DBClient.WriteScreepsGlobalHistory(Name, i, globalData.TimeStamp, globalData);
                        lastTickUploaded = i;
                    }
                    CentralOrderBookTrackerState.TryFindMatchBetweenOrderBookAndTerminalData(Name, i);

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
                        _logger.Information(performanceLogMessage);
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
                _logger.Error(ex, $"Error syncing shard {Name}: {ex.Message}");
            }
            finally
            {
                isSyncing = false;
            }
        }
    }
}
