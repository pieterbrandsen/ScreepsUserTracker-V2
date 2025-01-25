using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserTrackerShared.Helpers;

namespace UserTracker.Tests.RoomHistory
{
    public class RoomHistoryConverterTests
    {
        private static string HistoryFilesLocations = @"C:\Users\Pieter\source\repos\ScreepsUserTracker-V2\UserTracker.Tests\RoomHistory\Histories";
        
        [Fact]
        public void Test_OwnedEnemyCreepsCounting()
        {
            // Arrange
            var json = File.ReadAllText($@"{HistoryFilesLocations}\1.json");
            var jObject = JObject.Parse(json);

            // Act
            var history = ScreepsRoomHistoryComputedHelper.Compute(jObject);

            // Assert
            Assert.Single(history.Creeps.OwnedCreeps);
            Assert.Single(history.Creeps.EnemyCreeps);
        }

        [Fact]
        public void Test_OtherCreepsCounting()
        {
            // Arrange
            var json = File.ReadAllText($@"{HistoryFilesLocations}\2.json");
            var jObject = JObject.Parse(json);

            // Act
            var history = ScreepsRoomHistoryComputedHelper.Compute(jObject);

            // Assert
            Assert.Single(history.Creeps.OtherCreeps);
        }

        [Fact]
        public void Test_CreepDies()
        {
            // Arrange
            var json = File.ReadAllText($@"{HistoryFilesLocations}\3.json");
            var jObject = JObject.Parse(json);

            // Act
            var history = ScreepsRoomHistoryComputedHelper.Compute(jObject);

            // Assert
            Assert.Empty(history.Creeps.OtherCreeps);
        }
    }
}
