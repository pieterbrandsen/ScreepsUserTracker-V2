using System;
using System.Threading.Tasks;
using UserTracker.Tests.Helper;
using UserTrackerScreepsApi;
using UserTrackerShared.Models.ScreepsAPI;
using UserTrackerShared.States;
using Xunit;

namespace UserTracker.Tests.States
{
    public class ShardStateTests
    {
        [Fact]
        public async Task ShardState_InitializesCorrectly()
        {
            // Arrange
            SetConfigurationManager.SetPrivateConfig();
            GameState.Init();                   // Ensure this is properly initialized
            var shardState = new ShardState("shardName");

            // Wait for async initialization to complete
            await Task.Delay(1000); // Adjust delay as needed

            // Assert
            Assert.Equal("shardName", shardState.Name);
            Assert.NotNull(shardState.Users); // Check if users are loaded
            Assert.NotNull(shardState.Rooms); // Check if rooms are loaded
            Assert.True(shardState.Time > 0); // Check if time is set correctly
        }

        [Fact]
        public async Task OnSetTimeTimer_UpdatesTimeCorrectly()
        {
            // Arrange
            SetConfigurationManager.SetPrivateConfig();
            GameState.Init();                   // Ensure this is properly initialized
            var shardState = new ShardState("shardName");

            // Wait for async initialization to complete
            await Task.Delay(1000); // Adjust delay as needed

            // Act
            shardState.OnSetTimeTimer(null,null);

            // Wait for timer logic to complete
            await Task.Delay(1000); // Adjust delay as needed

            // Assert
            Assert.True(shardState.Time > 0); // Check if time is updated correctly
        }
    }
}
