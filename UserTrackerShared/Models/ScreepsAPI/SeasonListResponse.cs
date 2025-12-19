using Newtonsoft.Json;

namespace UserTrackerShared.Models.ScreepsAPI
{
    public class SeasonListItem
    {
        public string Type { get; set; } = null!;
        [JsonProperty("_id")]
        public string Id { get; set; } = null!;

        [JsonProperty("season")]
        public string Season { get; set; } = null!;
        [JsonProperty("user")]
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        [JsonProperty("score")]
        public long Score { get; set; }
        [JsonProperty("rank")]
        public int Rank { get; set; }
        public DateTime Timestamp { get; set; }
    }
    public class SeasonListResponse
    {
        [JsonProperty("ok")]
        public int Ok { get; set; }
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("list")]
        public required List<SeasonListItem> List { get; set; }
        [JsonProperty("users")]
        public required Dictionary<string, ScreepsUser> Users { get; set; }
    }
}
