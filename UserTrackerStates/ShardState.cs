using System.Diagnostics;
using System.Timers;
using UserTrackerScreepsApi;
using UserTrackerShared.Models.ScreepsAPI;
using UserTrackerStates;
using Timer = System.Timers.Timer;

namespace UserTrackerShared.States
{
    public class ShardState
    {
        public ShardState(string Name)
        {
            this.Name = Name;

            _setTimeTimer = new Timer(300000);
            _setTimeTimer.Elapsed += OnSetTimeTimer;
            _setTimeTimer.AutoReset = true;
            _setTimeTimer.Enabled = true;

            AsyncConstructor();
        }
        async void AsyncConstructor()
        {
            var response = await ScreepsAPI.GetAllMapStats(Name, "claim0");
            Users = response.Users;
            foreach (var room in response.Rooms)
            {
                Rooms.Add(new RoomState(room.Key, Name));
            }

            OnSetTimeTimer(null, null);
        }

        public string Name { get; set; }
        private long LastSynceTime { get; set; }
        public long Time { get; set; }
        public List<RoomState> Rooms { get; set; } = new List<RoomState>();
        public Dictionary<string, MapStatUser> Users { get; set; } = new Dictionary<string, MapStatUser>();

        private static Timer? _setTimeTimer;
        private static bool isSyncing = false;

        private async void OnSetTimeTimer(Object? source, ElapsedEventArgs? e)
        {
            var timeResponse = await ScreepsAPI.GetTimeOfShard(Name);
            if (timeResponse != null)
            {
                if (Time != timeResponse.Time)
                {
                    if (isSyncing) return;
                    isSyncing = true;
                    var syncTime = Convert.ToInt32(Math.Round(Convert.ToDouble((timeResponse.Time - 500) / 100)) * 100);
                    if (LastSynceTime == 0) LastSynceTime = syncTime - 500 * 100;
                    
                    for (long i = LastSynceTime; i < syncTime; i += 100)
                    {
                        var mainStopwatch = Stopwatch.StartNew();
                        var stopwatches = new List<Stopwatch>();

                        var roomDict = Rooms.ToDictionary(r => r.Name);  // Faster lookups
                        var proxiesAvailable = GameState.GetAvailableProxies(Rooms.Count);

                        // Set up Parallel Options with max parallelism equal to the proxy count
                        var parallelOptions = new ParallelOptions
                        {
                            MaxDegreeOfParallelism = proxiesAvailable.Count // Limit parallelism to the proxy count
                        };

                        // Use Parallel.ForEachAsync to run tasks based on proxies
                        await Parallel.ForEachAsync(proxiesAvailable.Zip(Rooms, (proxy, room) => (room, proxy)),
                            parallelOptions,
                            async (pair, cancellationToken) =>
                            {
                                var (room, proxy) = pair;
                                var stopwatch = Stopwatch.StartNew();
                                stopwatches.Add(stopwatch);

                                try
                                {
                                    await room.UpdateRoomData(i, proxy);
                                    stopwatch.Stop();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error updating room {room.Name}: {ex.Message}");
                                }
                            });


                        mainStopwatch.Stop();

                        try
                        {
                            var totalMiliseconds = mainStopwatch.ElapsedMilliseconds;
                            var totalMicroSeconds = totalMiliseconds * 1000;
                            var averageTime = stopwatches.Average(sw => sw.ElapsedMilliseconds);
                            //Screen.LogsPart.AddLog($"timeT {totalMiliseconds}, roomsT {Rooms.Count}, shard {Name}:{i} timeT/roomsT {Math.Round(Convert.ToDouble(totalMicroSeconds / Rooms.Count),2)}miS, avg time taken {Math.Round(averageTime)}ms");
                        }
                        catch (Exception)
                        {
                        }
                    }
                    LastSynceTime = syncTime;
                    isSyncing = false;
                }
                Time = timeResponse.Time;
            }
        }
    }
}
