using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTrackerShared.Models.ScreepsAPI
{
    public class GetUserResponse
    {
        [JsonProperty("ok")]
        public int Ok { get; set; }
        [JsonProperty("user")]
        public ScreepsUser User { get; set; }
    }
}
