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
    public class SeasonListResponse
    {
        [JsonProperty("ok")]
        public int Ok { get; set; }
        [JsonProperty("list")]
        public List<SeaonListItem> List { get; set; }
    }
}
