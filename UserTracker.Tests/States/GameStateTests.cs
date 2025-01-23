using Castle.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using UserTracker.Tests.Helper;
using UserTrackerScreepsApi;
using UserTrackerShared;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models.ScreepsAPI;
using UserTrackerShared.States;
using Xunit;

namespace UserTracker.Tests.States
{
    public class GameStateIntegrationTests : IDisposable
    {
        public GameStateIntegrationTests()
        {
        }

        public void Dispose()
        {
            // Clean up after each test
            GameState.CurrentLeaderboard.Clear();
            GameState.ProxyStates.Clear();
            GameState.Shards.Clear();
        }

        [Fact]
        public async Task Init_PrivateServer_SuccessfullyInitializes()
        {
            // Arrange
            // Ensure ProxyHelper and ScreepsAPI are functional
            // (You may need to set up a test environment for these dependencies)

            // Act
            SetConfigurationManager.SetPrivateConfig();
            GameState.Init();

            // Wait for async operations to complete
            await Task.Delay(1000);

            // Assert
            Assert.NotNull(GameState.Shards);
            Assert.Single(GameState.Shards); // Only one shard for private server
        }

        [Fact]
        public async Task GetAvailableProxyAsync_ReturnsAvailableProxy()
        {
            // Arrange
            // Ensure ProxyHelper returns valid proxy IPs
            var proxyIps = new List<string> { "192.168.1.1", "192.168.1.2" };
            ProxyHelper.Proxies = proxyIps;

            SetConfigurationManager.SetLiveConfig();
            GameState.Init();

            // Act
            var proxy = await GameState.GetAvailableProxyAsync();

            // Assert
            Assert.NotNull(proxy);
            Assert.True(proxy.InUse);
        }

        [Fact]
        public void GetAvailableProxies_ReturnsAvailableProxies()
        {
            // Arrange
            // Ensure ProxyHelper returns valid proxy IPs
            var proxyIps = new List<string> { "192.168.1.1", "192.168.1.2" };
            ProxyHelper.Proxies = proxyIps;

            SetConfigurationManager.SetLiveConfig();
            GameState.Init();

            // Act
            var proxies = GameState.GetAvailableProxies(2);

            // Assert
            Assert.Equal(proxyIps.Count, proxies.Count);
            Assert.All(proxies, p => Assert.True(p.InUse));
        }

        [Fact]
        public async Task OnSetTimeTimer_UpdatesCurrentLeaderboard()
        {
            // Arrange
            // Ensure ScreepsAPI returns a valid leaderboard response
            var leaderboardResponse = new List<SeaonListItem> { new SeaonListItem() };
            // You may need to modify ScreepsAPI to return this response for testing

            SetConfigurationManager.SetLiveConfig();
            GameState.Init();

            // Wait for timer to trigger
            await Task.Delay(60*1000);

            // Assert
            Assert.NotNull(GameState.CurrentLeaderboard);
            Assert.NotEmpty(GameState.CurrentLeaderboard);
        }
    }
}