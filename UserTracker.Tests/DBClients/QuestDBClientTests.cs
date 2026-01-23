using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
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

            var expectedStructureCount = 1 + expectedStructureKeys.Sum(key => structureCounts[key]);
            var expectedPlacedStructureCount = expectedPlacedKeys.Sum(key => structureCounts[key]);

            Assert.Equal(expectedStructureCount, structureCount);
            Assert.Equal(expectedPlacedStructureCount, placedStructureCount);
            Assert.Equal(12, structureCounts["wall"]);
            Assert.Equal(3, structureCounts["container"]);
            Assert.Equal(5, structureCounts["extension"]);
            Assert.Equal(6, structureCounts["rampart"]);
            Assert.Equal(3, structureCounts["link"]);
            Assert.Equal(4, structureCounts["powerspawn"]);
            Assert.Equal(10, structureCounts["road"]);
            Assert.Equal(3, structureCounts["spawn"]);
            Assert.Equal(1, structureCounts["storage"]);
            Assert.Equal(2, structureCounts["terminal"]);
            Assert.Equal(2, structureCounts["tower"]);
            Assert.Equal(1, structureCounts["nuker"]);
        }

        [Fact]
        public void QuestDBDtoHelper_IncludesControllerMineralAndDepositCounts()
        {
            var history = new ScreepsRoomHistoryDto();
            history.Structures.Controller.UserId = "player";
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
            var method = typeof(QuestDBClientState).GetMethod("GetQuestDBDto", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(method);

            var history = new ScreepsRoomHistoryDto();
            history.Structures.Controller.Upgraded = 123;

            var dto = (QuestDBHistoryDTO)method.Invoke(null, new object?[] { history })!;
            Assert.Equal(123, dto.ControllerPointsPerTick);
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

            var (count, counts) = QuestDBDtoHelper.GetCreepIntentsCounts(history);

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

            var (_, counts) = QuestDBDtoHelper.GetCreepIntentsCounts(history);
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
        public void QuestDBDtoHelper_GetRoomCounts_DetectsReservedAndOtherRooms()
        {
            var reservedHistory = new ScreepsRoomHistoryDto();
            reservedHistory.Structures.Controller.ReservationUserId = "runner";
            var reserved = QuestDBDtoHelper.GetRoomCounts(reservedHistory);
            Assert.Equal(0, reserved.Item1);
            Assert.Equal(1, reserved.Item2);
            Assert.Equal(0, reserved.Item3);

            var ownedHistory = new ScreepsRoomHistoryDto();
            ownedHistory.Structures.Controller.ReservationUserId = null;
            var owned = QuestDBDtoHelper.GetRoomCounts(ownedHistory);
            Assert.Equal(1, owned.Item1);
            Assert.Equal(0, owned.Item2);
            Assert.Equal(0, owned.Item3);

            var otherHistory = new ScreepsRoomHistoryDto();
            otherHistory.Structures.Controller = null!;
            var other = QuestDBDtoHelper.GetRoomCounts(otherHistory);
            Assert.Equal(0, other.Item1);
            Assert.Equal(0, other.Item2);
            Assert.Equal(1, other.Item3);
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
            var expectedValues = new Dictionary<string, int>
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
            var method = typeof(QuestDBClientState).GetMethod("GetQuestDBDto", BindingFlags.Static | BindingFlags.NonPublic);
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

            var (intentCount, intentCounts) = QuestDBDtoHelper.GetCreepIntentsCounts(dto);
            Assert.Equal(intentCount, questDto.CreepIntentCount);
            Assert.Equal(intentCounts, questDto.CreepIntentCounts);

            var (ownedRoomCount, reservedRoomCount, otherRoomCount) = QuestDBDtoHelper.GetRoomCounts(dto);
            Assert.Equal(ownedRoomCount, questDto.OwnedRoomCount);
            Assert.Equal(reservedRoomCount, questDto.ReservedRoomCount);
            Assert.Equal(otherRoomCount, questDto.OtherRoomCount);

            Assert.Equal(dto.Structures.Controller == null ? null : Convert.ToInt32(dto.Structures.Controller.Level), questDto.ControllerLevel);
            Assert.Equal(dto.Structures.Controller == null ? null : Convert.ToInt32(dto.Structures.Controller.Progress), questDto.ControllerProgress);
            Assert.Equal(dto.Structures.Controller == null ? null : Convert.ToInt32(dto.Structures.Controller.ProgressTotal), questDto.ControllerProgressTotal);
            Assert.Equal(dto.Structures.Controller == null ? null : Convert.ToInt32(dto.Structures.Controller.Upgraded), questDto.ControllerPointsPerTick);

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

            var method = typeof(QuestDBClientState).GetMethod("GetQuestDBDto", BindingFlags.Static | BindingFlags.NonPublic);
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

            var (intentCount, intents) = QuestDBDtoHelper.GetCreepIntentsCounts(history);
            Assert.Equal(intentCount, dto.CreepIntentCount);
            Assert.Equal(intents, dto.CreepIntentCounts);

            var (ownedRooms, reservedRooms, otherRooms) = QuestDBDtoHelper.GetRoomCounts(history);
            Assert.Equal(ownedRooms, dto.OwnedRoomCount);
            Assert.Equal(reservedRooms, dto.ReservedRoomCount);
            Assert.Equal(otherRooms, dto.OtherRoomCount);

            Assert.Equal(5, dto.ControllerLevel);
            Assert.Equal(100, dto.ControllerProgress);
            Assert.Equal(500, dto.ControllerProgressTotal);
            Assert.Equal(42, dto.ControllerPointsPerTick);

            var (storeTotal, storeTotals) = QuestDBDtoHelper.GetStoreCounts(history);
            Assert.Equal(storeTotal, dto.StoreTotal);
            Assert.Equal(storeTotals, dto.StoreTotals);
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
                ExeConfigFilename = "App.Config"
            };
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            ConfigSettingsState.InitTest(configuration.AppSettings);
            _configInitialized = true;
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
