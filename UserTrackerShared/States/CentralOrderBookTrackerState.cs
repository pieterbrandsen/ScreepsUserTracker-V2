using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerShared.Models.ScreepsAPI;

namespace UserTrackerShared.States
{
    public static class CentralOrderBookTrackerState
    {
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.CentralOrderBookTracker);
        private static readonly Serilog.ILogger _logger2 = Logger.GetLogger(LogCategory.CentralOrderBookTracker2);
        public static ConcurrentDictionary<long, ConcurrentDictionary<string, MarketOrderBook>> TickShardMarketOrderbookPairs { get; set; } = new();
        public static ConcurrentDictionary<long, ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, int>>>> TickShardRoomStorePairs { get; set; } = new();

        private static Dictionary<(string, string), int> GetStoreChanges(Dictionary<string, Dictionary<string, object?>> historyChanges)
        {
            var allStoreChanges = new Dictionary<(string, string), int>();
            foreach (var changeKvp in historyChanges)
            {
                var id = changeKvp.Key;
                var storeChanges = changeKvp.Value.Where(s => s.Key.StartsWith("store."));
                foreach (var storeChange in storeChanges)
                {
                    var resource = storeChange.Key.Substring("store.".Length);
                    var change = Convert.ToInt32(storeChange.Value);
                    var key = (id, resource);
                    allStoreChanges[key] = change;
                }
            }
            return allStoreChanges;
        }

        public static void UpdateTerminalRoomStore(string shardName, string roomName, ScreepsRoomHistory prevScreepsRoomHistory, ScreepsRoomHistory screepsRoomHistory)
        {
            if (screepsRoomHistory.Structures.Terminals.Count == 0) return;

            var currTerminal = screepsRoomHistory.Structures.Terminals.Values.First();
            var prevTerminal = prevScreepsRoomHistory.Structures.Terminals.Values.FirstOrDefault(t => t.Id == currTerminal.Id);

            var changes = new Dictionary<(string, string), int>();
            var currAllStoreChanges = GetStoreChanges(screepsRoomHistory.HistoryChangesDictionary);
            // in creep in range check for if any combinations between prev and curr store get back to terminal total store. 


            var tick = screepsRoomHistory.Tick;
            TickShardRoomStorePairs[tick] = TickShardRoomStorePairs.GetValueOrDefault(tick, new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, int>>>());
            TickShardRoomStorePairs[tick][shardName] = TickShardRoomStorePairs[tick].GetValueOrDefault(shardName, new ConcurrentDictionary<string, ConcurrentDictionary<string, int>>());

            var creepsInRange = screepsRoomHistory.Creeps.OwnedCreeps.Where(c => (c.Value.X - currTerminal.X) * -1 <= 2 && (c.Value.Y - currTerminal.Y) * -1 <= 2);
            var currCreepsInRange = screepsRoomHistory.Creeps.OwnedCreeps.Where(c => (c.Value.X - currTerminal.X) * -1 <= 2 && (c.Value.Y - currTerminal.Y) * -1 <= 2);

            var terminalStoreChanges = new ConcurrentDictionary<string, int>();
            if (currTerminal.Store == null || prevTerminal == null) return;
            var properties = typeof(Store).GetProperties();
            foreach (var property in properties)
            {
                var currValue = Convert.ToInt32(property.GetValue(currTerminal.Store) ?? 0);
                var prevValue = Convert.ToInt32(property.GetValue(prevTerminal.Store) ?? 0);
                var value = currValue - prevValue;
                if (value == 0) continue;

                terminalStoreChanges[property.Name] = value;
            }

            TickShardRoomStorePairs[tick][shardName][roomName] = terminalStoreChanges;
        }

