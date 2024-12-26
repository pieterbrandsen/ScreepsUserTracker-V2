using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserTrackerScreepsApi;

namespace UserTrackerStates
{
    public class ProxyState
    {
        public Uri Uri { get; set; }
        public ProxyState(Uri uri)
        {
            Uri = uri;
        }

        public void SetUsed()
        {
            InUse = true;
        }
        public void SetFree()
        {
            InUse = false;
        }

        private int lastRequest = DateTime.Now.Millisecond;
        public bool InUse = false;

        public async Task<JObject?> GetHistory(string shard, string room, long tick)
        {
            long currentTicks = DateTime.Now.Ticks;
            long elapsedTicks = currentTicks - lastRequest;
            long delayTicks = TimeSpan.FromMilliseconds(500).Ticks;

            if (elapsedTicks < delayTicks)
            {
                await Task.Delay(TimeSpan.FromTicks(delayTicks - elapsedTicks));
            }

            var result = await ScreepsAPI.GetHistory(shard, room, tick, Uri);
            SetFree();
            return result;
        }
    }
}
