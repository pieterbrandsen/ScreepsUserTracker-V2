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
                    if (LastSynceTime == 0) LastSynceTime = syncTime - 100 * 1000;
                    for (long i = LastSynceTime; i < syncTime; i += 100)
                    {
                        var stopwatch2 = Stopwatch.StartNew();
                        var stopwatches = new List<Stopwatch>();

                        var roomsCount = Rooms.Count;
                        List<Task> updateTasks = new List<Task>();

                        var roomsChecked = 0;
                        var roomNamesToBeChecked = Rooms.Select(room => room.Name).ToArray();
                        while (roomsChecked < roomsCount)
                        {
                            var proxiesAvailable = GameState.GetAvailableProxiesAsync(roomNamesToBeChecked.Length);
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
                        stopwatch2.Stop();

                        var totalTime = stopwatch2.ElapsedMilliseconds;
                        var averageTime = stopwatches.Average(sw => sw.ElapsedMilliseconds);
                        Screen.LogsPart.AddLog($"time {totalTime} rooms {Rooms.Count} shard {Name} time/time {totalTime/ Rooms.Count} ms / Average time taken per request: {averageTime} ms");
                    }
                    LastSynceTime = syncTime;
                    isSyncing = false;
                }
                Time = timeResponse.Time;
            }
        }
    }
}
