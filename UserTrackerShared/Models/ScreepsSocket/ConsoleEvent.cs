namespace UserTrackerShared.Models.ScreepsAPI
{
    public class ConsoleEvent
    {
        [JsonPropertyName("messages")]
        public ConsoleMessages? Messages { get; set; }

        [JsonPropertyName("shard")]
        public string? Shard { get; set; }
    }
}
