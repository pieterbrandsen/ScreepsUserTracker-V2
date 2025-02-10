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
    public class GameStateIntegrationTests
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
    }
}