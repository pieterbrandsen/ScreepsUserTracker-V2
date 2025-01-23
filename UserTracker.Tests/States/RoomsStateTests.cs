using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserTracker.Tests.Helper;
using UserTrackerShared.States;
using UserTrackerStates;

namespace UserTracker.Tests.States
{
    public class RoomsStateTests
    {
        [Fact]
        public void RoomState_InitializesCorrectly()
        {
            // Arrange
            var roomState = new RoomState("roomName", "shardName");            // Ensure this is properly initialized

            // Assert
            Assert.Equal("roomName", roomState.Name);
            Assert.Equal("shardName", roomState.Shard);
        }
    }
}
