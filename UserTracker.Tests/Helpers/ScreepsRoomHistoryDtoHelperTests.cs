using System.Configuration;
using System;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerShared.States;
using Xunit;

namespace UserTracker.Tests.Helpers;

public class ScreepsRoomHistoryDtoHelperTests
{
    public ScreepsRoomHistoryDtoHelperTests()
    {
        var configFileMap = new ExeConfigurationFileMap
        {
            ExeConfigFilename = "App.Config"
        };
        var configuration = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
        ConfigSettingsState.InitTest(configuration.AppSettings);
    }

    [Fact]
    public void ConvertBody_CountsAllParts()
    {
        var body = new[]
        {
            new BodyPart { Type = "move" },
            new BodyPart { Type = "work" },
            new BodyPart { Type = "carry" },
            new BodyPart { Type = "attack" },
            new BodyPart { Type = "ranged_attack" },
            new BodyPart { Type = "tough" },
            new BodyPart { Type = "heal" },
            new BodyPart { Type = "claim" },
            new BodyPart { Type = "unknown" }
        };

        var counts = new CountByPartDto();
        ScreepsRoomHistoryDtoHelper.ConvertBody(body, counts);

        Assert.Equal(1, counts.Move);
        Assert.Equal(1, counts.Work);
        Assert.Equal(1, counts.Carry);
        Assert.Equal(1, counts.Attack);
        Assert.Equal(1, counts.RangedAttack);
        Assert.Equal(1, counts.Tough);
        Assert.Equal(1, counts.Heal);
        Assert.Equal(1, counts.Claim);
    }

    [Fact]
    public void ComputeExtraIntentPower_AccumulatesBoostEffects()
    {
        var body = new[]
        {
            new BodyPart { Type = "work", Boost = "UO", Hits = 1 },
            new BodyPart { Type = "work", Boost = "LH", Hits = 1 },
            new BodyPart { Type = "attack", Boost = "UH2O", Hits = 1 },
            new BodyPart { Type = "ranged_attack", Boost = "KO", Hits = 1 },
            new BodyPart { Type = "heal", Boost = "LO", Hits = 1 },
            new BodyPart { Type = "work", Boost = "XUHO2", Hits = 0 }
        };

        var intent = new ScreepsRoomHistoryDtoHelper.IntentMapDto();
        ScreepsRoomHistoryDtoHelper.ComputeExtraIntentPower(body, new CountByPartDto(), intent);

        Assert.Equal(2m, intent.Harvest);
        Assert.Equal(0.5m, intent.Build);
        Assert.Equal(0.5m, intent.Repair);
        Assert.Equal(60m, intent.Attack);
        Assert.Equal(20m, intent.RangedAttack);
        Assert.Equal(12m, intent.Heal);
        Assert.Equal(4m, intent.RangedHeal);
    }

    [Fact]
    public void CombineStores_SumsNullableValues()
    {
        var storeA = new Store { energy = 1, power = 2 };
        var storeB = new Store { energy = 3, power = 4 };

        var result = ScreepsRoomHistoryDtoHelper.CombineStores(storeA, storeB);

        Assert.Equal(4m, result.energy);
        Assert.Equal(6m, result.power);
    }

    [Fact]
    public void CombineCountByParts_SumsAllFields()
    {
        var a = new CountByPartDto { Move = 1, Work = 2, Carry = 3 };
        var b = new CountByPartDto { Move = 4, Work = 5, Carry = 6 };

        var result = ScreepsRoomHistoryDtoHelper.CombineCountByParts(a, b);

        Assert.Equal(5, result.Move);
        Assert.Equal(7, result.Work);
        Assert.Equal(9, result.Carry);
    }

