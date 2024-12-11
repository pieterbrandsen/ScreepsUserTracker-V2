using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UserTrackerShared.Models.ScreepsAPI
{
    public class MapStatRoomOwn
    {
        [JsonProperty("user")]
        public string User { get; set; }
        [JsonProperty("level")]
        public int Level { get; set; }
    }
    public class MapStatRoomSign
    {
        [JsonProperty("user")]
        public string User { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("time")]
        public long Time { get; set; }
        [JsonProperty("datetime")]
        public long DateTime { get; set; }

    }
    public class MapStatRoom
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("respawnArea")]
        public long? RespawnArea { get; set; }
        [JsonProperty("own")]
        public MapStatRoomOwn Own { get; set; }
        [JsonProperty("sign")]
        public MapStatRoomSign Sign { get; set; }
        [JsonProperty("decorations")]
        public object Decorations { get; set; }
        [JsonProperty("isPowerEnabled")]
        public bool IsPowerEnabled { get; set; }
    }
    public class MapStatUserBadge
    {
        [JsonProperty("type")]
        public object Type { get; set; }
        [JsonProperty("color1")]
        public string Color1 { get; set; }
        [JsonProperty("color2")]
        public string Color2 { get; set; }
        [JsonProperty("color3")]
        public string Color3 { get; set; }
        [JsonProperty("param")]
        public int Param { get; set; }
        [JsonProperty("decoration")]
        public string Decoration { get; set; }
        [JsonProperty("flip")]
        public bool Flip { get; set; }
    }
    public class MapStatUser : ScreepsUser
    {
    }

    public class MapStatsResponse
    {
        [JsonProperty("ok")]
        public int Ok { get; set; }
        [JsonProperty("gameTime")]
        public long GameTime { get; set; }
        [JsonProperty("stats")]
        public Dictionary<string, MapStatRoom> Rooms { get; set; } = new Dictionary<string, MapStatRoom>();
        [JsonProperty("decorations")]
        public object Decorations { get; set; }
        [JsonProperty("users")]
        public Dictionary<string, MapStatUser> Users { get; set; } = new Dictionary<string, MapStatUser>();
    }
}
