using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UserTrackerShared.Models.ScreepsAPI
{
    public class SignInResponse
    {
        [JsonProperty("ok")]
        public int Ok { get; set; }

        [JsonProperty("token")]
        public required string Token { get; set; }
    }
}