    [Fact]
    public void CombineActionLogs_SumsNestedValues()
    {
        var a = new ActionLogDto();
        var b = new ActionLogDto();

        a.Attack.Count = 1;
        a.Attack.Damage = 2;
        a.Harvest.Count = 1;
        a.Harvest.Inflow = 3;
        a.Move.Count = 4;

        b.Attack.Count = 2;
        b.Attack.Damage = 3;
        b.Harvest.Count = 4;
        b.Harvest.Inflow = 5;
        b.Move.Count = 6;

        var result = ScreepsRoomHistoryDtoHelper.CombineActionLogs(a, b);

        Assert.Equal(3, result.Attack.Count);
        Assert.Equal(5, result.Attack.Damage);
        Assert.Equal(5, result.Harvest.Count);
        Assert.Equal(8, result.Harvest.Inflow);
        Assert.Equal(10, result.Move.Count);
    }

    [Fact]
    public void ConvertActiongLog_UpdatesDamageAndGenericCounts()
    {
        var ticks = ConfigSettingsState.TicksInObject;
        var actionLog = new ActionLog
        {
            Attack = new Coordinate(),
            Heal = new Coordinate(),
            Harvest = new Coordinate(),
            Build = new Coordinate(),
            Say = new SayAction(),
            TransferEnergy = new Coordinate()
        };
        var actionLogDto = new ActionLogDto();
        var body = new CountByPartDto { Attack = 2, Heal = 1, Work = 3 };
        var intent = new ScreepsRoomHistoryDtoHelper.IntentMapDto
        {
            Attack = 1,
            Heal = 1,
            Harvest = 2,
            Build = 1
        };

        ScreepsRoomHistoryDtoHelper.ConvertActiongLog(actionLog, actionLogDto, body, intent, 0);

        Assert.Equal(1m / ticks, actionLogDto.Attack.Count);
        Assert.Equal(90m / ticks, actionLogDto.Attack.Damage);
        Assert.Equal(1m / ticks, actionLogDto.Heal.Count);
        Assert.Equal(24m / ticks, actionLogDto.Heal.Heal);
        Assert.Equal(1m / ticks, actionLogDto.Harvest.Count);
        Assert.Equal(10m, actionLogDto.Harvest.Inflow);
        Assert.Equal(1m / ticks, actionLogDto.Build.Count);
        Assert.Equal(3m / ticks, actionLogDto.Build.Outflow);
        Assert.Equal(20m / ticks, actionLogDto.Build.Effect);
        Assert.Equal(1m, actionLogDto.Move.Count);
        Assert.Equal(1m / ticks, actionLogDto.Say.Count);
        Assert.Equal(1m / ticks, actionLogDto.TransferEnergy.Count);
    }
}

public class ScreepsRoomHistoryDtoHelperStructuresTests
{
    public ScreepsRoomHistoryDtoHelperStructuresTests()
    {
        var configFileMap = new ExeConfigurationFileMap
        {
            ExeConfigFilename = "App.Config"
        };
        var configuration = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
        ConfigSettingsState.InitTest(configuration.AppSettings);
    }

