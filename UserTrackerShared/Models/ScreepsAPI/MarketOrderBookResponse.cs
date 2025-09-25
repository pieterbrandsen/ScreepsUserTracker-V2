using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTrackerShared.Models.ScreepsAPI
{
    public partial class MarketOrderBookItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("amount")]
        public int Amount { get; set; }
        [JsonProperty("remainingAmount")]
        public int RemainingAmount { get; set; }
        [JsonProperty("price")]
        public double Price { get; set; }
        [JsonProperty("roomName")]
        public string RoomName { get; set; }
        public string ResourceType { get; set; }
    }
    public class MarketOrderBook
    {
        public string Shard { get; set; }
        public long EstimatedTick { get; set; }
        public List<MarketOrderBookItem> Buy { get; set; } = new();
        public List<MarketOrderBookItem> Sell { get; set; } = new();
    }
    public class MarketOrderBookResponse
    {
        [JsonProperty("ok")]
        public int Ok { get; set; }
        [JsonProperty("list")]
        public List<MarketOrderBookItem> Items { get; set; }
    }
}
