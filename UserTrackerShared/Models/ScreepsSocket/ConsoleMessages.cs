namespace UserTrackerShared.Models.ScreepsAPI
{
    public class ConsoleMessages
    {
        [JsonPropertyName("log")]
        public List<string>? Log { get; set; }

        [JsonPropertyName("results")]
        public List<string>? Results { get; set; }
    }
}
