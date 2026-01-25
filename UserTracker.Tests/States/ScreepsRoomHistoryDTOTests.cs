using System;
using System.Collections.Generic;
using System.Configuration;
using UserTrackerShared.Models;
using UserTrackerShared.States;
using Xunit;

namespace UserTracker.Tests.Models
{
    public class ScreepsRoomHistoryDtoTests
    {
        public ScreepsRoomHistoryDtoTests()
        {
            var configFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = "App.Config"
            };
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            ConfigSettingsState.InitTest(configuration.AppSettings);
        }

        [Fact]
        public void InitializationTest()
        {
            var dto = new ScreepsRoomHistoryDto();

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
            var dto = new ScreepsRoomHistoryDto();
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
            var dto = new ScreepsRoomHistoryDto();
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
            var dto = new ScreepsRoomHistoryDto();
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
            var dto = new ScreepsRoomHistoryDto();
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
            var dto = new ScreepsRoomHistoryDto();
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
            var dto = new ScreepsRoomHistoryDto();
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
            var dto = new ScreepsRoomHistoryDto();
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
        public void Combine_ValidScreepsRoomHistory()
        {
            var dto = new ScreepsRoomHistoryDto();
            var dto2 = new ScreepsRoomHistoryDto();
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
            var history2 = new ScreepsRoomHistory
            {
                TimeStamp = 12345678,
                Base = 98765432,
                Tick = 200,
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
                dto2.Update(history2);
            }

            dto.Combine(dto2);

            Assert.Equal(12345678, dto.TimeStamp);
            Assert.Equal(987654321, dto.Base);
            Assert.Equal(200, dto.Tick);
            Assert.Single(dto.GroundResources);
            Assert.Equal(1000 * 2, dto.GroundResources["energy"]);
            Assert.NotNull(dto.Creeps.OwnedCreeps);
            Assert.Equal(1 * 2, dto.Creeps.OwnedCreeps.Count);
            Assert.NotNull(dto.Structures.Controller);
            Assert.Equal(1 * 2, dto.Structures.Controller.Level);
        }

        [Fact]
        public void ClearAllTest()
        {
            var dto = new ScreepsRoomHistoryDto();
            dto.GroundResources["energy"] = 1000;
            dto.Creeps.OwnedCreeps.Count = 1;
            dto.Structures.Controller.Level = 1;

            dto.ClearAll();

            Assert.Empty(dto.GroundResources);
            Assert.Equal(0, dto.Creeps.OwnedCreeps.Count);
            Assert.Equal(0, dto.Structures.Controller.Level);
        }  

        [Fact]
        public void Combine_AggregatesRoomMetrics()
        {
            var aggregate = new ScreepsRoomHistoryDto();

            var groundResources = new Dictionary<string, decimal>
            {
                { "energy", 1000 }
            };
            var groundResources2 = new Dictionary<string, decimal>
            {
                { "energy", 1000 },
                { "x", 1000 }
            };

            var structures = new StructuresDto
            {
                Controller = new StructureControllerDto { 
                    Count = 1000,
                    Level = 1000,
                    Progress = 1000,
                    ProgressTotal = 1000,
                    UserId = string.Empty,
                    ReservationUserId = null,
                    Upgraded = 1000
                },
                Mineral = new StructureMineralDto { 
                    Count = 1000,
                },
                Deposit = new StructureDepositDto { 
                    Count = 1000,
                },
                Wall = new StructureWallDto()
                {
                    Hits = 1000,
                    Count = 1000,
                },
                ConstructionSite = new StructureConstructionSiteDto()
                {
                    Count = 1000,
                    Progress = 1000,
                    ProgressTotal = 1000,
                    TypesBuilding = new Dictionary<string, decimal>()
                    {
                        { "a", 1000 },
                        { "b", 1000 }
                    }
                },
                Container = new StructureContainerDto()
                {
                    Count = 1000,
                    Store = new Store()
                    {
                        energy = 1000,
                        power = 1000,
                        alloy = 1000,
                    },
                },
                Extension = new StructureExtensionDto()
                {
                    Count = 1000,
                    Energy = 1000,
                    EnergyCapacity = 1000,
                },
                Extractor = new StructureExtractorDto()
                {
                    Count = 1000,
                },
                Factory = new StructureFactoryDto()
                {
                    Count = 1000,
                },
                InvaderCore = new StructureInvaderCoreDto()
                {
                    Count = 1000,
                },
                KeeperLair = new StructureKeeperLairDto()
                {
                    Count = 1000,
                },
                Lab = new StructureLabDto()
                {
                    Count = 1000,
                },
                Link = new StructureLinkDto()
                { 
                    Count = 1000,
                    EnergyCapacity = 1000,
                    Energy = 1000,
                },
                Observer = new StructureObserverDto() 
                {
                    Count = 1000,
                },
                Portal = new StructurePortalDto() 
                {
                    Count = 1000,
                },
                PowerBank = new StructurePowerBankDto()
                {
                    Count = 1000,
                },
                PowerSpawn = new StructurePowerSpawnDto()
                {
                    Count = 1000,
                },
                Rampart = new StructureRampartDto()
                {
                    Count = 1000,
                    Hits = 1000,
                },
                Road = new StructureRoadDto()
                {
                    Count = 1000,
                },
                Source = new StructureSourceDto() { 
                    Count = 1000,
                    Energy = 1000,
                    EnergyCapacity = 1000,
                },
                Spawn = new StructureSpawnDto()
                {
                    Count = 1000,
                },
                Storage = new StructureStorageDto() { 
                    Count = 1000,
                    Store = new Store()
                    {
                        energy = 1000,
                        power = 1000,
                        alloy = 1000,
                    },
                },
                Terminal = new StructureTerminalDto() 
                { 
                    Count = 1000,
                    Store = new Store()
                    {
                        energy = 1000,
                        power = 1000,
                        alloy = 1000,
                    },
                },
                Tombstone = new StructureTombstoneDto() 
                {
                    Count = 1000,
                },
                Tower = new StructureTowerDto() 
                { 
                    Count = 1000,
                    Energy = 1000,
                },
                Nuker = new StructureNukerDto()
                {
                    Count = 1000,
                },
                Nuke = new StructureNukeDto()
                {
                    Count = 1000
                }
            };


            var creep = new CreepDto()
            {
                Count = 1000,
                ActionLog = new ActionLogDto()
                {
                    Attack = new DamageActionDto()
                    {
                        Count = 1000,
                        Damage = 1000
                    },
                    Attacked = new DamageActionDto()
                    {
                        Count = 1000,
                        Damage = 1000
                    },
                    Build = new OutlflowActionDto()
                    {
                        Count = 1000,
                        Outflow = 1000,
                    },
                    Heal = new HealActionDto()
                    {
                        Count = 1000,
                        Heal = 1000
                    },
                    Healed = new HealActionDto()
                    {
                        Count = 1000,
                        Heal = 1000
                    },
                    Harvest = new InflowActionDto()
                    {
                        Count = 1000,
                        Inflow = 1000
                    },
                    Move = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    Repair = new OutlflowActionDto()
                    {
                        Count = 1000,
                        Outflow = 1000
                    },
                    ReserveController = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    Say = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    Spawned = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    TransferEnergy = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    UpgradeController = new OutlflowActionDto()
                    {
                        Count = 1000,
                        Outflow = 1000
                    },
                    Power = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    RangedAttack = new DamageActionDto()
                    {
                        Count = 1000,
                        Damage = 1000
                    },
                    RangedHeal = new HealActionDto()
                    {
                        Count = 1000,
                        Heal = 1000
                    },
                    AttackController = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    Produce = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    RangedMassAttack = new DamageActionDto()
                    {
                        Count = 1000,
                        Damage = 1000
                    },
                    ReverseReaction = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    RunReaction = new GenericActionDto()
                    {
                        Count = 1000
                    }
                },
                BodyParts = new CountByPartDto()
                {
                    Move = 1000,
                    Work = 1000,
                    Carry = 1000,
                    Attack = 1000,
                    RangedAttack = 1000,
                    Tough = 1000,
                    Heal = 1000,
                    Claim = 1000,
                },
                Store = new Store()
                {
                    energy = 1000,
                    power = 1000,
                    alloy = 1000,
                }
            };
            var creep2 = new CreepDto()
            {
                Count = 1000,
                ActionLog = new ActionLogDto()
                {
                    Attack = new DamageActionDto()
                    {
                        Count = 1000,
                        Damage = 1000
                    },
                    Attacked = new DamageActionDto()
                    {
                        Count = 1000,
                        Damage = 1000
                    },
                    Build = new OutlflowActionDto()
                    {
                        Count = 1000,
                        Outflow = 1000,
                    },
                    Heal = new HealActionDto()
                    {
                        Count = 1000,
                        Heal = 1000
                    },
                    Healed = new HealActionDto()
                    {
                        Count = 1000,
                        Heal = 1000
                    },
                    Harvest = new InflowActionDto()
                    {
                        Count = 1000,
                        Inflow = 1000
                    },
                    Move = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    Repair = new OutlflowActionDto()
                    {
                        Count = 1000,
                        Outflow = 1000
                    },
                    ReserveController = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    Say = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    Spawned = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    TransferEnergy = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    UpgradeController = new OutlflowActionDto()
                    {
                        Count = 1000,
                        Outflow = 1000
                    },
                    Power = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    RangedAttack = new DamageActionDto()
                    {
                        Count = 1000,
                        Damage = 1000
                    },
                    RangedHeal = new HealActionDto()
                    {
                        Count = 1000,
                        Heal = 1000
                    },
                    AttackController = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    Produce = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    RangedMassAttack = new DamageActionDto()
                    {
                        Count = 1000,
                        Damage = 1000
                    },
                    ReverseReaction = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    RunReaction = new GenericActionDto()
                    {
                        Count = 1000
                    }
                },
                BodyParts = new CountByPartDto()
                {
                    Move = 1000,
                    Work = 1000,
                    Carry = 1000,
                    Attack = 1000,
                    RangedAttack = 1000,
                    Tough = 1000,
                    Heal = 1000,
                    Claim = 1000,
                },
                Store = new Store()
                {
                    energy = 1000,
                    power = 1000,
                    alloy = 1000,
                }
            };
            var creep3 = new CreepDto()
            {
                Count = 1000,
                ActionLog = new ActionLogDto()
                {
                    Attack = new DamageActionDto()
                    {
                        Count = 1000,
                        Damage = 1000
                    },
                    Attacked = new DamageActionDto()
                    {
                        Count = 1000,
                        Damage = 1000
                    },
                    Build = new OutlflowActionDto()
                    {
                        Count = 1000,
                        Outflow = 1000,
                    },
                    Heal = new HealActionDto()
                    {
                        Count = 1000,
                        Heal = 1000
                    },
                    Healed = new HealActionDto()
                    {
                        Count = 1000,
                        Heal = 1000
                    },
                    Harvest = new InflowActionDto()
                    {
                        Count = 1000,
                        Inflow = 1000
                    },
                    Move = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    Repair = new OutlflowActionDto()
                    {
                        Count = 1000,
                        Outflow = 1000
                    },
                    ReserveController = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    Say = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    Spawned = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    TransferEnergy = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    UpgradeController = new OutlflowActionDto()
                    {
                        Count = 1000,
                        Outflow = 1000
                    },
                    Power = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    RangedAttack = new DamageActionDto()
                    {
                        Count = 1000,
                        Damage = 1000
                    },
                    RangedHeal = new HealActionDto()
                    {
                        Count = 1000,
                        Heal = 1000
                    },
                    AttackController = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    Produce = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    RangedMassAttack = new DamageActionDto()
                    {
                        Count = 1000,
                        Damage = 1000
                    },
                    ReverseReaction = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    RunReaction = new GenericActionDto()
                    {
                        Count = 1000
                    }
                },
                BodyParts = new CountByPartDto()
                {
                    Move = 1000,
                    Work = 1000,
                    Carry = 1000,
                    Attack = 1000,
                    RangedAttack = 1000,
                    Tough = 1000,
                    Heal = 1000,
                    Claim = 1000,
                },
                Store = new Store()
                {
                    energy = 1000,
                    power = 1000,
                    alloy = 1000,
                }
            };
            var creep4 = new CreepDto()
            {
                Count = 1000,
                ActionLog = new ActionLogDto()
                {
                    Attack = new DamageActionDto()
                    {
                        Count = 1000,
                        Damage = 1000
                    },
                    Attacked = new DamageActionDto()
                    {
                        Count = 1000,
                        Damage = 1000
                    },
                    Build = new OutlflowActionDto()
                    {
                        Count = 1000,
                        Outflow = 1000,
                    },
                    Heal = new HealActionDto()
                    {
                        Count = 1000,
                        Heal = 1000
                    },
                    Healed = new HealActionDto()
                    {
                        Count = 1000,
                        Heal = 1000
                    },
                    Harvest = new InflowActionDto()
                    {
                        Count = 1000,
                        Inflow = 1000
                    },
                    Move = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    Repair = new OutlflowActionDto()
                    {
                        Count = 1000,
                        Outflow = 1000
                    },
                    ReserveController = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    Say = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    Spawned = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    TransferEnergy = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    UpgradeController = new OutlflowActionDto()
                    {
                        Count = 1000,
                        Outflow = 1000
                    },
                    Power = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    RangedAttack = new DamageActionDto()
                    {
                        Count = 1000,
                        Damage = 1000
                    },
                    RangedHeal = new HealActionDto()
                    {
                        Count = 1000,
                        Heal = 1000
                    },
                    AttackController = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    Produce = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    RangedMassAttack = new DamageActionDto()
                    {
                        Count = 1000,
                        Damage = 1000
                    },
                    ReverseReaction = new GenericActionDto()
                    {
                        Count = 1000
                    },
                    RunReaction = new GenericActionDto()
                    {
                        Count = 1000
                    }
                },
                BodyParts = new CountByPartDto()
                {
                    Move = 1000,
                    Work = 1000,
                    Carry = 1000,
                    Attack = 1000,
                    RangedAttack = 1000,
                    Tough = 1000,
                    Heal = 1000,
                    Claim = 1000,
                },
                Store = new Store()
                {
                    energy = 1000,
                    power = 1000,
                    alloy = 1000,
                }
            };

            var roomOne = new ScreepsRoomHistoryDto
            {
                GroundResources = groundResources,
                Structures = structures,
                Creeps = new CreepsDto
                {
                    EnemyCreeps = creep,
                    OtherCreeps = creep2,
                    OwnedCreeps = creep3,
                    PowerCreeps = creep4,
                }
            };

            var roomTwo = new ScreepsRoomHistoryDto
            {
                GroundResources = groundResources2,
                Structures = structures,
                Creeps = new CreepsDto
                {
                    EnemyCreeps = creep,
                    OtherCreeps = creep2,
                    OwnedCreeps = creep3,
                    PowerCreeps = creep4,
                }
            };

            aggregate.Combine(roomOne);
            aggregate.Combine(roomTwo);

            Assert.Equal(2000, aggregate.GroundResources["energy"]);
            Assert.Equal(1000, aggregate.GroundResources["x"]);


            Assert.Equal(2000, aggregate.Structures.Controller.Count);
            Assert.Equal(2000, aggregate.Structures.Controller.Level);
            Assert.Equal(2000, aggregate.Structures.Controller.Progress);
            Assert.Equal(2000, aggregate.Structures.Controller.ProgressTotal);
            Assert.Equal(2000, aggregate.Structures.Controller.Upgraded);
            Assert.Equal(2000, aggregate.Structures.Mineral.Count);
            Assert.Equal(2000, aggregate.Structures.Deposit.Count);
            Assert.Equal(2000, aggregate.Structures.Wall.Count);
            Assert.Equal(2000, aggregate.Structures.Wall.Hits);
            Assert.Equal(2000, aggregate.Structures.ConstructionSite.Count);
            Assert.Equal(2000, aggregate.Structures.ConstructionSite.Progress);
            Assert.Equal(2000, aggregate.Structures.ConstructionSite.ProgressTotal);
            Assert.Equal(2000, aggregate.Structures.ConstructionSite.TypesBuilding["a"]);
            Assert.Equal(2000, aggregate.Structures.ConstructionSite.TypesBuilding["b"]);
            Assert.Equal(2000, aggregate.Structures.Container.Count);
            Assert.Equal(2000, aggregate.Structures.Container.Store.energy);
            Assert.Equal(2000, aggregate.Structures.Container.Store.power);
            Assert.Equal(2000, aggregate.Structures.Container.Store.alloy);
            Assert.Equal(2000, aggregate.Structures.Extension.Count);
            Assert.Equal(2000, aggregate.Structures.Extension.Energy);
            Assert.Equal(2000, aggregate.Structures.Extension.EnergyCapacity);
            Assert.Equal(2000, aggregate.Structures.Extractor.Count);
            Assert.Equal(2000, aggregate.Structures.Factory.Count);
            Assert.Equal(2000, aggregate.Structures.InvaderCore.Count);
            Assert.Equal(2000, aggregate.Structures.KeeperLair.Count);
            Assert.Equal(2000, aggregate.Structures.Lab.Count);
            Assert.Equal(2000, aggregate.Structures.Link.Count);
            Assert.Equal(2000, aggregate.Structures.Link.Energy);
            Assert.Equal(2000, aggregate.Structures.Link.EnergyCapacity);
            Assert.Equal(2000, aggregate.Structures.Observer.Count);
            Assert.Equal(2000, aggregate.Structures.Portal.Count);
            Assert.Equal(2000, aggregate.Structures.PowerBank.Count);
            Assert.Equal(2000, aggregate.Structures.PowerSpawn.Count);
            Assert.Equal(2000, aggregate.Structures.Rampart.Count);
            Assert.Equal(2000, aggregate.Structures.Rampart.Hits);
            Assert.Equal(2000, aggregate.Structures.Road.Count);
            Assert.Equal(2000, aggregate.Structures.Source.Count);
            Assert.Equal(2000, aggregate.Structures.Source.Energy);
            Assert.Equal(2000, aggregate.Structures.Source.EnergyCapacity);
            Assert.Equal(2000, aggregate.Structures.Spawn.Count);
            Assert.Equal(2000, aggregate.Structures.Storage.Count);
            Assert.Equal(2000, aggregate.Structures.Storage.Store.energy);
            Assert.Equal(2000, aggregate.Structures.Storage.Store.power);
            Assert.Equal(2000, aggregate.Structures.Storage.Store.alloy);
            Assert.Equal(2000, aggregate.Structures.Terminal.Count);
            Assert.Equal(2000, aggregate.Structures.Terminal.Store.energy);
            Assert.Equal(2000, aggregate.Structures.Terminal.Store.power);
            Assert.Equal(2000, aggregate.Structures.Terminal.Store.alloy);
            Assert.Equal(2000, aggregate.Structures.Tombstone.Count);
            Assert.Equal(2000, aggregate.Structures.Tower.Count);
            Assert.Equal(2000, aggregate.Structures.Tower.Energy);
            Assert.Equal(2000, aggregate.Structures.Nuker.Count);
            Assert.Equal(2000, aggregate.Structures.Nuke.Count);


            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.Attack.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.Attack.Damage);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.Attacked.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.Attacked.Damage);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.Build.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.Build.Outflow);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.Heal.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.Heal.Heal);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.Healed.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.Healed.Heal);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.Harvest.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.Harvest.Inflow);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.Move.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.Repair.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.Repair.Outflow);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.ReserveController.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.Say.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.Spawned.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.TransferEnergy.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.UpgradeController.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.UpgradeController.Outflow);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.Power.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.RangedAttack.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.RangedAttack.Damage);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.RangedHeal.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.RangedHeal.Heal);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.AttackController.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.Produce.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.RangedMassAttack.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.RangedMassAttack.Damage);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.ReverseReaction.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.ActionLog.RunReaction.Count);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.BodyParts.Move);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.BodyParts.Work);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.BodyParts.Carry);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.BodyParts.Attack);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.BodyParts.RangedAttack);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.BodyParts.Tough);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.BodyParts.Heal);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.BodyParts.Claim);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.Store.energy);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.Store.power);
            Assert.Equal(2000, aggregate.Creeps.OwnedCreeps.Store.alloy);
        }

        [Fact]
        public void PropertyTests()
        {
            var dto = new ScreepsRoomHistoryDto();

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
            var dto = new ScreepsRoomHistoryDto();
            var history = new ScreepsRoomHistory
            {
                GroundResources = new Dictionary<string, GroundResource>
                {
                }
            };

            dto.ProcessGroundResources(history);

            Assert.Empty(dto.GroundResources);
        }

        [Fact]
        public void ProcessStructures_NullValues()
        {
            var dto = new ScreepsRoomHistoryDto();
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
            var dto = new ScreepsRoomHistoryDto();
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
            var dto = new ScreepsRoomHistoryDto();
            var history = new ScreepsRoomHistory
            {
                Structures = new Structures
                {
                    Controller = new StructureController { Id = "controller1", Level = 900000 }
                }
            };

            for (int i = 0; i < ConfigSettingsState.TicksInFile; i++)
            {
                dto.ProcessStructures(history);
            }


            Assert.NotNull(dto.Structures.Controller);
            Assert.Equal(900000, dto.Structures.Controller.Level);
        }
    }
}
