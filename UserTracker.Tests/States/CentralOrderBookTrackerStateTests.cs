using UserTrackerShared.Models.ScreepsAPI;
using UserTrackerShared.States;

namespace UserTracker.Tests.States
{
    public class CentralOrderBookTrackerStateTests
    {
        [Fact]
        public void TryFindMatchBetweenOrderBookAndTerminalData_GapInOrderbook_ShouldStillMatch()
        {
            // Arrange
            string shard = "shard0";
            string room = "W1N1";
            string resourceType = "energy";
            string orderId = "order123";
            long firstTick = 1000;
            long secondTick = 1001;
            long thirdTick = 1002;
            ConfigSettingsState.TicksInFile = 3;

            CentralOrderBookTrackerState.TickShardMarketOrderbookPairs.Clear();
            CentralOrderBookTrackerState.TickShardRoomStorePairs.Clear();

            // Tick 1: order initialized (filled 100)
            var order1 = new MarketOrderBookItem
            {
                Id = orderId,
                Amount = 1000,
                RemainingAmount = 900,
                ResourceType = resourceType
            };
            var orderBook1 = new MarketOrderBook
            {
                Shard = shard,
                EstimatedTick = firstTick,
                Buy = new List<MarketOrderBookItem> { order1 },
                Sell = new List<MarketOrderBookItem>()
            };
            CentralOrderBookTrackerState.TickShardMarketOrderbookPairs[firstTick] = new Dictionary<string, MarketOrderBook>
            {
                [shard] = orderBook1
            };
            CentralOrderBookTrackerState.TickShardRoomStorePairs[firstTick] = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>
            {
                [shard] = new Dictionary<string, Dictionary<string, int>>
                {
                    [room] = new Dictionary<string, int>
                    {
                        [resourceType] = 0
                    }
                }
            };

            // Tick 2: (orderbook missing, gap)
            // No orderbook for secondTick

            // Terminal store for secondTick, with matching diff
            CentralOrderBookTrackerState.TickShardRoomStorePairs[secondTick] = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>
            {
                [shard] = new Dictionary<string, Dictionary<string, int>>
                {
                    [room] = new Dictionary<string, int>
                    {
                        [resourceType] = 100
                    }
                }
            };

            // Tick 3: order filled 300
            var order3 = new MarketOrderBookItem
            {
                Id = orderId,
                Amount = 1000,
                RemainingAmount = 700,
                ResourceType = resourceType
            };
            var orderBook3 = new MarketOrderBook
            {
                Shard = shard,
                EstimatedTick = thirdTick,
                Buy = new List<MarketOrderBookItem> { order3 },
                Sell = new List<MarketOrderBookItem>()
            };
            CentralOrderBookTrackerState.TickShardMarketOrderbookPairs[thirdTick] = new Dictionary<string, MarketOrderBook>
            {
                [shard] = orderBook3
            };
            CentralOrderBookTrackerState.TickShardRoomStorePairs[thirdTick] = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>
            {
                [shard] = new Dictionary<string, Dictionary<string, int>>
                {
                    [room] = new Dictionary<string, int>
                    {
                        [resourceType] = 100
                    }
                }
            };

            // Act
            CentralOrderBookTrackerState.TryFindMatchBetweenOrderBookAndTerminalData(shard, firstTick);
        }

        [Fact]
        public void TryFindMatchBetweenOrderBookAndTerminalData_GapInTerminalData_ShouldStillMatch()
        {
            // Arrange
            string shard = "shard0";
            string room = "W1N1";
            string resourceType = "energy";
            string orderId = "order123";
            long firstTick = 1000;
            long secondTick = 1001;
            long thirdTick = 1002;
            ConfigSettingsState.TicksInFile = 3;

            CentralOrderBookTrackerState.TickShardMarketOrderbookPairs.Clear();
            CentralOrderBookTrackerState.TickShardRoomStorePairs.Clear();

            // Tick 1: order initialized (filled 100)
            var order1 = new MarketOrderBookItem
            {
                Id = orderId,
                Amount = 1000,
                RemainingAmount = 900,
                ResourceType = resourceType
            };
            var orderBook1 = new MarketOrderBook
            {
                Shard = shard,
                EstimatedTick = firstTick,
                Buy = new List<MarketOrderBookItem> { order1 },
                Sell = new List<MarketOrderBookItem>()
            };
            CentralOrderBookTrackerState.TickShardMarketOrderbookPairs[firstTick] = new Dictionary<string, MarketOrderBook>
            {
                [shard] = orderBook1
            };
            CentralOrderBookTrackerState.TickShardRoomStorePairs[firstTick] = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>
            {
                [shard] = new Dictionary<string, Dictionary<string, int>>
                {
                    [room] = new Dictionary<string, int>
                    {
                        [resourceType] = 0
                    }
                }
            };

            // Tick 2: order filled 200, but terminal data missing (gap)
            var order2 = new MarketOrderBookItem
            {
                Id = orderId,
                Amount = 1000,
                RemainingAmount = 800,
                ResourceType = resourceType
            };
            var orderBook2 = new MarketOrderBook
            {
                Shard = shard,
                EstimatedTick = secondTick,
                Buy = new List<MarketOrderBookItem> { order2 },
                Sell = new List<MarketOrderBookItem>()
            };
            CentralOrderBookTrackerState.TickShardMarketOrderbookPairs[secondTick] = new Dictionary<string, MarketOrderBook>
            {
                [shard] = orderBook2
            };
            // No terminal data for secondTick

            // Tick 3: order filled 300
            var order3 = new MarketOrderBookItem
            {
                Id = orderId,
                Amount = 1000,
                RemainingAmount = 700,
                ResourceType = resourceType
            };
            var orderBook3 = new MarketOrderBook
            {
                Shard = shard,
                EstimatedTick = thirdTick,
                Buy = new List<MarketOrderBookItem> { order3 },
                Sell = new List<MarketOrderBookItem>()
            };
            CentralOrderBookTrackerState.TickShardMarketOrderbookPairs[thirdTick] = new Dictionary<string, MarketOrderBook>
            {
                [shard] = orderBook3
            };
            CentralOrderBookTrackerState.TickShardRoomStorePairs[thirdTick] = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>
            {
                [shard] = new Dictionary<string, Dictionary<string, int>>
                {
                    [room] = new Dictionary<string, int>
                    {
                        [resourceType] = 100
                    }
                }
            };

            // Act
            CentralOrderBookTrackerState.TryFindMatchBetweenOrderBookAndTerminalData(shard, firstTick);
        }
    }
}
