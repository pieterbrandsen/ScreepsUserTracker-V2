using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UserTrackerShared.Models.ScreepsAPI
{
    public class OkResponse
    {
        [JsonProperty("ok")]
        public int Ok { get; set; }
    }
}
