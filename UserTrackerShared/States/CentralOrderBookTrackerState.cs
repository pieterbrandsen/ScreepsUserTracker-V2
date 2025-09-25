using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerShared.Models.ScreepsAPI;
using UserTrackerShared.Utilities;

namespace UserTrackerShared.States
{
    public static class CentralOrderBookTrackerState
    {
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.CentralOrderBookTracker);
        public static Dictionary<long, Dictionary<string, MarketOrderBook>> TickShardMarketOrderbookPairs { get; set; } = new();
        public static Dictionary<long, Dictionary<string, Dictionary<string, Dictionary<string, int>>>> TickShardRoomStorePairs { get; set; } = new();

        public static void UpdateTerminalRoomStore(string shardName, string roomName, ScreepsRoomHistory screepsRoomHistory)
        {
            if (screepsRoomHistory.Structures.Terminals.Count == 0) return;
            var terminal = screepsRoomHistory.Structures.Terminals.First().Value;
            var tick = screepsRoomHistory.Tick;

            TickShardRoomStorePairs[tick] = TickShardRoomStorePairs.GetValueOrDefault(tick, new Dictionary<string, Dictionary<string, Dictionary<string, int>>>());
            TickShardRoomStorePairs[tick][shardName] = TickShardRoomStorePairs[tick].GetValueOrDefault(shardName, new Dictionary<string, Dictionary<string, int>>());
            
            var terminalData = new Dictionary<string, int>();

            if (terminal.Store == null) return;
            var properties = typeof(Store).GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(terminal.Store);
                terminalData[property.Name] = value != null ? (int)value : 0;
            }
            TickShardRoomStorePairs[tick][shardName][roomName] = terminalData;
        }

        public static void UpdateMarketOrderBook(MarketOrderBook marketOrderBook)
        {
            var shardName = marketOrderBook.Shard;
            var tick = marketOrderBook.EstimatedTick;

            if (!TickShardMarketOrderbookPairs.TryGetValue(tick, out var shardDict))
            {
                shardDict = new Dictionary<string, MarketOrderBook>();
                TickShardMarketOrderbookPairs[tick] = shardDict;
            }
            shardDict[shardName] = marketOrderBook;
        }
        private static void PruneOldMarketOrderBooks(string shardName, long lastTick)
        {
            var ticksToRemove = TickShardMarketOrderbookPairs
                           .Where(kvp => kvp.Value.ContainsKey(shardName) && kvp.Key <= lastTick)
                           .Select(kvp => kvp.Key)
                           .OrderByDescending(t => t)
                           .Skip(1)
                           .ToList();

            foreach (var oldTick in ticksToRemove)
            {
                TickShardMarketOrderbookPairs[oldTick].Remove(shardName);
                if (TickShardMarketOrderbookPairs[oldTick].Count == 0)
                    TickShardMarketOrderbookPairs.Remove(oldTick);
            }
        }

        public static void TryFindMatchBetweenOrderBookAndTerminalData(string shard, long startTick)
        {
            var lastTick = startTick + ConfigSettingsState.TicksInFile;
            List<MarketOrderBookItem> prevBuy = null;
            List<MarketOrderBookItem> prevSell = null;
            long? prevTick = null;

            for (long tick = startTick; tick <= lastTick; tick++)
            {
                if (TickShardMarketOrderbookPairs.TryGetValue(tick, out var shardDict) &&
                    shardDict.TryGetValue(shard, out var orderBook))
                {
                    var buy = orderBook.Buy ?? new List<MarketOrderBookItem>();
                    var sell = orderBook.Sell ?? new List<MarketOrderBookItem>();

                    if (prevBuy != null && prevSell != null && prevTick.HasValue)
                    {
                        if (!OrderListEquals(prevBuy, buy) || !OrderListEquals(prevSell, sell))
                        {
                            // Change detected between prevTick and tick
                            OnOrderBookChanged(shard, prevTick.Value + 1, tick, buy, sell, prevBuy, prevSell);
                        }
                    }

                    prevBuy = buy;
                    prevSell = sell;
                    prevTick = tick;
                }
            }

            PruneOldMarketOrderBooks(shard, lastTick);
        }

        // Helper to compare two lists of MarketOrderBookItem (by id or relevant fields)
        private static bool OrderListEquals(List<MarketOrderBookItem> a, List<MarketOrderBookItem> b)
        {
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;
            // Compare by a unique property like OrderId (replace "Id" with your actual property)
            return a.OrderBy(x => x.Id).SequenceEqual(b.OrderBy(x => x.Id));
        }

        // Notification function for order book changes, now with tick range
        private static void OnOrderBookChanged(
            string shard, long fromTick, long toTick,
            List<MarketOrderBookItem> currentBuy, List<MarketOrderBookItem> currentSell,
            List<MarketOrderBookItem> prevBuy, List<MarketOrderBookItem> prevSell)
        {
            var roomStoreDataInRange = new Dictionary<long, Dictionary<string, Dictionary<string, int>>>();

            for (long tick = fromTick; tick <= toTick; tick++)
            {
                if (TickShardRoomStorePairs.TryGetValue(tick, out var shardDict) &&
                    shardDict.TryGetValue(shard, out var roomDict))
                {
                    roomStoreDataInRange[tick] = roomDict;
                }
            }

            // Helper to process a list (buy or sell)
            void ProcessOrderList(List<MarketOrderBookItem> current, List<MarketOrderBookItem> previous)
            {
                var prevDict = previous.ToDictionary(o => o.Id, o => o);

                foreach (var order in current)
                {
                    if (prevDict.TryGetValue(order.Id, out var prevOrder))
                    {
                        // Calculate the difference in filled amount
                        var prevFilled = prevOrder.Amount - prevOrder.RemainingAmount;
                        var currFilled = order.Amount - order.RemainingAmount;
                        var diff = currFilled - prevFilled;

                        if (diff != 0 && !string.IsNullOrEmpty(order.ResourceType))
                        {
                            // Try to find the diff in one of the rooms' terminal stores
                            foreach (var tickKvp in roomStoreDataInRange)
                            {
                                foreach (var roomKvp in tickKvp.Value)
                                {
                                    var roomName = roomKvp.Key;
                                    var store = roomKvp.Value;
                                    // Check for the specific resource type
                                    if (store.TryGetValue(order.ResourceType, out var amount) && amount == diff)
                                    {
                                        var credits = diff * order.Price;
                                        _logger.Information(
                                            "[Match Found] Order {OrderId} | Room {OrderRoom} | Resource {Resource} | Diff {Diff} | Credits {Credits} | Matched Room {MatchedRoom} | Tick {Tick}",
                                            order.Id,
                                            order.RoomName,
                                            order.ResourceType,
                                            diff,
                                            credits,
                                            roomName,
                                            tickKvp.Key
                                        );
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Process buy and sell lists
            ProcessOrderList(currentBuy, prevBuy);
            ProcessOrderList(currentSell, prevSell);
        }
    }
}
