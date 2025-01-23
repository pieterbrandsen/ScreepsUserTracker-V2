using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
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

        private async void StartSync(long time)
        {
            if (isSyncing) return;
            isSyncing = true;

            var syncTime = Convert.ToInt32(Math.Round(Convert.ToDouble((time - 500) / 100)) * 100);
            if (LastSynceTime == 0) LastSynceTime = syncTime - 100 * 100;

            for (long i = LastSynceTime; i < syncTime; i += 100)
            {
                var mainStopwatch = Stopwatch.StartNew();
                var stopwatches = new ConcurrentBag<Stopwatch>();

                var roomsCount = Rooms.Count;
                List<Task> updateTasks = new List<Task>();

                var roomsChecked = 0;
                var roomNamesToBeChecked = Rooms.Select(room => room.Name).ToArray();
                while (roomsChecked < roomsCount)
                {
                    var proxiesAvailable = GameState.GetAvailableProxies(roomNamesToBeChecked.Length);
                    var checkingRooms = roomNamesToBeChecked.Take(proxiesAvailable.Count).ToArray();
                    roomNamesToBeChecked = roomNamesToBeChecked.Skip(proxiesAvailable.Count).ToArray();

                    for (var j = 0; j < proxiesAvailable.Count; j++)
                    {
                        var roomName = checkingRooms[j];
                        var room = Rooms.First(r => r.Name == roomName);
                        var proxy = proxiesAvailable[j];
                        var stopwatch = Stopwatch.StartNew();
                        stopwatches.Add(stopwatch);
                        var task = Task.Run(async () =>
                        {
                            await room.UpdateRoomData(i, proxy);
                            stopwatch.Stop();
                        });
                        updateTasks.Add(task);

                        roomsChecked++;
                    }
                    await Task.Delay(100);
                }
                await Task.WhenAll(updateTasks);



                try
                {
                    mainStopwatch.Stop();
                    var totalMiliseconds = mainStopwatch.ElapsedMilliseconds;
                    var totalMicroSeconds = totalMiliseconds * 1000;
                    var averageTime = stopwatches.Average(sw => sw.ElapsedMilliseconds);
                    Screen.AddLog($"timeT {totalMiliseconds}, roomsT {Rooms.Count}, shard {Name}:{i} timeT/roomsT {Math.Round(Convert.ToDouble(totalMicroSeconds / Rooms.Count), 2)}miS, avg time taken {Math.Round(averageTime)}ms");
                }
                catch (Exception)
                {
                }
            }
            LastSynceTime = syncTime;
            isSyncing = false;
        }

        public async Task StartUpdate()
        {
            var timeResponse = await ScreepsAPI.GetTimeOfShard(Name);
            if (timeResponse != null)
            {
                if (Time != timeResponse.Time)
                {
                    StartSync(timeResponse.Time);
                }
                Time = timeResponse.Time;
            }
        }
        private async void OnSetTimeTimer(Object? source, ElapsedEventArgs? e)
        {
            await StartUpdate();
        }
    }
}
