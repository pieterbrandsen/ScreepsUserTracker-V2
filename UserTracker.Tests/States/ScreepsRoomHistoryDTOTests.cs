using System;
using System.Collections.Generic;
using System.Configuration;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using Xunit;

namespace UserTracker.Tests.Models
{
    public class ScreepsRoomHistoryDTOTests
    {
        public ScreepsRoomHistoryDTOTests()
        {
            var configFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = "App.Live.Config"
            };
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            ConfigSettingsState.InitTest(configuration.AppSettings);
        }

        [Fact]
        public void InitializationTest()
        {
            var dto = new ScreepsRoomHistoryDTO();

            Assert.Equal(0, dto.TimeStamp);
            Assert.Equal(0, dto.Base);
            Assert.Equal(0, dto.Tick);
            Assert.Empty(dto.GroundResources);
            Assert.NotNull(dto.Creeps);
            Assert.NotNull(dto.Structures);
        }

        [Fact]
        public void ProcessGroundResources_EmptyDictionary()
        {
            var dto = new ScreepsRoomHistoryDTO();
            var history = new ScreepsRoomHistory
            {
                GroundResources = new Dictionary<string, GroundResource>()
            };

            dto.ProcessGroundResources(history);

            Assert.Empty(dto.GroundResources);
        }

        [Fact]
        public void ProcessGroundResources_NonEmptyDictionary()
        {
            var dto = new ScreepsRoomHistoryDTO();
            var history = new ScreepsRoomHistory
            {
                GroundResources = new Dictionary<string, GroundResource>
                {
                    { "energy", new GroundResource { ResourceType = "energy", energy = 1000 } }
                }
            };

            dto.ProcessGroundResources(history);

            Assert.Single(dto.GroundResources);
            Assert.Equal(1000 / ConfigSettingsState.TicksInFile, dto.GroundResources["energy"]);
        }

        [Fact]
        public void ProcessCreeps_EmptyDictionary()
        {
            var dto = new ScreepsRoomHistoryDTO();
            var history = new ScreepsRoomHistory
            {
                Creeps = new Creeps()
            };

            dto.ProcessCreeps(history);

            Assert.NotNull(dto.Creeps.OwnedCreeps);
            Assert.NotNull(dto.Creeps.EnemyCreeps);
            Assert.NotNull(dto.Creeps.OtherCreeps);
            Assert.NotNull(dto.Creeps.PowerCreeps);
        }

        [Fact]
        public void ProcessCreeps_NonEmptyDictionary()
        {
            var dto = new ScreepsRoomHistoryDTO();
            var history = new ScreepsRoomHistory
            {
                Creeps = new Creeps
                {
                    OwnedCreeps = new Dictionary<string, Creep>
                    {
                        { "creep1", new Creep { Id = "creep1", Name = "Creep1" } }
                    }
                }
            };

            for (int i = 0; i < ConfigSettingsState.TicksInFile; i++)
            {
                dto.ProcessCreeps(history);
            }

            Assert.NotNull(dto.Creeps.OwnedCreeps);
            Assert.Equal(1, dto.Creeps.OwnedCreeps.Count);
        }

        [Fact]
        public void ProcessStructures_EmptyDictionary()
        {
            var dto = new ScreepsRoomHistoryDTO();
            var history = new ScreepsRoomHistory
            {
                Structures = new Structures()
            };

            dto.ProcessStructures(history);

            Assert.NotNull(dto.Structures.Controller);
            Assert.NotNull(dto.Structures.Mineral);
            Assert.NotNull(dto.Structures.Deposit);
        }

        [Fact]
        public void ProcessStructures_NonEmptyDictionary()
        {
            var dto = new ScreepsRoomHistoryDTO();
            var history = new ScreepsRoomHistory
            {
                Structures = new Structures
                {
                    Controller = new StructureController { Id = "controller1", Level = 1 }
                }
            };

            for (int i = 0; i < ConfigSettingsState.TicksInFile; i++)
            {
                dto.ProcessStructures(history);
            }

            Assert.NotNull(dto.Structures.Controller);
            Assert.Equal(1, dto.Structures.Controller.Level);
        }

