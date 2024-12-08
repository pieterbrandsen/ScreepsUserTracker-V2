using System.Timers;
using UserTrackerScreepsApi;
using Timer = System.Timers.Timer;

namespace UserTrackerShared.States
{
    public class ShardState
    {
        private static Timer? _setTimeTimer;

        public ShardState(string Name)
        {
            this.Name = Name;

            _setTimeTimer = new Timer(300000);
            _setTimeTimer.Elapsed += OnSetTimeTimer;
            _setTimeTimer.AutoReset = true;
            _setTimeTimer.Enabled = true;
            OnSetTimeTimer(null, null);
        }

        public string Name { get; set; }
        public long Time { get; set; }

        private async void OnSetTimeTimer(Object? source, ElapsedEventArgs e)
        {
            var timeResponse = await ScreepsAPI.GetTimeOfShard(Name);
            if (timeResponse != null)
            {
                Time = timeResponse.Time;
            }
        }
    }
}
