using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using System.Configuration;
using UserTrackerShared.States;

namespace UserTracker.Tests.States
{
    public class ScreepsRoomHistoryDtoFileTests
    {
        public ScreepsRoomHistoryDtoFileTests()
        {
            var configFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = "App.Live.Config"
            };
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            ConfigSettingsState.InitTest(configuration.AppSettings);
        }
        private static JObject ParseFile(string fileName)
        {
            using var reader = new StreamReader(@"C:\Users\Pieter\source\repos\ScreepsUserTracker-V2\UserTracker.Tests\Files\"+fileName);
            using var jsonReader = new JsonTextReader(reader);
            return JObject.Load(jsonReader);
        }

        private static ScreepsRoomHistoryDto ProcessHistory(JObject roomData)
        {
            var roomHistory = new ScreepsRoomHistory();
            var roomHistoryDto = new ScreepsRoomHistoryDto();

            roomData.TryGetValue("timestamp", out JToken? jTokenTime);
            if (jTokenTime != null) roomHistory.TimeStamp = jTokenTime.Value<long>();
            roomData.TryGetValue("base", out JToken? jTokenBase);
            if (jTokenBase != null) roomHistory.Base = jTokenBase.Value<long>();

            if (roomData.TryGetValue("ticks", out JToken? jTokenTicks) && jTokenTicks is JObject jObjectTicks)
            {
                for (int i = 0; i < ConfigSettingsState.TicksInObject; i++)
                {
                    long tickNumber = roomHistory.Base + i;
                    roomHistory.Tick = tickNumber;

                    if (jObjectTicks.TryGetValue(tickNumber.ToString(), out JToken? tickObject) && tickObject != null)
                    {
                        roomHistory = ScreepsRoomHistoryHelper.ComputeTick(tickObject, roomHistory);
                    }
                    roomHistoryDto.Update(roomHistory);
                }
            }

            return roomHistoryDto;
        }

        [Fact]
        public void Case1()
        {
            var jObject = ParseFile("case1.json");
            Assert.NotNull(jObject);
            var dto = ProcessHistory(jObject);

            Assert.NotNull(dto);
            Assert.Equal(12, dto.Creeps.OwnedCreeps.Count);
        }

        [Fact]
        public void Case2()
        {
            var jObject = ParseFile("case2.json");
            Assert.NotNull(jObject);
            var dto = ProcessHistory(jObject);

            Assert.NotNull(dto);
            Assert.Equal(4, dto.Creeps.OwnedCreeps.Count);
        }

        [Fact]
        public void Case3()
        {
            var jObject = ParseFile("case3.json");
            Assert.NotNull(jObject);
            var dto = ProcessHistory(jObject);

            Assert.NotNull(dto);
            Assert.Equal(375, dto.Structures.Wall.Count);
        }
    }
}
