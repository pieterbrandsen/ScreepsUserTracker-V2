using Newtonsoft.Json;
using System.Text.Json;

namespace UserTrackerShared.Models.ScreepsAPI
{
    public class ConsoleEvent
    {
        [JsonProperty("messages")]
        public ConsoleMessages? Messages { get; set; }

        [JsonProperty("shard")]
        public string? Shard { get; set; }
    }
}
