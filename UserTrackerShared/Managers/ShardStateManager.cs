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
            this.Name = Name;
        }
        public async Task StartAsync()
        {
            var response = await ScreepsApi.GetAllMapStats(Name, "claim0");
            foreach (var room in response.Rooms)
            {
                Rooms.Add(room.Key);
            }

            var message = $"Loaded Shard {Name} with rooms {response.Rooms.Count}";
            _logger.Warning(message);
            _ = StartUpdate();

            var setTimeTimer = new Timer(300000);
            setTimeTimer.Elapsed += (s, e) => _ = StartUpdate();
            setTimeTimer.AutoReset = true;
            setTimeTimer.Enabled = true;
        }

        public string Name { get; set; }
        private long LastSyncTime { get; set; }
        public long Time { get; set; }
        public List<string> Rooms { get; set; } = [];
        private bool isSyncing = false;

        public async Task StartUpdate()
        {
            var timeResponse = await ScreepsApi.GetTimeOfShard(Name);
            if (timeResponse != null && Time != timeResponse.Time)
            {
                Time = timeResponse.Time;
                _logger.Information($"Updated time of shard {Name} to {Time} ({isSyncing})");
                if (isSyncing) return;
                isSyncing = true;
                _ = StartSync();
            }
        }

        private long GetSyncTime()
        {
            var syncTime = Convert.ToInt32(Math.Round(Convert.ToDouble((Time - 500) / 100)) * 100);
            return syncTime;
        }

        private async Task StartSync()
        {
            var syncTime = GetSyncTime();
            if (LastSyncTime == 0) LastSyncTime = syncTime - ConfigSettingsState.PullBackwardsTickAmount;

            var ticksToBeSynced = syncTime - LastSyncTime;
            if (ticksToBeSynced <= 0)
            {
                isSyncing = false;
                return;
            }
            var message = $"Syncing Shard {Name} for {ticksToBeSynced} ticks and {Rooms.Count} rooms, last sync time was {LastSyncTime}, current sync time is {syncTime}";
            _logger.Warning(message);
            for (long i = LastSyncTime; i < syncTime; i += 100)
            {
                var resultCodes = new ConcurrentDictionary<int, int>();

                var mainStopwatch = Stopwatch.StartNew();
                var semaphore = new SemaphoreSlim(Rooms.Count);
                var tasks = new List<Task>();

                var reservedRoomsByUser = new ConcurrentDictionary<string, ScreepsRoomHistoryDto>();
                var userLocks = new ConcurrentDictionary<string, object>();
                foreach (var room in Rooms)
                {
                    await semaphore.WaitAsync();
                    tasks.Add(
                        Task.Run(async () =>
                        {
                            try
                            {
                                var statusResult = await RoomDataHelper.GetAndHandleRoomData(Name, room, i, reservedRoomsByUser, userLocks);
                                if (resultCodes.TryGetValue(statusResult, out int value))
                                {
                                    resultCodes[statusResult] = value + 1;
                                }
                                else
                                {
                                    resultCodes[statusResult] = 1;
                                }
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        })
                    );
                }
                await Task.WhenAll(tasks);

                foreach (var historyDto in reservedRoomsByUser.Select(x => x.Value))
                {
                    await DBClient.WriteScreepsRoomHistory(Name, "Reserved", i, historyDto.TimeStamp, historyDto);
                }


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
            isSyncing = false;
        }
    }
}
