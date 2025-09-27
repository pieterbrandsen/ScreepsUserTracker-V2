using Newtonsoft.Json;

namespace UserTrackerShared.Models.ScreepsAPI
{
    public class ConsoleMessages
    {
        [JsonProperty("log")]
        public List<string>? Log { get; set; } = new();

        [JsonProperty("results")]
        public List<string>? Results { get; set; } = new();
    }
}