        public static void UpdateMarketOrderBook(MarketOrderBook marketOrderBook)
        {
            var shardName = marketOrderBook.Shard;
            var tick = marketOrderBook.Tick;

            if (!TickShardMarketOrderbookPairs.TryGetValue(tick, out var shardDict))
            {
                shardDict = new ConcurrentDictionary<string, MarketOrderBook>();
                TickShardMarketOrderbookPairs[tick] = shardDict;
            }

            _logger2.Information("Updated Market Order Book | Tick: {Tick}, Shard: {Shard}, BuyCount: {BuyCount}, SellCount: {SellCount}",
                tick, shardName, marketOrderBook.Buy?.Count ?? 0, marketOrderBook.Sell?.Count ?? 0);
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
                if (TickShardMarketOrderbookPairs.TryGetValue(oldTick, out var shardDict))
                {
                    shardDict.TryRemove(shardName, out _);
                    if (shardDict.IsEmpty)
                        TickShardMarketOrderbookPairs.TryRemove(oldTick, out _);
                }
            }
        }

        public static void TryFindMatchBetweenOrderBookAndTerminalData(string shard, long startTick)
        {
            var lastTick = startTick + ConfigSettingsState.TicksInFile;
            var (prevBuy, prevSell, prevTick) = GetPreviousOrderBookState(shard, startTick);

            for (long tick = startTick; tick <= lastTick; tick++)
            {
                if (TickShardMarketOrderbookPairs.TryGetValue(tick, out var shardDict) &&
                    shardDict.TryGetValue(shard, out var orderBook))
                {
                    var buy = orderBook.Buy ?? new List<MarketOrderBookItem>();
                    var sell = orderBook.Sell ?? new List<MarketOrderBookItem>();

                    if (prevBuy != null && prevSell != null && prevTick.HasValue && tick - 1 == prevTick)
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
            var roomStoreDataInRange = new ConcurrentDictionary<long, ConcurrentDictionary<string, ConcurrentDictionary<string, int>>>();

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
                        var diff = currFilled - prevFilled * (order.Type == "buy" ? -1 : 1);

                        if (diff != 0 && !string.IsNullOrEmpty(order.ResourceType))
                        {
                            // Try to find the diff in one of the rooms' terminal stores
                            foreach (var tickKvp in roomStoreDataInRange)
                            {
                                foreach (var roomKvp in tickKvp.Value)
                                {
                                    var roomName = roomKvp.Key;
                                    var store = roomKvp.Value;
                                    // todo: remove 2500 check
                                    if (store.TryGetValue(order.ResourceType, out var amount) && amount == diff && amount > 2500)
                                    {
                                        var username = GameState.GetUsernameByRoom(shard, roomName) ?? "Unknown";
                                        var credits = diff * order.Price;
                                        _logger.Information(
                                            "[Match Found] Tick {Tick} | Order {Direction} {OrderId} ({Shard}/{Username}/{OrderRoom}/{Resource}) | Diff={Diff}, Credits={Credits}, MatchedRoom={MatchedRoom}",
                                            tickKvp.Key,
                                            order.Type,
                                            order.Id,
                                            shard,
                                            username,
                                            order.RoomName,
                                            order.ResourceType,
                                            diff,
                                            credits,
                                            roomName
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

        private static (List<MarketOrderBookItem>? prevBuy, List<MarketOrderBookItem>? prevSell, long? prevTick) GetPreviousOrderBookState(string shard, long startTick)
        {
            // Prefer startTick, otherwise use the last tick before startTick
            if (TickShardMarketOrderbookPairs.TryGetValue(startTick, out var startShardDict) &&
                startShardDict.TryGetValue(shard, out var startOrderBook))
            {
                return (
                    startOrderBook.Buy ?? new List<MarketOrderBookItem>(),
                    startOrderBook.Sell ?? new List<MarketOrderBookItem>(),
                    startTick
                );
            }
            else
            {
                var prevOrderBookTick = TickShardMarketOrderbookPairs
                    .Where(kvp => kvp.Key < startTick && kvp.Value.ContainsKey(shard))
                    .Select(kvp => kvp.Key)
                    .DefaultIfEmpty()
                    .Max();

                if (prevOrderBookTick != 0 && TickShardMarketOrderbookPairs.TryGetValue(prevOrderBookTick, out var prevShardDict)
                    && prevShardDict.TryGetValue(shard, out var prevOrderBook))
                {
                    return (
                        prevOrderBook.Buy ?? new List<MarketOrderBookItem>(),
                        prevOrderBook.Sell ?? new List<MarketOrderBookItem>(),
                        prevOrderBookTick
                    );
                }
            }
            return (null, null, null);
        }
    }
}
