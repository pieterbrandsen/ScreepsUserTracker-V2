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

        [Fact]
        public async Task Init_PrivateServer_SuccessfullyInitializes()
        {
            // Arrange
            SetConfigurationManager.SetPrivateConfig();

            // Act
            var gameState = new GameState();
            await gameState.InitAsync();

            // Assert
            Assert.NotNull(gameState.Shards);
            Assert.Single(gameState.Shards); // Only one shard for private server
        }

        [Fact]
        public async Task GetAvailableProxyAsync_ReturnsAvailableProxy()
        {
            // Arrange
            // Ensure ProxyHelper returns valid proxy IPs
            var proxyIps = new List<string> { "192.168.1.1", "192.168.1.2" };
            ProxyHelper.Proxies = proxyIps;

            SetConfigurationManager.SetLiveConfig();
            var gameState = new GameState();
            await gameState.InitAsync();

            // Act
            var proxy = await GameState.GetAvailableProxyAsync();

            // Assert
            Assert.NotNull(proxy);
            Assert.True(proxy.InUse);
        }

        [Fact]
        public async Task GetAvailableProxies_ReturnsAvailableProxies()
        {
            // Arrange
            // Ensure ProxyHelper returns valid proxy IPs
            var proxyIps = new List<string> { "192.168.1.1", "192.168.1.2" };
            ProxyHelper.Proxies = proxyIps;

            SetConfigurationManager.SetLiveConfig();
            var gameState = new GameState();
            await gameState.InitAsync();

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
            SetConfigurationManager.SetLiveConfig();

            // Act
            var gameState = new GameState();
            await gameState.UpdateLeaderboard();

            // Assert
            Assert.NotNull(gameState.CurrentLeaderboard);
            Assert.NotEmpty(gameState.CurrentLeaderboard);
        }

        public void Dispose()
        {
            GameState.ProxyStates.Clear();
        }
    }
}