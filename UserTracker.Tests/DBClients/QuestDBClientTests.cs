using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using QuestDB.Senders;
using QuestDB.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserTrackerShared.DBClients;
using UserTrackerShared.Models;
using UserTrackerShared.Models.Db;
using UserTrackerShared.Helpers;
using UserTrackerShared.States;
using Xunit;

namespace UserTracker.Tests.DBClients
{
    public class QuestDBClientTests
    {
        private static bool _configInitialized;

        [Fact]
        public void QuestDBDtoHelper_GetStructureCounts_CalculatesExpectedTotals()
        {
            var history = new ScreepsRoomHistoryDto();
            history.Structures.Controller.UserId = "player";
            history.Structures.Wall.Count = 12.9m;
            history.Structures.Container.Count = 3.4m;
            history.Structures.Extension.Count = 5.2m;
            history.Structures.Rampart.Count = 6.8m;
            history.Structures.Link.Count = 3.9m;
            history.Structures.PowerSpawn.Count = 4.2m;
            history.Structures.Road.Count = 10.5m;
            history.Structures.Spawn.Count = 3.5m;
            history.Structures.Storage.Count = 1.0m;
            history.Structures.Terminal.Count = 2.1m;
            history.Structures.Tower.Count = 2.8m;
            history.Structures.Nuker.Count = 1.0m;

            var (structureCount, placedStructureCount, structureCounts) = QuestDBDtoHelper.GetStructureCounts(history);

            var expectedStructureKeys = new[]
            {
                "wall", "container", "extension", "rampart", "link",
                "powerspawn", "road", "spawn", "storage", "terminal",
                "tower", "nuker"
            };
            var expectedPlacedKeys = new[]
            {
                "wall", "container", "extension", "extractor", "factory",
                "lab", "link", "observer", "powerspawn", "rampart", "road",
                "spawn", "storage", "terminal", "tower", "nuker"
            };

            var expectedStructureCount = expectedStructureKeys.Sum(key => structureCounts[key]);
            var expectedPlacedStructureCount = expectedPlacedKeys.Sum(key => structureCounts[key]);

            Assert.Equal(expectedStructureCount, structureCount);
            Assert.Equal(expectedPlacedStructureCount, placedStructureCount);
            Assert.Equal(12.9m, structureCounts["wall"]);
            Assert.Equal(3.4m, structureCounts["container"]);
            Assert.Equal(5.2m, structureCounts["extension"]);
            Assert.Equal(6.8m, structureCounts["rampart"]);
            Assert.Equal(3.9m, structureCounts["link"]);
            Assert.Equal(4.2m, structureCounts["powerspawn"]);
            Assert.Equal(10.5m, structureCounts["road"]);
            Assert.Equal(3.5m, structureCounts["spawn"]);
            Assert.Equal(1.0m, structureCounts["storage"]);
            Assert.Equal(2.1m, structureCounts["terminal"]);
            Assert.Equal(2.8m, structureCounts["tower"]);
            Assert.Equal(1.0m, structureCounts["nuker"]);
        }

        [Fact]
        public void QuestDBDtoHelper_IncludesControllerMineralAndDepositCounts()
        {
            var history = new ScreepsRoomHistoryDto();
            history.Structures.Controller.Count = 1;
            history.Structures.Mineral.Count = 1;
            history.Structures.Deposit.Count = 1;

            var (_, _, structureCounts) = QuestDBDtoHelper.GetStructureCounts(history);

            Assert.Equal(1, structureCounts["controller"]);
            Assert.Equal(1, structureCounts["mineral"]);
            Assert.Equal(1, structureCounts["deposit"]);
        }

