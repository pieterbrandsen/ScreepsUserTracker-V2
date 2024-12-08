using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UserTrackerShared.Models.ScreepsAPI
{
    public class TimeResponse
    {
        [JsonProperty("ok")]
        public int Ok { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }
    }
}