    [Fact]
    public void ConvertStructures_UpdatesCountsAndStores()
    {
        var ticks = ConfigSettingsState.TicksInObject;
        var structures = new Structures
        {
            Controller = new StructureController
            {
                Level = 2,
                Progress = 10,
                ProgressTotal = 20,
                User = "owner",
                _upgraded = 5
            },
            Mineral = new StructureMineral(),
        };
        structures.Deposits["dep1"] = new StructureDeposit();
        structures.Walls["wall1"] = new StructureWall { Hits = 100 };
        structures.ConstructionSites["site1"] = new StructureConstructionSite { StructureType = "spawn", Progress = 4, ProgressTotal = 10 };
        structures.Containers["cont1"] = new StructureContainer { Store = new Store { energy = 100 } };
        structures.Extensions["ext1"] = new StructureExtension
        {
            Store = new Store { energy = 50 },
            StoreCapacityResource = new Store { energy = 200 }
        };
        structures.Links["link1"] = new StructureLink
        {
            Store = new Store { energy = 40 },
            StoreCapacityResource = new Store { energy = 80 }
        };
        structures.Sources["source1"] = new StructureSource { Energy = 300, EnergyCapacity = 600 };
        structures.Towers["tower1"] = new StructureTower { Store = new Store { energy = 25 } };

        var dto = new StructuresDto();
        ScreepsRoomHistoryDtoHelper.ConvertStructures(structures, dto);

        Assert.Equal(1m / ticks, dto.Controller.Count);
        Assert.Equal(2m / ticks, dto.Controller.Level);
        Assert.Equal(10m / ticks, dto.Controller.Progress);
        Assert.Equal(20m / ticks, dto.Controller.ProgressTotal);
        Assert.Equal("owner", dto.Controller.UserId);
        Assert.Equal(5m / ticks, dto.Controller.Upgraded);

        Assert.Equal(1m / ticks, dto.Mineral.Count);
        Assert.Equal(1m / ticks, dto.Deposit.Count);
        Assert.Equal(1m / ticks, dto.Wall.Count);
        Assert.Equal(100m / ticks, dto.Wall.Hits);
        Assert.Equal(1m / ticks, dto.ConstructionSite.Count);
        Assert.True(dto.ConstructionSite.TypesBuilding.ContainsKey("spawn"));
        Assert.Equal(1m / ticks, dto.Container.Count);
        Assert.Equal(100m / ticks, dto.Container.Store.energy);
        Assert.Equal(1m / ticks, dto.Extension.Count);
        Assert.Equal(50m / ticks, dto.Extension.Energy);
        Assert.Equal(200m / ticks, dto.Extension.EnergyCapacity);
        Assert.Equal(1m / ticks, dto.Link.Count);
        Assert.Equal(40m / ticks, dto.Link.Energy);
        Assert.Equal(80m / ticks, dto.Link.EnergyCapacity);
        Assert.Equal(1m / ticks, dto.Source.Count);
        Assert.Equal(300m / ticks, dto.Source.Energy);
        Assert.Equal(600m / ticks, dto.Source.EnergyCapacity);
        Assert.Equal(1m / ticks, dto.Tower.Count);
        Assert.Equal(25m / ticks, dto.Tower.Energy);
    }

    [Fact]
    public void CombineStructures_MergesCountsAndStores()
    {
        var a = new StructuresDto();
        var b = new StructuresDto();

        a.Controller.Count = 1;
        a.Controller.Level = 2;
        a.Controller.UserId = "first";
        a.ConstructionSite.TypesBuilding["spawn"] = 1;
        a.Container.Count = 1;
        a.Container.Store.energy = 5;
        a.Storage.Store.energy = 10;

        b.Controller.Count = 2;
        b.Controller.Level = 3;
        b.Controller.UserId = "second";
        b.ConstructionSite.TypesBuilding["extension"] = 2;
        b.Container.Count = 4;
        b.Container.Store.energy = 6;
        b.Storage.Store.energy = 7;

        ScreepsRoomHistoryDtoHelper.CombineStructures(a, b);

        Assert.Equal(3, a.Controller.Count);
        Assert.Equal(5, a.Controller.Level);
        Assert.Equal("first", a.Controller.UserId);
        Assert.Equal(2, a.ConstructionSite.TypesBuilding.Count);
        Assert.Equal(1, a.ConstructionSite.TypesBuilding["spawn"]);
        Assert.Equal(2, a.ConstructionSite.TypesBuilding["extension"]);
        Assert.Equal(5, a.Container.Count);
        var ticks = ConfigSettingsState.TicksInObject;
        Assert.Equal(5m + (6m / ticks), a.Container.Store.energy);
        Assert.Equal(10m + (7m / ticks), a.Storage.Store.energy);
    }

    [Fact]
    public void CombineCreeps_SumsCountsAndStores()
    {
        var a = new CreepDto { Count = 1, Store = new Store { energy = 2 }, BodyParts = new CountByPartDto { Move = 1 } };
        var b = new CreepDto { Count = 3, Store = new Store { energy = 4 }, BodyParts = new CountByPartDto { Move = 2 } };

        var result = ScreepsRoomHistoryDtoHelper.CombineCreeps(a, b);

        Assert.Equal(4, result.Count);
        Assert.Equal(6, result.Store.energy);
        Assert.Equal(3, result.BodyParts.Move);
    }
}