        [Fact]
        public void QuestDBClientState_GetQuestDBDto_CalculatesControllerPointsPerTick()
        {
            var method = typeof(QuestDBClientState).GetMethod("GetQuestDBDto", BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(method);

            var history = new ScreepsRoomHistoryDto();
            history.Structures.Controller.Upgraded = 123;

            var dto = (QuestDBHistoryDTO)method.Invoke(null, new object?[] { history })!;
            Assert.Equal(123, dto.ControllerPointsPerTick);
            Assert.Equal(0, dto.ControllerScorePerTick);
        }

        [Fact]
        public void QuestDBClientState_GetQuestDBDto_KeepsControllerUpgradedWhenScoreIsPresent()
        {
            var method = typeof(QuestDBClientState).GetMethod("GetQuestDBDto", BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(method);

            var history = new ScreepsRoomHistoryDto();
            history.Structures.Controller.Upgraded = 123;
            history.Structures.Controller.ScorePerTick = 7;

            var dto = (QuestDBHistoryDTO)method.Invoke(null, new object?[] { history })!;
            Assert.Equal(123, dto.ControllerPointsPerTick);
            Assert.Equal(7, dto.ControllerScorePerTick);
        }

        [Fact]
        public void QuestDBDtoHelper_GetCreepCounts_SumsOwningValues()
        {
            var history = new ScreepsRoomHistoryDto();
            history.Creeps.OwnedCreeps.Count = 2;
            history.Creeps.EnemyCreeps.Count = 3;
            history.Creeps.OtherCreeps.Count = 1;
            history.Creeps.PowerCreeps.Count = 4;

            var (total, owned, enemy, other, power) = QuestDBDtoHelper.GetCreepCounts(history);

            Assert.Equal(10, total);
            Assert.Equal(2, owned);
            Assert.Equal(3, enemy);
            Assert.Equal(1, other);
            Assert.Equal(4, power);
        }

        [Fact]
        public void QuestDBDtoHelper_GetCreepPartsCounts_AggregatesOwnedParts()
        {
            var history = new ScreepsRoomHistoryDto();
            history.Creeps.OwnedCreeps.BodyParts.Attack = 2;
            history.Creeps.OwnedCreeps.BodyParts.Carry = 3;
            history.Creeps.OwnedCreeps.BodyParts.Heal = 1;
            history.Creeps.OwnedCreeps.BodyParts.Move = 4;
            history.Creeps.OwnedCreeps.BodyParts.RangedAttack = 1;
            history.Creeps.OwnedCreeps.BodyParts.Tough = 2;
            history.Creeps.OwnedCreeps.BodyParts.Work = 5;
            history.Creeps.OwnedCreeps.BodyParts.Claim = 1;

            var (count, counts) = QuestDBDtoHelper.GetCreepPartsCounts(history);

            Assert.Equal(19, count);
            Assert.Equal(2, counts["attack"]);
            Assert.Equal(3, counts["carry"]);
            Assert.Equal(1, counts["heal"]);
            Assert.Equal(4, counts["move"]);
            Assert.Equal(1, counts["ranged_attack"]);
            Assert.Equal(2, counts["tough"]);
            Assert.Equal(5, counts["work"]);
            Assert.Equal(1, counts["claim"]);
        }

        [Fact]
        public void QuestDBDtoHelper_GetCreepIntentsCounts_AggregatesActions()
        {
            var history = new ScreepsRoomHistoryDto();
            var log = history.Creeps.OwnedCreeps.ActionLog;
            log.Attack.Count = 2;
            log.Attacked.Count = 1;
            log.RangedAttack.Count = 2;
            log.RangedMassAttack.Count = 1;
            log.RangedHeal.Count = 3;
            log.Heal.Count = 1;
            log.Healed.Count = 1;
            log.Harvest.Count = 4;
            log.Repair.Count = 2;
            log.Build.Count = 1;
            log.UpgradeController.Count = 2;
            log.Move.Count = 3;
            log.Say.Count = 1;
            log.ReserveController.Count = 1;
            log.AttackController.Count = 1;
            log.Produce.Count = 2;
            log.TransferEnergy.Count = 1;
            log.RunReaction.Count = 1;
            log.ReverseReaction.Count = 1;
            log.Spawned.Count = 1;
            log.Power.Count = 1;

            var (count, counts, inflow, outflow) = QuestDBDtoHelper.GetCreepIntentsCounts(history);

            Assert.Equal(counts.Values.Sum(), count);
            Assert.Equal(2, counts["attack"]);
            Assert.Equal(1, counts["attacked"]);
            Assert.Equal(2, counts["ranged_attack"]);
            Assert.Equal(1, counts["ranged_mass_attacked"]);
            Assert.Equal(3, counts["ranged_heal"]);
            Assert.Equal(1, counts["heal"]);
            Assert.Equal(1, counts["healed"]);
            Assert.Equal(4, counts["harvest"]);
            Assert.Equal(2, counts["repair"]);
            Assert.Equal(1, counts["build"]);
            Assert.Equal(2, counts["upgrade_controller"]);
            Assert.Equal(3, counts["move"]);
            Assert.Equal(1, counts["say"]);
            Assert.Equal(1, counts["reserve_controller"]);
            Assert.Equal(1, counts["attack_controller"]);
            Assert.Equal(2, counts["produce"]);
            Assert.Equal(1, counts["transfer_energy"]);
            Assert.Equal(1, counts["run_reaction"]);
            Assert.Equal(1, counts["reverse_reaction"]);
            Assert.Equal(1, counts["spawned"]);
            Assert.Equal(1, counts["power"]);
        }

        [Fact]
        public void QuestDBDtoHelper_GetCreepIntentsCounts_ReturnsAllIntentKeys()
        {
            var history = new ScreepsRoomHistoryDto();
            var log = history.Creeps.OwnedCreeps.ActionLog;
            log.Attack.Count = 1;
            log.Attacked.Count = 1;
            log.RangedAttack.Count = 1;
            log.RangedMassAttack.Count = 1;
            log.RangedHeal.Count = 1;
            log.Heal.Count = 1;
            log.Healed.Count = 1;
            log.Harvest.Count = 1;
            log.Repair.Count = 1;
            log.Build.Count = 1;
            log.UpgradeController.Count = 1;
            log.Move.Count = 1;
            log.Say.Count = 1;
            log.ReserveController.Count = 1;
            log.AttackController.Count = 1;
            log.Produce.Count = 1;
            log.TransferEnergy.Count = 1;
            log.RunReaction.Count = 1;
            log.ReverseReaction.Count = 1;
            log.Spawned.Count = 1;
            log.Power.Count = 1;

            var (_, counts, inflow, outflow) = QuestDBDtoHelper.GetCreepIntentsCounts(history);
            var expectedKeys = new[]
            {
                "attack", "attacked", "ranged_attack", "ranged_mass_attacked",
                "ranged_heal", "heal", "healed", "harvest", "repair", "build",
                "upgrade_controller", "move", "say", "reserve_controller",
                "attack_controller", "produce", "transfer_energy",
                "run_reaction", "reverse_reaction", "spawned", "power"
            };

            Assert.Equal(expectedKeys.OrderBy(k => k), counts.Keys.OrderBy(k => k));
        }

        [Fact]
        public void QuestDBDtoHelper_GetStructureStoreCounts_AggregatesResources()
        {
            var store = new Store
            {
                energy = 100,
                battery = 10,
                H = 1,
                O = 2,
                U = 3,
                L = 4,
                K = 5,
                Z = 6,
                X = 7,
                G = 8,
                power = 9,
                ops = 2
            };

            var (total, map) = QuestDBDtoHelper.GetStructureStoreCounts(store);

            Assert.Equal(157, total);
            Assert.Equal(100, map["energy"]);
            Assert.Equal(10, map["battery"]);
            Assert.Equal(1, map["h"]);
            Assert.Equal(2, map["o"]);
            Assert.Equal(3, map["u"]);
            Assert.Equal(4, map["l"]);
            Assert.Equal(5, map["k"]);
            Assert.Equal(6, map["z"]);
            Assert.Equal(7, map["x"]);
            Assert.Equal(8, map["g"]);
            Assert.Equal(9, map["power"]);
            Assert.Equal(2, map["ops"]);
        }

        [Fact]
        public void QuestDBDtoHelper_GetStructureStoreCounts_IncludesEveryPart()
        {
            var expectedValues = new Dictionary<string, decimal>
            {
                ["energy"] = 10,
                ["battery"] = 20,
                ["h"] = 1,
                ["o"] = 2,
                ["u"] = 3,
                ["l"] = 4,
                ["k"] = 5,
                ["z"] = 6,
                ["x"] = 7,
                ["g"] = 8,
                ["power"] = 11,
                ["ops"] = 12
            };

            var store = new Store
            {
                energy = expectedValues["energy"],
                battery = expectedValues["battery"],
                H = expectedValues["h"],
                O = expectedValues["o"],
                U = expectedValues["u"],
                L = expectedValues["l"],
                K = expectedValues["k"],
                Z = expectedValues["z"],
                X = expectedValues["x"],
                G = expectedValues["g"],
                power = expectedValues["power"],
                ops = expectedValues["ops"]
            };

            var (_, totals) = QuestDBDtoHelper.GetStructureStoreCounts(store);

            Assert.Equal(expectedValues.Count, totals.Count);
            foreach (var kvp in expectedValues)
            {
                Assert.Equal(kvp.Value, totals[kvp.Key]);
            }
        }

        [Fact]
        public void QuestDBDtoHelper_GetStoreCounts_MergesAllLocations()
        {
            var history = new ScreepsRoomHistoryDto();
            history.Structures.Storage.Store.energy = 100;
            history.Structures.Storage.Store.battery = 10;
            history.Structures.Terminal.Store.energy = 50;
            history.Structures.Terminal.Store.battery = 5;
            history.Structures.Container.Store.energy = 25;
            history.Structures.Container.Store.battery = 2;
            history.Structures.Link.Energy = 40;

            var (total, map) = QuestDBDtoHelper.GetStoreCounts(history);

            Assert.Equal(232, total);
            Assert.Equal(215, map["energy"]);
            Assert.Equal(17, map["battery"]);
        }

        [Fact]
        public void QuestDBClientState_FromJsonFileProducesQuestDbDto()
        {
            var dto = LoadRoomHistoryDto("case1.json");
            var method = typeof(QuestDBClientState).GetMethod("GetQuestDBDto", BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(method);

            var questDto = (QuestDBHistoryDTO)method.Invoke(null, new object?[] { dto })!;

            var (structureCount, placedStructureCount, structureCounts) = QuestDBDtoHelper.GetStructureCounts(dto);
            Assert.Equal(structureCount, questDto.StructureCount);
            Assert.Equal(placedStructureCount, questDto.PlacedStructureCount);
            Assert.Equal(structureCounts, questDto.StructureCounts);

            var (creepCount, owned, enemy, other, power) = QuestDBDtoHelper.GetCreepCounts(dto);
            Assert.Equal(creepCount, questDto.CreepCount);
            Assert.Equal(owned, questDto.OwnedCreepCount);
            Assert.Equal(enemy, questDto.EnemyCreepCount);
            Assert.Equal(other, questDto.OtherCreepCount);
            Assert.Equal(power, questDto.PowerCreepCount);

            var (_, ownedParts) = QuestDBDtoHelper.GetCreepPartsCounts(dto);
            Assert.Equal(ownedParts, questDto.OwnedCreepPartsCounts);

            var (intentCount, intentCounts, inflow, outflow) = QuestDBDtoHelper.GetCreepIntentsCounts(dto);
            Assert.Equal(intentCount, questDto.CreepIntentCount);
            Assert.Equal(intentCounts, questDto.CreepIntentCounts);

            Assert.Equal(dto.Structures.Controller == null ? null : dto.Structures.Controller.Level, questDto.ControllerLevel);
            Assert.Equal(dto.Structures.Controller == null ? null : dto.Structures.Controller.Progress, questDto.ControllerProgress);
            Assert.Equal(dto.Structures.Controller == null ? null : dto.Structures.Controller.ProgressTotal, questDto.ControllerProgressTotal);
            Assert.Equal(dto.Structures.Controller == null ? null : dto.Structures.Controller.Upgraded, questDto.ControllerPointsPerTick);
            Assert.Equal(0, inflow);
            Assert.Equal(16, outflow);

            var (storeTotal, storeTotals) = QuestDBDtoHelper.GetStoreCounts(dto);
            Assert.Equal(storeTotal, questDto.StoreTotal);
            Assert.Equal(storeTotals, questDto.StoreTotals);
        }

        [Fact]
        public void QuestDBClientState_GetQuestDBDto_FullHistory()
        {
            var history = new ScreepsRoomHistoryDto();
            history.Structures.Controller.UserId = "player";
            history.Structures.Controller.Level = 5;
            history.Structures.Controller.Progress = 100;
            history.Structures.Controller.ProgressTotal = 500;
            history.Structures.Controller.Upgraded = 42;
            history.Structures.Controller.ScorePerTick = 9;
            history.Structures.Controller.ReservationUserId = null;
            history.Structures.Wall.Count = 18m;
            history.Structures.Container.Count = 3.2m;
            history.Structures.Storage.Count = 1.4m;
            history.Structures.Terminal.Count = 2.6m;
            history.Structures.Link.Count = 2.1m;
            history.Structures.Spawn.Count = 1.2m;
            history.Structures.Tower.Count = 2m;
            history.Structures.Road.Count = 5m;
            history.Structures.Mineral.Count = 1m;
            history.Structures.Deposit.Count = 1m;

            history.Creeps.OwnedCreeps.Count = 4;
            history.Creeps.EnemyCreeps.Count = 2;
            history.Creeps.OtherCreeps.Count = 1;
            history.Creeps.PowerCreeps.Count = 1;

            history.Creeps.OwnedCreeps.BodyParts.Move = 4;
            history.Creeps.OwnedCreeps.BodyParts.Work = 2;
            history.Creeps.OwnedCreeps.ActionLog.Attack.Count = 1;
            history.Creeps.OwnedCreeps.ActionLog.Move.Count = 3;
            history.Creeps.OwnedCreeps.ActionLog.Harvest.Count = 2;

            history.Structures.Storage.Store.energy = 100;
            history.Structures.Terminal.Store.energy = 50;
            history.Structures.Terminal.Store.battery = 5;
            history.Structures.Container.Store.power = 20;
            history.Structures.Link.Energy = 40;

            var method = typeof(QuestDBClientState).GetMethod("GetQuestDBDto", BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(method);
            var dto = (QuestDBHistoryDTO)method.Invoke(null, new object?[] { history })!;

            var (structureCount, placedStructureCount, structureCounts) = QuestDBDtoHelper.GetStructureCounts(history);
            Assert.Equal(structureCount, dto.StructureCount);
            Assert.Equal(placedStructureCount, dto.PlacedStructureCount);
            Assert.Equal(structureCounts, dto.StructureCounts);

            var (creepCount, owned, enemy, other, power) = QuestDBDtoHelper.GetCreepCounts(history);
            Assert.Equal(creepCount, dto.CreepCount);
            Assert.Equal(owned, dto.OwnedCreepCount);
            Assert.Equal(enemy, dto.EnemyCreepCount);
            Assert.Equal(other, dto.OtherCreepCount);
            Assert.Equal(power, dto.PowerCreepCount);

            var (ownedPartsCount, ownedParts) = QuestDBDtoHelper.GetCreepPartsCounts(history);
            Assert.Equal(ownedPartsCount, dto.OwnedCreepPartsCount);
            Assert.Equal(ownedParts, dto.OwnedCreepPartsCounts);

            var (intentCount, intents, inflow, outflow) = QuestDBDtoHelper.GetCreepIntentsCounts(history);
            Assert.Equal(intentCount, dto.CreepIntentCount);
            Assert.Equal(intents, dto.CreepIntentCounts);

            Assert.Equal(5, dto.ControllerLevel);
            Assert.Equal(100, dto.ControllerProgress);
            Assert.Equal(500, dto.ControllerProgressTotal);
            Assert.Equal(42, dto.ControllerPointsPerTick);
            Assert.Equal(9, dto.ControllerScorePerTick);

            var (storeTotal, storeTotals) = QuestDBDtoHelper.GetStoreCounts(history);
            Assert.Equal(storeTotal, dto.StoreTotal);
            Assert.Equal(storeTotals, dto.StoreTotals);
        }

        [Fact]
        public void QuestDBClientWriter_UploadRoomHistoryData_FlattensEveryNumericField()
        {
            var dto = CreateSampleQuestDto();
            var points = CaptureUploadedPoints(() =>
            {
                QuestDBClientWriter.UploadRoomHistoryData(
                    database: "server_room",
                    shard: "shard",
                    room: "E1N1",
                    tick: 123,
                    timestamp: 456,
                    username: "hero",
                    obj: dto);
            });

            Assert.NotEmpty(points);
            Assert.All(points, p => Assert.Equal("server_room", p.Measurement));
            Assert.All(points, p => Assert.Equal("shard", p.Shard));
            Assert.All(points, p => Assert.Equal("E1N1", p.Room));
            Assert.All(points, p => Assert.Equal("hero", p.Username));

            var fieldValues = points
                .GroupBy(p => p.Field)
                .ToDictionary(g => g.Key, g => g.Last().Value ?? double.NaN);

            foreach (var (field, expectedValue) in CreateExpectedFieldValues(dto))
            {
                Assert.True(fieldValues.TryGetValue(field, out var actualValue), $"Missing field {field}");
                Assert.Equal(expectedValue, actualValue);
            }
        }

        [Fact]
        public void QuestDBClientWriter_UploadUserHistoryData_UsesEmptyRoom()
        {
            var dto = CreateSampleQuestDto();
            var points = CaptureUploadedPoints(() =>
            {
                QuestDBClientWriter.UploadUserHistoryData(
                    database: "server_user",
                    shard: "shard",
                    tick: 999,
                    timestamp: 888,
                    username: "hero",
                    obj: dto);
            });

            Assert.NotEmpty(points);
            Assert.All(points, p => Assert.Equal("server_user", p.Measurement));
            Assert.All(points, p => Assert.Equal("shard", p.Shard));
            Assert.All(points, p => Assert.Equal(string.Empty, p.Room));
            Assert.All(points, p => Assert.Equal("hero", p.Username));

            var fields = points.Select(p => p.Field).ToHashSet();
            Assert.Contains("structurecount", fields);
            Assert.Contains("structurecounts_wall", fields);
            Assert.Contains("storetotals_energy", fields);
        }

        [Fact]
        public void QuestDBClientWriter_UploadGlobalHistoryData_UsesEmptyRoomAndUser()
        {
            var dto = CreateSampleQuestDto();
            var points = CaptureUploadedPoints(() =>
            {
                QuestDBClientWriter.UploadGlobalHistoryData(
                    database: "server_global",
                    shard: "shard",
                    tick: 777,
                    timestamp: 666,
                    obj: dto);
            });

            Assert.NotEmpty(points);
            Assert.All(points, p => Assert.Equal("server_global", p.Measurement));
            Assert.All(points, p => Assert.Equal("shard", p.Shard));
            Assert.All(points, p => Assert.Equal(string.Empty, p.Room));
            Assert.All(points, p => Assert.Equal(string.Empty, p.Username));
            Assert.Contains("structurecount", points.Select(p => p.Field));
        }

        [Fact]
        public void Projection_PreservesFractionalWindowAverages()
        {
            var history = new ScreepsRoomHistoryDto();
            history.Structures.Controller.Level = 2.37m;
            history.Structures.Controller.OwnedUserIdCount = 0.37m;
            history.Structures.Rampart.Count = 0.01m;
            history.Structures.Storage.Store.energy = 0.25m;
            history.Creeps.OwnedCreeps.Count = 0.01m;
            history.Creeps.OwnedCreeps.BodyParts.Work = 0.03m;
            history.Creeps.OwnedCreeps.ActionLog.Harvest.Count = 0.01m;
            history.Creeps.OwnedCreeps.ActionLog.Harvest.Inflow = 0.02m;
            history.Creeps.OwnedCreeps.ActionLog.Repair.Outflow = 0.04m;

            var dto = QuestDBClientState.GetQuestDBDto(history);
            var fields = CaptureUploadedPoints(() => QuestDBClientWriter.UploadRoomHistoryData(
                "test", "shard", "room", 100, 200, "user", dto)).ToDictionary(p => p.Field, p => p.Value);
            Assert.Equal(2.37, fields["controllerlevel"]);
            Assert.Equal(0.37, fields["ownedroomcount"]);
            Assert.Equal(0.01, fields["structurecounts_rampart"]);
            Assert.Equal(0.25, fields["storetotals_energy"]);
            Assert.Equal(0.01, fields["ownedcreepcount"]);
            Assert.Equal(0.03, fields["ownedcreeppartscounts_work"]);
            Assert.Equal(0.01, fields["creepintentcounts_harvest"]);
            Assert.Equal(0.02, fields["creepenergyinflow"]);
            Assert.Equal(0.04, fields["creepenergyoutflow"]);
        }

        [Theory]
        [InlineData(false, "room")]
        [InlineData(false, "user")]
        [InlineData(false, "global")]
        [InlineData(true, "room")]
        [InlineData(true, "user")]
        [InlineData(true, "global")]
        public void DetailedProjection_IsOptInForEveryHistoryTable(bool detailed, string table)
        {
            var previous = ConfigSettingsState.QuestDbDetailedEnabled;
            ConfigSettingsState.QuestDbDetailedEnabled = detailed;
            try
            {
                var history = new ScreepsRoomHistoryDto();
                history.Structures.Wall.Hits = 100.5m;
                history.Structures.Rampart.Hits = 200.25m;
                history.Structures.Extension.EnergyCapacity = 50;
                history.Structures.Link.Energy = 10;
                history.Structures.Tower.Energy = 20;
                history.Structures.Source.Energy = 30;
                history.Structures.ConstructionSite.Progress = 40;
                history.Structures.ConstructionSite.ProgressTotal = 500;
                history.Structures.ConstructionSite.TypesBuilding["spawn"] = 0.5m;
                history.Structures.Storage.Store.XGH2O = 1.25m;
                history.Structures.Terminal.Store.XGH2O = 2.5m;
                history.Structures.Container.Store.XGH2O = 3.75m;
                history.Creeps.OwnedCreeps.ActionLog.Attack.Damage = 9;
                history.Creeps.EnemyCreeps.ActionLog.Heal.Heal = 8;
                history.Creeps.OtherCreeps.ActionLog.Build.Outflow = 7;
                history.Creeps.PowerCreeps.ActionLog.Repair.Effect = 6;
                history.GroundResources["energy"] = 12.5m;
                var dto = QuestDBClientState.GetQuestDBDto(history);
                var fields = CaptureUploadedPoints(() =>
                {
                    if (table == "room") QuestDBClientWriter.UploadRoomHistoryData("test", "shard", "room", 1, 2, "user", dto);
                    else if (table == "user") QuestDBClientWriter.UploadUserHistoryData("test", "shard", 1, 2, "user", dto);
                    else QuestDBClientWriter.UploadGlobalHistoryData("test", "shard", 1, 2, dto);
                }).ToDictionary(p => p.Field, p => p.Value);

                Assert.Contains("structurecount", fields.Keys);
                Assert.Contains("storetotals_energy", fields.Keys);
                if (!detailed)
                {
                    Assert.DoesNotContain(fields.Keys, k => k.StartsWith("structures_") || k.StartsWith("creeps_") || k.StartsWith("groundresources_"));
                    Assert.DoesNotContain("storetotals_xgh2o", fields.Keys);
                    return;
                }

                var expected = new Dictionary<string, double>
                {
                    ["structures_wall_hits"] = 100.5, ["structures_rampart_hits"] = 200.25,
                    ["structures_extension_energycapacity"] = 50, ["structures_link_energy"] = 10,
                    ["structures_tower_energy"] = 20, ["structures_source_energy"] = 30,
                    ["structures_constructionsite_progress"] = 40, ["structures_constructionsite_progresstotal"] = 500,
                    ["structures_constructionsite_typesbuilding_spawn"] = 0.5,
                    ["structures_storage_store_xgh2o"] = 1.25, ["structures_terminal_store_xgh2o"] = 2.5,
                    ["structures_container_store_xgh2o"] = 3.75, ["storetotals_xgh2o"] = 7.5,
                    ["creeps_ownedcreeps_actionlog_attack_damage"] = 9,
                    ["creeps_enemycreeps_actionlog_heal_heal"] = 8,
                    ["creeps_othercreeps_actionlog_build_outflow"] = 7,
                    ["creeps_powercreeps_actionlog_repair_effect"] = 6,
                    ["groundresources_energy"] = 12.5
                };
                foreach (var (key, value) in expected) Assert.Equal(value, fields[key]);
                foreach (var property in typeof(Store).GetProperties())
                    Assert.Contains("storetotals_" + property.Name.ToLowerInvariant(), fields.Keys);
                Assert.Equal(0, fields["storetotals_xuh2o"]);
            }
            finally { ConfigSettingsState.QuestDbDetailedEnabled = previous; }
        }

        [Fact]
        public async Task ConcurrentUploads_EnqueueCompleteRowsIncludingDistinctTicksAtSameTimestamp()
        {
            var channel = InitializeHistoryChannel();
            try
            {
                await Task.WhenAll(Enumerable.Range(0, 100).Select(tick => Task.Run(() =>
                    QuestDBClientWriter.UploadRoomHistoryData("test", "shard", "room", tick, 123, "user", CreateSampleQuestDto()))));
                var ticks = new HashSet<long>();
                var expected = CreateExpectedFieldValues(CreateSampleQuestDto());
                while (channel.Reader.TryRead(out var row))
                {
                    Assert.True(ticks.Add(row[0].Tick));
                    Assert.All(row, p => Assert.Equal(row[0].Tick, p.Tick));
                    var fields = row.ToDictionary(p => p.Field, p => p.Value);
                    foreach (var (key, value) in expected) Assert.Equal(value, fields[key]);
                }
                Assert.Equal(100, ticks.Count);
            }
            finally { ResetHistoryChannel(); }
        }

        private static ScreepsRoomHistoryDto LoadRoomHistoryDto(string fileName)
        {
            EnsureConfigInitialized();
            var filePath = Path.Combine(AppContext.BaseDirectory, "Files", fileName);
            using var reader = new StreamReader(filePath);
            using var jsonReader = new JsonTextReader(reader);
            var roomData = JObject.Load(jsonReader);
            return ProcessRoomHistory(roomData);
        }

        private static ScreepsRoomHistoryDto ProcessRoomHistory(JObject roomData)
        {
            var roomHistory = new ScreepsRoomHistory();
            var roomHistoryDto = new ScreepsRoomHistoryDto();

            roomData.TryGetValue("timestamp", out JToken? jTokenTime);
            if (jTokenTime != null) roomHistory.TimeStamp = jTokenTime.Value<long>();
            roomData.TryGetValue("base", out JToken? jTokenBase);
            if (jTokenBase != null) roomHistory.Base = jTokenBase.Value<long>();

            if (roomData.TryGetValue("ticks", out JToken? jTokenTicks) && jTokenTicks is JObject jObjectTicks)
            {
                for (int i = 0; i < ConfigSettingsState.TicksInFile; i++)
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

        private static void EnsureConfigInitialized()
        {
            if (_configInitialized) return;
            var configFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = "App.config"
            };
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            ConfigSettingsState.InitTest(configuration.AppSettings);
            _configInitialized = true;
        }

        private static QuestDBHistoryDTO CreateSampleQuestDto()
        {
            return new QuestDBHistoryDTO
            {
                StructureCount = 3,
                PlacedStructureCount = 4,
                StructureCounts = new Dictionary<string, decimal>
                {
                    ["wall"] = 2,
                    ["extension"] = 1
                },
                CreepCount = 6,
                OwnedCreepCount = 2,
                EnemyCreepCount = 1,
                OtherCreepCount = 2,
                PowerCreepCount = 1,
                OwnedCreepPartsCount = 6,
                OwnedCreepPartsCounts = new Dictionary<string, decimal>
                {
                    ["move"] = 3,
                    ["work"] = 3
                },
                CreepIntentCount = 5,
                CreepIntentCounts = new Dictionary<string, decimal>
                {
                    ["move"] = 3,
                    ["attack"] = 2
                },
                OwnedRoomCount = 1,
                ReservedRoomCount = 0,
                ControllerLevel = 7,
                ControllerProgress = 200,
                ControllerProgressTotal = 1000,
                ControllerPointsPerTick = 3,
                ControllerScorePerTick = 5,
                StoreTotal = 600,
                StoreTotals = new Dictionary<string, decimal>
                {
                    ["energy"] = 500,
                    ["battery"] = 100
                }
            };
        }

        private static IReadOnlyList<QuestHistoryPointDataParameter> CaptureUploadedPoints(Action uploadAction)
        {
            var channel = InitializeHistoryChannel();
            try
            {
                uploadAction();
                return ReadHistoryChannel(channel);
            }
            finally
            {
                ResetHistoryChannel();
            }
        }

        private static Channel<IReadOnlyList<QuestHistoryPointDataParameter>> InitializeHistoryChannel()
        {
            var channel = Channel.CreateUnbounded<IReadOnlyList<QuestHistoryPointDataParameter>>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true
            });

            SetPrivateField("_historyChannel", channel);
            SetPrivateField("_pendingPointCount", 0L);
            return channel;
        }

        private static void ResetHistoryChannel()
        {
            SetPrivateField("_historyChannel", null);
            SetPrivateField("_pendingPointCount", 0L);
        }

        private static void SetPrivateField(string fieldName, object? value)
        {
            var field = typeof(QuestDBClientWriter).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(field);
            field!.SetValue(null, value);
        }

        private static List<QuestHistoryPointDataParameter> ReadHistoryChannel(Channel<IReadOnlyList<QuestHistoryPointDataParameter>> channel)
        {
            var points = new List<QuestHistoryPointDataParameter>();
            while (channel.Reader.TryRead(out var point))
            {
                points.AddRange(point);
            }

            return points;
        }

        private static IReadOnlyDictionary<string, double> CreateExpectedFieldValues(QuestDBHistoryDTO dto)
        {
            return new Dictionary<string, double>
            {
                ["structurecount"] = (double)(dto.StructureCount),
                ["placedstructurecount"] = (double)(dto.PlacedStructureCount),
                ["structurecounts_wall"] = (double)(dto.StructureCounts["wall"]),
                ["structurecounts_extension"] = (double)(dto.StructureCounts["extension"]),
                ["creepcount"] = (double)(dto.CreepCount),
                ["ownedcreepcount"] = (double)(dto.OwnedCreepCount),
                ["enemycreepcount"] = (double)(dto.EnemyCreepCount),
                ["othercreepcount"] = (double)(dto.OtherCreepCount),
                ["powercreepcount"] = (double)(dto.PowerCreepCount),
                ["ownedcreeppartscount"] = (double)(dto.OwnedCreepPartsCount),
                ["ownedcreeppartscounts_move"] = (double)(dto.OwnedCreepPartsCounts["move"]),
                ["ownedcreeppartscounts_work"] = (double)(dto.OwnedCreepPartsCounts["work"]),
                ["creepintentcount"] = (double)(dto.CreepIntentCount),
                ["creepintentcounts_move"] = (double)(dto.CreepIntentCounts["move"]),
                ["creepintentcounts_attack"] = (double)(dto.CreepIntentCounts["attack"]),
                ["ownedroomcount"] = (double)(dto.OwnedRoomCount),
                ["reservedroomcount"] = (double)(dto.ReservedRoomCount),
                ["controllerlevel"] = (double)(dto.ControllerLevel ?? 0),
                ["controllerprogress"] = (double)(dto.ControllerProgress ?? 0),
                ["controllerprogresstotal"] = (double)(dto.ControllerProgressTotal ?? 0),
                ["controllerpointspertick"] = (double)(dto.ControllerPointsPerTick ?? 0),
                ["controllerscorepertick"] = (double)(dto.ControllerScorePerTick ?? 0),
                ["storetotal"] = (double)(dto.StoreTotal),
                ["storetotals_energy"] = (double)(dto.StoreTotals["energy"]),
                ["storetotals_battery"] = (double)(dto.StoreTotals["battery"])
            };
        }

        [Fact]
        public void QuestDBPointHelper_UpdateHistoryPoint_RegistersColumns()
        {
            var sender = new FakeSender();
            var parameter = new QuestHistoryPointDataParameter(
                database: "db",
                shard: "shard",
                room: "E1N1",
                tick: 123,
                timestamp: 321,
                username: "player",
                field: "store.energy",
                value: 42.5);

            var returned = QuestDBPointHelper.UpdateHistoryPoint(sender, parameter);

            Assert.Same(sender, returned);
            Assert.Single(sender.NullableColumnCalls);
            Assert.Equal("store_energy", sender.NullableColumnCalls[0].Field);
            Assert.Equal(42.5, sender.NullableColumnCalls[0].Value);
        }

        [Fact]
        public async Task QuestDBPointHelper_InsertAdminUtilsPoint_FlowsThroughSymbols()
        {
            var sender = new FakeSender();
            var parameter = new QuestAdminUtilsPointDataParameter(
                database: "admin",
                username: "hero",
                field: "metrics.cpu",
                value: 5.5);

            await QuestDBPointHelper.InsertAdminUtilsPoint(sender, parameter);

            Assert.Equal("admin", sender.TableName);
            Assert.Contains(sender.SymbolCalls, call => call.Key == "field" && call.Value == "metrics_cpu");
            Assert.Contains(sender.SymbolCalls, call => call.Key == "user" && call.Value == "hero");
            Assert.Single(sender.NullableColumnCalls);
            Assert.Equal("value", sender.NullableColumnCalls[0].Field);
            Assert.Equal(5.5, sender.NullableColumnCalls[0].Value);
            Assert.True(sender.LastTimestamp.HasValue);
        }

        private sealed class FakeSender : ISender
        {
            public string? TableName { get; private set; }
            public List<(string Field, double? Value)> NullableColumnCalls { get; } = new();
            public List<(string Key, string Value)> SymbolCalls { get; } = new();
            public long? LastTimestamp { get; private set; }

            private readonly SenderOptions _options = new();

            public int Length => 0;
            public int RowCount => 0;
            public bool WithinTransaction => false;
            public DateTime LastFlush => DateTime.UtcNow;
            public SenderOptions Options => _options;

            public ISender Transaction(ReadOnlySpan<char> tableName) => this;
            public Task CommitAsync(CancellationToken cancellationToken) => Task.CompletedTask;
            public void Commit(CancellationToken cancellationToken) { }
            public Task SendAsync(CancellationToken cancellationToken) => Task.CompletedTask;
            public void Send(CancellationToken cancellationToken) { }
            public void Rollback() { }
            public ISender Table(ReadOnlySpan<char> tableName)
            {
                TableName = tableName.ToString();
                return this;
            }

            public ISender Symbol(ReadOnlySpan<char> key, ReadOnlySpan<char> value)
            {
                SymbolCalls.Add((key.ToString(), value.ToString()));
                return this;
            }

            public ISender Column(ReadOnlySpan<char> name, ReadOnlySpan<char> value) => this;
            public ISender Column(ReadOnlySpan<char> name, Array? values) => this;
            public ISender Column(ReadOnlySpan<char> name, string? value) => this;
            public ISender Column(ReadOnlySpan<char> name, long value) => this;
            public ISender Column(ReadOnlySpan<char> name, bool value) => this;
            public ISender Column(ReadOnlySpan<char> name, double value) => this;
            public ISender Column(ReadOnlySpan<char> name, DateTime value) => this;
            public ISender Column(ReadOnlySpan<char> name, DateTimeOffset value) => this;
            // ISenderV2 generic column implementations are provided below.

            public ISender NullableColumn(ReadOnlySpan<char> name, double? value)
            {
                NullableColumnCalls.Add((name.ToString(), value));
                return this;
            }

            public ISender NullableColumn(ReadOnlySpan<char> name, long? value) => this;
            public ISender NullableColumn(ReadOnlySpan<char> name, bool? value) => this;
            public ISender NullableColumn(ReadOnlySpan<char> name, DateTime? value) => this;
            public ISender NullableColumn(ReadOnlySpan<char> name, DateTimeOffset? value) => this;
            public ISender NullableColumn(ReadOnlySpan<char> name, string? value) => this;
            public ISender NullableColumn(ReadOnlySpan<char> name, Array? values) => this;
            // ISenderV2 generic nullable column implementations are provided below.

            public ValueTask AtAsync(DateTime timestamp, CancellationToken cancellationToken)
            {
                LastTimestamp = timestamp.ToUniversalTime().Ticks;
                return ValueTask.CompletedTask;
            }

            public ValueTask AtAsync(DateTimeOffset timestamp, CancellationToken cancellationToken)
            {
                LastTimestamp = timestamp.ToUnixTimeMilliseconds();
                return ValueTask.CompletedTask;
            }

            public ValueTask AtAsync(long epochNano, CancellationToken cancellationToken)
            {
                LastTimestamp = epochNano;
                return ValueTask.CompletedTask;
            }

            public ValueTask AtNowAsync(CancellationToken cancellationToken)
            {
                LastTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                return ValueTask.CompletedTask;
            }

            public void At(DateTime timestamp, CancellationToken cancellationToken)
            {
                LastTimestamp = timestamp.ToUniversalTime().Ticks;
            }

            public void At(DateTimeOffset timestamp, CancellationToken cancellationToken)
            {
                LastTimestamp = timestamp.ToUnixTimeMilliseconds();
            }

            public void At(long epochNano, CancellationToken cancellationToken)
            {
                LastTimestamp = epochNano;
            }

            public void AtNow(CancellationToken cancellationToken)
            {
                LastTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            public void Truncate() { }
            public void CancelRow() { }
            public void Clear() { }
            ISender ISenderV2.Column<T>(ReadOnlySpan<char> name, ReadOnlySpan<T> value) => this;
            ISender ISenderV2.Column<T>(ReadOnlySpan<char> name, IEnumerable<T>? values, IEnumerable<int>? indexes) => this;
            ISender ISenderV2.NullableColumn<T>(ReadOnlySpan<char> name, IEnumerable<T>? values, IEnumerable<int>? indexes) => this;
            public void Dispose() { }
        }
    }
}
