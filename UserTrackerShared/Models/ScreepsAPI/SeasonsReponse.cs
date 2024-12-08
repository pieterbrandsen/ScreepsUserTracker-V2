using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTrackerShared.Models.ScreepsAPI
{
    public class Season
    {
        [JsonProperty("_id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
    public class SeasonsReponse
    {
        [JsonProperty("ok")]
        public int Ok { get; set; }

        [JsonProperty("seasons")]
        public List<Season> Seasons { get; set; }
    }
}
