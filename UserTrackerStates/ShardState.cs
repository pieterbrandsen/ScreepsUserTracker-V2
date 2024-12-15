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
                        foreach (var room in Rooms)
                        {
                            await room.UpdateRoomData(i);
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
