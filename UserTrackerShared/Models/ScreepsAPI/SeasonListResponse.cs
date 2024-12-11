using Newtonsoft.Json;

namespace UserTrackerShared.Models.ScreepsAPI
{
    public class SeaonListItem
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("season")]
        public string Season { get; set; }
        [JsonProperty("user")]
        public string User { get; set; }
        [JsonProperty("score")]
        public string Score { get; set; }
        [JsonProperty("rank")]
        public int Rank { get; set; }
    }
    public class SeasonListUser : ScreepsUser
    {
        [JsonProperty("gcl")]
        public long GCL { get; set; }
    }
    public class SeasonListResponse
    {
        [JsonProperty("ok")]
        public int Ok { get; set; }
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("list")]
        public List<SeaonListItem> List { get; set; }
        [JsonProperty("users")]
        public Dictionary<string, SeasonListUser> Users { get; set; }
    }
}
