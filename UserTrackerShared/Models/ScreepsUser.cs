using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserTrackerShared.Models.ScreepsAPI;

namespace UserTrackerShared.Models
{
    public class ScreepsUser
    {
        [JsonProperty("_id")]
        public string Id { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("gcl")]
        public long GCL { get; set; }
        [JsonProperty("power")]
        public long Power { get; set; }
        [JsonProperty("badge")]
        public MapStatUserBadge Badge { get; set; }
        public int GCLRank { get; set; }
        public int PowerRank { get; set; }
    }
}
