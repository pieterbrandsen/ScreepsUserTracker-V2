using Newtonsoft.Json;

namespace UserTrackerShared.Models.ScreepsAPI
{
    public class SeaonListItem
    {
        public required string Type { get; set; }
        [JsonProperty("_id")]
        public required string Id { get; set; }

        [JsonProperty("season")]
        public required string Season { get; set; }
        [JsonProperty("user")]
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        [JsonProperty("score")]
        public int Score { get; set; }
        [JsonProperty("rank")]
        public int Rank { get; set; }
    }
    public class SeasonListResponse
    {
        [JsonProperty("ok")]
        public int Ok { get; set; }
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("list")]
        public required List<SeaonListItem> List { get; set; }
        [JsonProperty("users")]
        public required Dictionary<string, ScreepsUser> Users { get; set; }
    }
}