        [Fact]
        public void Update_ValidScreepsRoomHistory()
        {
            var dto = new ScreepsRoomHistoryDTO();
            var history = new ScreepsRoomHistory
            {
                TimeStamp = 123456789,
                Base = 987654321,
                Tick = 100,
                GroundResources = new Dictionary<string, GroundResource>
                {
                    { "energy", new GroundResource { ResourceType = "energy", energy = 1000 } }
                },
                Creeps = new Creeps
                {
                    OwnedCreeps = new Dictionary<string, Creep>
                    {
                        { "creep1", new Creep { Id = "creep1", Name = "Creep1" } }
                    }
                },
                Structures = new Structures
                {
                    Controller = new StructureController { Id = "controller1", Level = 1 }
                }
            };


            for (int i = 0; i < ConfigSettingsState.TicksInFile; i++)
            {
                dto.Update(history);
            }

            Assert.Equal(123456789, dto.TimeStamp);
            Assert.Equal(987654321, dto.Base);
            Assert.Equal(100, dto.Tick);
            Assert.Single(dto.GroundResources);
            Assert.Equal(1000, dto.GroundResources["energy"]);
            Assert.NotNull(dto.Creeps.OwnedCreeps);
            Assert.Equal(1, dto.Creeps.OwnedCreeps.Count);
            Assert.NotNull(dto.Structures.Controller);
            Assert.Equal(1, dto.Structures.Controller.Level);
        }

        [Fact]
        public void ClearAllTest()
        {
            var dto = new ScreepsRoomHistoryDTO();
            dto.GroundResources["energy"] = 1000;
            dto.Creeps.OwnedCreeps.Count = 1;
            dto.Structures.Controller.Level = 1;

            dto.ClearAll();

            Assert.Empty(dto.GroundResources);
            Assert.Equal(0, dto.Creeps.OwnedCreeps.Count);
            Assert.Equal(0, dto.Structures.Controller.Level);
        }

        [Fact]
        public void PropertyTests()
        {
            var dto = new ScreepsRoomHistoryDTO();

            dto.TimeStamp = 123456789;
            dto.Base = 987654321;
            dto.Tick = 100;

            Assert.Equal(123456789, dto.TimeStamp);
            Assert.Equal(987654321, dto.Base);
            Assert.Equal(100, dto.Tick);
        }

        [Fact]
        public void ProcessGroundResources_NullValues()
        {
            var dto = new ScreepsRoomHistoryDTO();
            var history = new ScreepsRoomHistory
            {
                GroundResources = new Dictionary<string, GroundResource>
                {
                    { "energy", null }
                }
            };

            dto.ProcessGroundResources(history);

            Assert.Empty(dto.GroundResources);
        }

        [Fact]
        public void ProcessStructures_NullValues()
        {
            var dto = new ScreepsRoomHistoryDTO();
            var history = new ScreepsRoomHistory
            {
                Structures = new Structures
                {
                    Controller = null
                }
            };

            dto.ProcessStructures(history);

            Assert.NotNull(dto.Structures.Controller);
            Assert.Equal(0, dto.Structures.Controller.Level);
        }

        [Fact]
        public void ProcessGroundResources_ExtremelyLargeValues()
        {
            var dto = new ScreepsRoomHistoryDTO();
            var history = new ScreepsRoomHistory
            {
                GroundResources = new Dictionary<string, GroundResource>
                {
                    { "energy", new GroundResource { ResourceType = "energy", energy = 900000 } }
                }
            };


            for (int i = 0; i < ConfigSettingsState.TicksInFile; i++)
            {
                dto.ProcessGroundResources(history);
            }

            Assert.Single(dto.GroundResources);
            Assert.Equal(900000, dto.GroundResources["energy"]);
        }

        [Fact]
        public void ProcessStructures_ExtremelyLargeValues()
        {
            var dto = new ScreepsRoomHistoryDTO();
            var history = new ScreepsRoomHistory
            {
                Structures = new Structures
                {
                    Controller = new StructureController { Id = "controller1", Level = 987654321 }
                }
            };

            for (int i = 0; i < ConfigSettingsState.TicksInFile; i++)
            {
                dto.ProcessStructures(history);
            }


            Assert.NotNull(dto.Structures.Controller);
            Assert.Equal(987654321, dto.Structures.Controller.Level);
        }
    }
}
