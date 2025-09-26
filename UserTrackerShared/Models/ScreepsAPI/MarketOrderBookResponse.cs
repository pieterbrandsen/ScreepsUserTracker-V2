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
        [JsonProperty("created")]
        public long Created { get; set; }
        [JsonProperty("createdTimestamp")]
        public long CreatedTimestamp { get; set; }
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
        [JsonProperty("resourceType")]
        public string ResourceType { get; set; }
    }
    public class MarketOrderBook
    {
        public MarketOrderBook(string shard, MarketOrderBookResponse response)
        {
            Shard = shard;
            Tick = response.Tick;
            Buy = response.Orders.Where(o => o.Type == "buy").ToList();
            Sell = response.Orders.Where(o => o.Type == "sell").ToList();
        }
        public string Shard { get; set; }
        public long Tick { get; set; }
        public List<MarketOrderBookItem> Buy { get; set; } = new();
        public List<MarketOrderBookItem> Sell { get; set; } = new();
    }
    public class MarketOrderBookResponse
    {
        [JsonProperty("tick")]
        public long Tick { get; set; }
        [JsonProperty("orders")]
        public List<MarketOrderBookItem> Orders { get; set; }
    }
}
