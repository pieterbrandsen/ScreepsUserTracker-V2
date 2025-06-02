using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using System.Timers;
using UserTrackerScreepsApi;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerStates.DBClients;
using Timer = System.Timers.Timer;

namespace UserTrackerShared.States
{
    public class ShardState
    {
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.States);
        private bool _initialized;

        public ShardState(string Name)
        {
            this.Name = Name;

            _setTimeTimer = new Timer(300000);
            _setTimeTimer.Elapsed += OnSetTimeTimer;
            _setTimeTimer.AutoReset = true;
            _setTimeTimer.Enabled = true;
        }
        public async Task StartAsync()
        {
            var response = await ScreepsAPI.GetAllMapStats(Name, "claim0");
            foreach (var room in response.Rooms)
            {
                Rooms.Add(room.Key);
            }

            _initialized = true;
            _logger.Warning($"Loaded Shard {Name} with rooms {response.Rooms.Count}");
            _ = StartUpdate();
        }

        public string Name { get; set; }
        private long LastSyncTime { get; set; }
        public long Time { get; set; }
        public List<string> Rooms { get; set; } = new List<string>();
        private static Timer? _setTimeTimer;
        private bool isSyncing = false;

        public async Task StartUpdate()
        {
            var timeResponse = await ScreepsAPI.GetTimeOfShard(Name);
            if (timeResponse != null)
            {
                if (Time != timeResponse.Time)
                {
                    Time = timeResponse.Time;
                    if (isSyncing || !_initialized) return;
                    isSyncing = true;
                    _ = StartSync();
                }
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
            _logger.Warning($"Started sync Shard {Name} for {ticksToBeSynced} ticks and {Rooms.Count} rooms");
            for (long i = LastSyncTime; i < syncTime; i += 100)
            {
                var resultCodes = new ConcurrentDictionary<int, int>();

                var mainStopwatch = Stopwatch.StartNew();
                var semaphore = new SemaphoreSlim(Rooms.Count);
                var tasks = new List<Task>();

                var reservedRoomsByUser = new ConcurrentDictionary<string, ScreepsRoomHistoryDTO>();
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
                                    resultCodes[statusResult] = ++value;
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

                foreach (var historyDTO in reservedRoomsByUser)
                {
                    await DBClient.WriteScreepsRoomHistory(Name, "Reserved", i, historyDTO.Value.TimeStamp, historyDTO.Value);
                }


                mainStopwatch.Stop();
                var totalMilliseconds = mainStopwatch.ElapsedMilliseconds;
                var ticksBehind = GetSyncTime() - i;

                DBClient.WritePerformanceData(new PerformanceClassDTO
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
                }
            }
            LastSyncTime = syncTime;
            isSyncing = false;
        }
        private async void OnSetTimeTimer(Object? source, ElapsedEventArgs? e)
        {
            _= StartUpdate();
        }
    }
}
