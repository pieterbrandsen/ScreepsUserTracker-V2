using System.Diagnostics;
using System.Timers;
using UserTrackerScreepsApi;
using UserTrackerShared.Helpers;
using UserTrackerStates;
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
            StartUpdate();
        }

        public string Name { get; set; }
        private long LastSynceTime { get; set; }
        public long Time { get; set; }
        public List<string> Rooms { get; set; } = new List<string>();
        private static Timer? _setTimeTimer;
        private bool isSyncing = false;

        public async void StartUpdate()
        {
            var timeResponse = await ScreepsAPI.GetTimeOfShard(Name);
            if (timeResponse != null)
            {
                if (Time != timeResponse.Time)
                {
                    await StartSync(timeResponse.Time);
                }
                Time = timeResponse.Time;
            }
        }

        private async Task StartSync(long time)
        {
            if (isSyncing || !_initialized) return;
            isSyncing = true;

            var syncTime = Convert.ToInt32(Math.Round(Convert.ToDouble((time - 500) / 100)) * 100);
            if (LastSynceTime == 0) LastSynceTime = syncTime - 100 * 100;

            _logger.Warning($"Started sync Shard {Name} for {syncTime - LastSynceTime} ticks and {Rooms.Count} rooms");
            for (long i = LastSynceTime; i < syncTime; i += 100)
            {
                var mainStopwatch = Stopwatch.StartNew();

                //var getDataTasks = Rooms.Select(room => room.GetRoomData(i));
                //var getDataTaskResults = await Task.WhenAll(getDataTasks);
                //int successes = getDataTaskResults.Count(r => r);
                int successes = 0;
                var semaphore = new SemaphoreSlim(1000);
                var tasks = new List<Task>();
                foreach (var room in Rooms)
                {
                    await semaphore.WaitAsync();

                    tasks.Add(
                        Task.Run(async () =>
                        {
                            try
                            {
                                if (await RoomDataHelper.GetAndHandleRoomData(Name, room, i)) Interlocked.Increment(ref successes);
                            }
                            catch (Exception e)
                            {
                                _logger.Error(e, "");
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        })
                    );
                }
                await Task.WhenAll(tasks);
                mainStopwatch.Stop();
                var totalMiliseconds = mainStopwatch.ElapsedMilliseconds;

                await InfluxDBClientState.WritePerformanceData(new PerformanceClassDTO
                {
                    Shard = Name,
                    TicksBehind = syncTime - i,
                    TimeTakenMs = totalMiliseconds,
                    TotalRooms = Rooms.Count,
                    SuccessCount = successes
                });
                try
                {
                    var totalMicroSeconds = totalMiliseconds * 1000;
                    var performanceLogMessage = $"timeT {totalMiliseconds}, roomsT {successes}/{Rooms.Count}, shard {Name}:{i} timeT/roomsT {Math.Round(Convert.ToDouble(totalMicroSeconds / Rooms.Count), 2)}miS at {DateTime.Now.ToLongTimeString()}";
                    _logger.Information(performanceLogMessage);
                    Screen.AddLog(performanceLogMessage);
                }
                catch (Exception)
                {
                }
            }
            LastSynceTime = syncTime;
            isSyncing = false;
        }
        private async void OnSetTimeTimer(Object? source, ElapsedEventArgs? e)
        {
            StartUpdate();
        }
    }
}
