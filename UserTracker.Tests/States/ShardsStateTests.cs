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
            
            // Act
            var shardState = new ShardState("shardName");
            await shardState.StartUpdate();

            // Assert
            Assert.Equal("shardName", shardState.Name);
            Assert.True(shardState.Time > 0); // Check if time is set correctly
        }

        [Fact]
        public async Task OnSetTimeTimer_UpdatesTimeCorrectly()
        {
            // Arrange
            SetConfigurationManager.SetPrivateConfig();

            // Act
            var shardState = new ShardState("shardName");
            await shardState.StartUpdate();

            // Assert
            Assert.True(shardState.Time > 0); // Check if time is updated correctly
        }
    }
}
