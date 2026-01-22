using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using Xunit;

namespace UserTracker.Tests.Helpers;

public class ScreepsRoomHistoryHelperTests
{
    [Fact]
    public void UpdateRoomHistory_CreatesControllerAndPatchesLevel()
    {
        var roomHistory = new ScreepsRoomHistory();
        roomHistory.TypeMap["ctrl"] = "controller";

        var changes = new Dictionary<string, object?>
        {
            ["level"] = 5,
            ["user"] = "owner"
        };

        ScreepsRoomHistoryHelper.UpdateRoomHistory("ctrl", roomHistory, changes);

        Assert.NotNull(roomHistory.Structures.Controller);
        Assert.Equal("owner", roomHistory.Structures.Controller.User);
        Assert.Equal(5, roomHistory.Structures.Controller.Level);
    }

    [Fact]
    public void UpdateRoomHistory_AddsWallEntry()
    {
        var roomHistory = new ScreepsRoomHistory();
        roomHistory.TypeMap["wall"] = "constructedWall";

        var changes = new Dictionary<string, object?>
        {
            ["hits"] = 150,
            ["user"] = "builder"
        };

        ScreepsRoomHistoryHelper.UpdateRoomHistory("wall", roomHistory, changes);

        Assert.True(roomHistory.Structures.Walls.ContainsKey("wall"));
        Assert.Equal(150, roomHistory.Structures.Walls["wall"].Hits);
    }

    [Fact]
    public void UpdateRoomHistory_ClassifiesOwnedAndEnemyCreep()
    {
        var roomHistory = new ScreepsRoomHistory();
        roomHistory.TypeMap["creepoon"] = "creep";
        roomHistory.TypeMap["creepenemy"] = "creep";
        roomHistory.Structures.Controller = new StructureController { User = "owner" };

        var owned = new Dictionary<string, object?>
        {
            ["hits"] = 99,
            ["user"] = "owner"
        };
        var enemy = new Dictionary<string, object?>
        {
            ["hits"] = 25,
            ["user"] = "other"
        };

        ScreepsRoomHistoryHelper.UpdateRoomHistory("creepoon", roomHistory, owned);
        ScreepsRoomHistoryHelper.UpdateRoomHistory("creepenemy", roomHistory, enemy);

        Assert.True(roomHistory.Creeps.OwnedCreeps.ContainsKey("creepoon"));
        Assert.True(roomHistory.Creeps.EnemyCreeps.ContainsKey("creepenemy"));
        Assert.Equal(99, roomHistory.Creeps.OwnedCreeps["creepoon"].Hits);
        Assert.Equal(25, roomHistory.Creeps.EnemyCreeps["creepenemy"].Hits);
    }

    [Fact]
    public void RemoveFromRoomHistory_RemovesWallAndControllerReferences()
    {
        var roomHistory = new ScreepsRoomHistory();
        roomHistory.TypeMap["wall"] = "constructedWall";
        roomHistory.TypeMap["ctrl"] = "controller";
        roomHistory.Structures.Walls["wall"] = new StructureWall();
        roomHistory.Structures.Controller = new StructureController { Id = "ctrl" };

        ScreepsRoomHistoryHelper.RemoveFromRoomHistory("wall", roomHistory);
        ScreepsRoomHistoryHelper.RemoveFromRoomHistory("ctrl", roomHistory);

        Assert.False(roomHistory.Structures.Walls.ContainsKey("wall"));
        Assert.Null(roomHistory.Structures.Controller);
        Assert.False(roomHistory.TypeMap.ContainsKey("wall"));
        Assert.False(roomHistory.TypeMap.ContainsKey("ctrl"));
    }

    [Fact]
    public void GetLastPathSegment_ReturnsSuffix()
    {
        var path = "room.tick.object";
        var result = ScreepsRoomHistoryHelper.GetLastPathSegment(path);
        Assert.Equal("object", result.ToString());
    }

    [Fact]
    public void ComputeTick_PopulatesControllerAndWall()
    {
        var roomHistory = new ScreepsRoomHistory();
        var tickObject = JObject.Parse("""
        {
            "controllerId": {
                "_id": "controllerId",
                "type": "controller",
                "user": "owner",
                "level": 3
            },
            "wallAlpha": {
                "_id": "wallAlpha",
                "type": "constructedWall",
                "hits": 200,
                "user": "builder"
            }
        }
        """);

        var result = ScreepsRoomHistoryHelper.ComputeTick(tickObject, roomHistory);

        Assert.NotNull(result.Structures.Controller);
        Assert.Equal(3, result.Structures.Controller!.Level);
        Assert.Equal("owner", result.Structures.Controller.User);
        Assert.True(result.Structures.Walls.ContainsKey("wallAlpha"));
        Assert.Equal(200, result.Structures.Walls["wallAlpha"].Hits);
        Assert.Equal("constructedWall", result.TypeMap["wallAlpha"]);
        Assert.Equal("controller", result.TypeMap["controllerId"]);
    }
}

public class ScreepsRoomHistoryHelperAdditionalTypesTests
{
    [Fact]
    public void UpdateRoomHistory_AddsControllerAndSpawn()
    {
        var history = new ScreepsRoomHistory();
        history.TypeMap["controller1"] = "controller";
        history.TypeMap["spawn1"] = "spawn";

        ScreepsRoomHistoryHelper.UpdateRoomHistory("controller1", history, new Dictionary<string, object?>
        {
            ["level"] = 4,
            ["user"] = "owner"
        });
        ScreepsRoomHistoryHelper.UpdateRoomHistory("spawn1", history, new Dictionary<string, object?>
        {
            ["store.energy"] = 300
        });

        Assert.NotNull(history.Structures.Controller);
        Assert.Equal(4m, history.Structures.Controller!.Level);
        Assert.Equal("owner", history.Structures.Controller.User);
        Assert.True(history.Structures.Spawns.ContainsKey("spawn1"));
        Assert.Equal(300m, history.Structures.Spawns["spawn1"].Store.energy);
    }

    [Fact]
    public void UpdateRoomHistory_AddsLabFactoryObserverPortal()
    {
        var history = new ScreepsRoomHistory();
        history.TypeMap["lab1"] = "lab";
        history.TypeMap["factory1"] = "factory";
        history.TypeMap["observer1"] = "observer";
        history.TypeMap["portal1"] = "portal";

        ScreepsRoomHistoryHelper.UpdateRoomHistory("lab1", history, new Dictionary<string, object?>
        {
            ["store.energy"] = 50
        });
        ScreepsRoomHistoryHelper.UpdateRoomHistory("factory1", history, new Dictionary<string, object?>
        {
            ["store.energy"] = 60
        });
        ScreepsRoomHistoryHelper.UpdateRoomHistory("observer1", history, new Dictionary<string, object?>
        {
            ["hits"] = 100
        });
        ScreepsRoomHistoryHelper.UpdateRoomHistory("portal1", history, new Dictionary<string, object?>
        {
            ["tick"] = 123
        });

        Assert.True(history.Structures.Labs.ContainsKey("lab1"));
        Assert.Equal(50m, history.Structures.Labs["lab1"].Store.energy);
        Assert.True(history.Structures.Factories.ContainsKey("factory1"));
        Assert.Equal(60m, history.Structures.Factories["factory1"].Store.energy);
        Assert.True(history.Structures.Observers.ContainsKey("observer1"));
        Assert.Equal(100m, history.Structures.Observers["observer1"].Hits);
        Assert.True(history.Structures.Portals.ContainsKey("portal1"));
        Assert.Equal(123m, history.Structures.Portals["portal1"].Tick);
    }

    [Fact]
    public void UpdateRoomHistory_AddsNukerAndNuke()
    {
        var history = new ScreepsRoomHistory();
        history.TypeMap["nuker1"] = "nuker";
        history.TypeMap["nuke1"] = "nuke";

        ScreepsRoomHistoryHelper.UpdateRoomHistory("nuker1", history, new Dictionary<string, object?>
        {
            ["store.energy"] = 1000
        });
        ScreepsRoomHistoryHelper.UpdateRoomHistory("nuke1", history, new Dictionary<string, object?>
        {
            ["landTime"] = 999
        });

        Assert.True(history.Structures.Nukers.ContainsKey("nuker1"));
        Assert.Equal(1000m, history.Structures.Nukers["nuker1"].Store.energy);
        Assert.True(history.Structures.Nukes.ContainsKey("nuke1"));
        Assert.Equal(999m, history.Structures.Nukes["nuke1"].LandTime);
    }

    [Fact]
    public void RemoveFromRoomHistory_RemovesControllerAndNuke()
    {
        var history = new ScreepsRoomHistory();
        history.TypeMap["controller1"] = "controller";
        history.TypeMap["nuke1"] = "nuke";
        history.Structures.Controller = new StructureController { Id = "controller1" };
        history.Structures.Nukes["nuke1"] = new StructureNuke();

        ScreepsRoomHistoryHelper.RemoveFromRoomHistory("controller1", history);
        ScreepsRoomHistoryHelper.RemoveFromRoomHistory("nuke1", history);

        Assert.Null(history.Structures.Controller);
        Assert.False(history.Structures.Nukes.ContainsKey("nuke1"));
        Assert.False(history.TypeMap.ContainsKey("controller1"));
        Assert.False(history.TypeMap.ContainsKey("nuke1"));
    }
}

public class ScreepsRoomHistoryHelperComputeTickTests
{
    [Fact]
    public void ComputeTick_PopulatesHistoryChangesDictionary()
    {
        var history = new ScreepsRoomHistory
        {
            HistoryChangesDictionary = new Dictionary<string, Dictionary<string, object?>>()
        };
        var tickObject = JObject.Parse("""
        {
            "wall1": {
                "_id": "wall1",
                "type": "constructedWall",
                "hits": 10
            }
        }
        """);

        var result = ScreepsRoomHistoryHelper.ComputeTick(tickObject, history);

        Assert.True(result.TypeMap.ContainsKey("wall1"));
        Assert.Equal("constructedWall", result.TypeMap["wall1"]);
        Assert.True(result.HistoryChangesDictionary!.ContainsKey("wall1"));
        Assert.True(result.Structures.Walls.ContainsKey("wall1"));
        Assert.Equal(10m, result.Structures.Walls["wall1"].Hits);
    }

    [Fact]
    public void ComputeTick_RemovesObjectsWhenTokenMissing()
    {
        var history = new ScreepsRoomHistory();
        history.TypeMap["wall1"] = "constructedWall";
        history.ObjectUserMap["wall1"] = "owner";
        history.Structures.Walls["wall1"] = new StructureWall();

        var tickObject = JObject.Parse("""
        {
            "wall1": null
        }
        """);

        var result = ScreepsRoomHistoryHelper.ComputeTick(tickObject, history);

        Assert.False(result.Structures.Walls.ContainsKey("wall1"));
        Assert.False(result.TypeMap.ContainsKey("wall1"));
        Assert.False(result.ObjectUserMap.ContainsKey("wall1"));
    }

    [Fact]
    public void ComputeTick_ProcessesControllerBeforeCreeps()
    {
        var history = new ScreepsRoomHistory();
        var tickObject = JObject.Parse("""
        {
            "controller1": {
                "_id": "controller1",
                "type": "controller",
                "user": "owner",
                "level": 2
            },
            "creep1": {
                "_id": "creep1",
                "type": "creep",
                "user": "owner",
                "hits": 50
            }
        }
        """);

        var result = ScreepsRoomHistoryHelper.ComputeTick(tickObject, history);

        Assert.NotNull(result.Structures.Controller);
        Assert.True(result.Creeps.OwnedCreeps.ContainsKey("creep1"));
        Assert.Equal(50m, result.Creeps.OwnedCreeps["creep1"].Hits);
    }

    [Fact]
    public void ComputeTick_SkipsUndefinedKeyAndTracksUsers()
    {
        var history = new ScreepsRoomHistory
        {
            HistoryChangesDictionary = new Dictionary<string, Dictionary<string, object?>>()
        };
        var tickObject = JObject.Parse("""
        {
            "undefined": {
                "type": "constructionSite"
            },
            "wall2": {
                "_id": "wall2",
                "type": "constructedWall",
                "hits": 15,
                "user": "builder"
            }
        }
        """);

        var result = ScreepsRoomHistoryHelper.ComputeTick(tickObject, history);

        Assert.False(result.TypeMap.ContainsKey("undefined"));
        Assert.True(result.TypeMap.ContainsKey("wall2"));
        Assert.True(result.ObjectUserMap.ContainsKey("wall2"));
        Assert.Equal("builder", result.ObjectUserMap["wall2"]);
        Assert.True(result.HistoryChangesDictionary!.ContainsKey("wall2"));
        Assert.False(result.HistoryChangesDictionary.ContainsKey("undefined"));
    }

    [Fact]
    public void ComputeTick_ProcessesMultipleTypesAndRemovals()
    {
        var history = new ScreepsRoomHistory();
        history.TypeMap["oldRoad"] = "road";
        history.Structures.Roads["oldRoad"] = new StructureRoad { Hits = 5 };
        history.ObjectUserMap["oldRoad"] = "builder";

        var tickObject = JObject.Parse("""
        {
            "oldRoad": null,
            "container1": {
                "_id": "container1",
                "type": "container",
                "store": { "energy": 25 }
            },
            "storage1": {
                "_id": "storage1",
                "type": "storage",
                "store": { "energy": 75 }
            }
        }
        """);

        var result = ScreepsRoomHistoryHelper.ComputeTick(tickObject, history);

        Assert.False(result.Structures.Roads.ContainsKey("oldRoad"));
        Assert.True(result.Structures.Containers.ContainsKey("container1"));
        Assert.Equal(25m, result.Structures.Containers["container1"].Store.energy);
        Assert.True(result.Structures.Storages.ContainsKey("storage1"));
        Assert.Equal(75m, result.Structures.Storages["storage1"].Store.energy);
    }
}

public class ScreepsRoomHistoryHelperComputeTickMultiTests
{
    [Fact]
    public void ComputeTick_TracksMultipleObjectsInHistoryChanges()
    {
        var history = new ScreepsRoomHistory
        {
            HistoryChangesDictionary = new Dictionary<string, Dictionary<string, object?>>()
        };

        var tickObject = JObject.Parse("""
        {
            "wall1": {
                "_id": "wall1",
                "type": "constructedWall",
                "hits": 5
            },
            "ramp1": {
                "_id": "ramp1",
                "type": "rampart",
                "hits": 9
            },
            "terminal1": {
                "_id": "terminal1",
                "type": "terminal",
                "store": { "energy": 77 }
            }
        }
        """);

        var result = ScreepsRoomHistoryHelper.ComputeTick(tickObject, history);

        Assert.Equal("constructedWall", result.TypeMap["wall1"]);
        Assert.Equal("rampart", result.TypeMap["ramp1"]);
        Assert.Equal("terminal", result.TypeMap["terminal1"]);
        Assert.True(result.Structures.Walls.ContainsKey("wall1"));
        Assert.True(result.Structures.Ramparts.ContainsKey("ramp1"));
        Assert.True(result.Structures.Terminals.ContainsKey("terminal1"));
        Assert.True(result.HistoryChangesDictionary!.ContainsKey("wall1"));
        Assert.True(result.HistoryChangesDictionary.ContainsKey("ramp1"));
        Assert.True(result.HistoryChangesDictionary.ContainsKey("terminal1"));
    }

    [Fact]
    public void ComputeTick_PreservesExistingTypeMap()
    {
        var history = new ScreepsRoomHistory();
        history.TypeMap["wall1"] = "constructedWall";

        var tickObject = JObject.Parse("""
        {
            "wall1": {
                "_id": "wall1",
                "hits": 12
            }
        }
        """);

        var result = ScreepsRoomHistoryHelper.ComputeTick(tickObject, history);

        Assert.Equal("constructedWall", result.TypeMap["wall1"]);
        Assert.True(result.Structures.Walls.ContainsKey("wall1"));
        Assert.Equal(12m, result.Structures.Walls["wall1"].Hits);
    }
}

public class ScreepsRoomHistoryHelperMiscTypesTests
{
    [Fact]
    public void UpdateRoomHistory_AddsMineralDepositAndContainer()
    {
        var history = new ScreepsRoomHistory();
        history.TypeMap["mineral1"] = "mineral";
        history.TypeMap["deposit1"] = "deposit";
        history.TypeMap["container1"] = "container";

        ScreepsRoomHistoryHelper.UpdateRoomHistory("mineral1", history, new Dictionary<string, object?>
        {
            ["mineralAmount"] = 123
        });
        ScreepsRoomHistoryHelper.UpdateRoomHistory("deposit1", history, new Dictionary<string, object?>
        {
            ["harvested"] = 7
        });
        ScreepsRoomHistoryHelper.UpdateRoomHistory("container1", history, new Dictionary<string, object?>
        {
            ["store.energy"] = 80
        });

        Assert.NotNull(history.Structures.Mineral);
        Assert.Equal(123m, history.Structures.Mineral!.MineralAmount);
        Assert.True(history.Structures.Deposits.ContainsKey("deposit1"));
        Assert.Equal(7m, history.Structures.Deposits["deposit1"].Harvested);
        Assert.True(history.Structures.Containers.ContainsKey("container1"));
        Assert.Equal(80m, history.Structures.Containers["container1"].Store.energy);
    }

    [Fact]
    public void UpdateRoomHistory_AddsConstructionSiteRuinTombstoneAndPowerBank()
    {
        var history = new ScreepsRoomHistory();
        history.TypeMap["site1"] = "constructionSite";
        history.TypeMap["ruin1"] = "ruin";
        history.TypeMap["tomb1"] = "tombstone";
        history.TypeMap["bank1"] = "powerBank";

        ScreepsRoomHistoryHelper.UpdateRoomHistory("site1", history, new Dictionary<string, object?>
        {
            ["structureType"] = "extension",
            ["progress"] = 12
        });
        ScreepsRoomHistoryHelper.UpdateRoomHistory("ruin1", history, new Dictionary<string, object?>
        {
            ["store.energy"] = 15
        });
        ScreepsRoomHistoryHelper.UpdateRoomHistory("tomb1", history, new Dictionary<string, object?>
        {
            ["store.energy"] = 25
        });
        ScreepsRoomHistoryHelper.UpdateRoomHistory("bank1", history, new Dictionary<string, object?>
        {
            ["hits"] = 1000
        });

        Assert.True(history.Structures.ConstructionSites.ContainsKey("site1"));
        Assert.Equal(12m, history.Structures.ConstructionSites["site1"].Progress);
        Assert.True(history.Structures.Ruins.ContainsKey("ruin1"));
        Assert.NotNull(history.Structures.Ruins["ruin1"].Store);
        Assert.Equal(15m, history.Structures.Ruins["ruin1"].Store!.energy);
        Assert.True(history.Structures.Tombstones.ContainsKey("tomb1"));
        Assert.NotNull(history.Structures.Tombstones["tomb1"].Store);
        Assert.Equal(25m, history.Structures.Tombstones["tomb1"].Store!.energy);
        Assert.True(history.Structures.PowerBanks.ContainsKey("bank1"));
        Assert.Equal(1000m, history.Structures.PowerBanks["bank1"].Hits);
    }
}

public class ScreepsRoomHistoryHelperMoreTests
{
    [Fact]
    public void UpdateRoomHistory_AddsEnergyAndStorage()
    {
        var history = new ScreepsRoomHistory();
        history.TypeMap["energy1"] = "energy";
        history.TypeMap["storage1"] = "storage";

        ScreepsRoomHistoryHelper.UpdateRoomHistory("energy1", history, new Dictionary<string, object?>
        {
            ["resourceType"] = "energy",
            ["energy"] = 150
        });
        ScreepsRoomHistoryHelper.UpdateRoomHistory("storage1", history, new Dictionary<string, object?>
        {
            ["store.energy"] = 500
        });

        Assert.True(history.GroundResources.ContainsKey("energy1"));
        Assert.Equal(150m, history.GroundResources["energy1"].energy);
        Assert.True(history.Structures.Storages.ContainsKey("storage1"));
        Assert.Equal(500m, history.Structures.Storages["storage1"].Store.energy);
    }

    [Fact]
    public void UpdateRoomHistory_AddsTerminalAndLink()
    {
        var history = new ScreepsRoomHistory();
        history.TypeMap["terminal1"] = "terminal";
        history.TypeMap["link1"] = "link";

        ScreepsRoomHistoryHelper.UpdateRoomHistory("terminal1", history, new Dictionary<string, object?>
        {
            ["store.energy"] = 200
        });
        ScreepsRoomHistoryHelper.UpdateRoomHistory("link1", history, new Dictionary<string, object?>
        {
            ["store.energy"] = 40
        });

        Assert.True(history.Structures.Terminals.ContainsKey("terminal1"));
        Assert.Equal(200m, history.Structures.Terminals["terminal1"].Store.energy);
        Assert.True(history.Structures.Links.ContainsKey("link1"));
        Assert.Equal(40m, history.Structures.Links["link1"].Store.energy);
    }

    [Fact]
    public void UpdateRoomHistory_AddsRampartRoadAndPowerCreep()
    {
        var history = new ScreepsRoomHistory();
        history.TypeMap["ramp1"] = "rampart";
        history.TypeMap["road1"] = "road";
        history.TypeMap["pc1"] = "powerCreep";

        ScreepsRoomHistoryHelper.UpdateRoomHistory("ramp1", history, new Dictionary<string, object?>
        {
            ["hits"] = 1000
        });
        ScreepsRoomHistoryHelper.UpdateRoomHistory("road1", history, new Dictionary<string, object?>
        {
            ["hits"] = 500
        });
        ScreepsRoomHistoryHelper.UpdateRoomHistory("pc1", history, new Dictionary<string, object?>
        {
            ["hits"] = 250
        });

        Assert.True(history.Structures.Ramparts.ContainsKey("ramp1"));
        Assert.Equal(1000m, history.Structures.Ramparts["ramp1"].Hits);
        Assert.True(history.Structures.Roads.ContainsKey("road1"));
        Assert.Equal(500m, history.Structures.Roads["road1"].Hits);
        Assert.True(history.Creeps.PowerCreeps.ContainsKey("pc1"));
        Assert.Equal(250m, history.Creeps.PowerCreeps["pc1"].Hits);
    }

    [Fact]
    public void RemoveFromRoomHistory_RemovesMultipleTypes()
    {
        var history = new ScreepsRoomHistory();
        history.TypeMap["term1"] = "terminal";
        history.TypeMap["storage1"] = "storage";
        history.TypeMap["power1"] = "powerCreep";
        history.Structures.Terminals["term1"] = new StructureTerminal();
        history.Structures.Storages["storage1"] = new StructureStorage();
        history.Creeps.PowerCreeps["power1"] = new PowerCreep();

        ScreepsRoomHistoryHelper.RemoveFromRoomHistory("term1", history);
        ScreepsRoomHistoryHelper.RemoveFromRoomHistory("storage1", history);
        ScreepsRoomHistoryHelper.RemoveFromRoomHistory("power1", history);

        Assert.False(history.Structures.Terminals.ContainsKey("term1"));
        Assert.False(history.Structures.Storages.ContainsKey("storage1"));
        Assert.False(history.Creeps.PowerCreeps.ContainsKey("power1"));
        Assert.False(history.TypeMap.ContainsKey("term1"));
        Assert.False(history.TypeMap.ContainsKey("storage1"));
        Assert.False(history.TypeMap.ContainsKey("power1"));
    }
}

public class ScreepsRoomHistoryHelperRemainingTypesTests
{
    [Fact]
    public void UpdateRoomHistory_AddsExtensionExtractorInvaderAndKeeper()
    {
        var history = new ScreepsRoomHistory();
        history.TypeMap["ext1"] = "extension";
        history.TypeMap["extract1"] = "extractor";
        history.TypeMap["inv1"] = "invaderCore";
        history.TypeMap["keeper1"] = "keeperLair";

        ScreepsRoomHistoryHelper.UpdateRoomHistory("ext1", history, new Dictionary<string, object?>
        {
            ["store.energy"] = 100
        });
        ScreepsRoomHistoryHelper.UpdateRoomHistory("extract1", history, new Dictionary<string, object?>
        {
            ["cooldown"] = 3
        });
        ScreepsRoomHistoryHelper.UpdateRoomHistory("inv1", history, new Dictionary<string, object?>
        {
            ["hits"] = 200
        });
        ScreepsRoomHistoryHelper.UpdateRoomHistory("keeper1", history, new Dictionary<string, object?>
        {
            ["nextSpawnTime"] = 77
        });

        Assert.True(history.Structures.Extensions.ContainsKey("ext1"));
        Assert.Equal(100m, history.Structures.Extensions["ext1"].Store.energy);
        Assert.True(history.Structures.Extractors.ContainsKey("extract1"));
        Assert.Equal(3m, history.Structures.Extractors["extract1"].Cooldown);
        Assert.True(history.Structures.InvaderCores.ContainsKey("inv1"));
        Assert.Equal(200m, history.Structures.InvaderCores["inv1"].Hits);
        Assert.True(history.Structures.KeeperLairs.ContainsKey("keeper1"));
        Assert.Equal(77m, history.Structures.KeeperLairs["keeper1"].NextSpawnTime);
    }

    [Fact]
    public void UpdateRoomHistory_AddsSourcePowerSpawnFactoryAndObserver()
    {
        var history = new ScreepsRoomHistory();
        history.TypeMap["source1"] = "source";
        history.TypeMap["powerSpawn1"] = "powerSpawn";
        history.TypeMap["factory1"] = "factory";
        history.TypeMap["observer1"] = "observer";

        ScreepsRoomHistoryHelper.UpdateRoomHistory("source1", history, new Dictionary<string, object?>
        {
            ["energy"] = 300
        });
        ScreepsRoomHistoryHelper.UpdateRoomHistory("powerSpawn1", history, new Dictionary<string, object?>
        {
            ["store.energy"] = 40
        });
        ScreepsRoomHistoryHelper.UpdateRoomHistory("factory1", history, new Dictionary<string, object?>
        {
            ["level"] = 2
        });
        ScreepsRoomHistoryHelper.UpdateRoomHistory("observer1", history, new Dictionary<string, object?>
        {
            ["hits"] = 500
        });

        Assert.True(history.Structures.Sources.ContainsKey("source1"));
        Assert.Equal(300m, history.Structures.Sources["source1"].Energy);
        Assert.True(history.Structures.PowerSpawns.ContainsKey("powerSpawn1"));
        Assert.Equal(40m, history.Structures.PowerSpawns["powerSpawn1"].Store.energy);
        Assert.True(history.Structures.Factories.ContainsKey("factory1"));
        Assert.Equal(2m, history.Structures.Factories["factory1"].Level);
        Assert.True(history.Structures.Observers.ContainsKey("observer1"));
        Assert.Equal(500m, history.Structures.Observers["observer1"].Hits);
    }

    [Fact]
    public void RemoveFromRoomHistory_RemovesExtensionAndSource()
    {
        var history = new ScreepsRoomHistory();
        history.TypeMap["ext1"] = "extension";
        history.TypeMap["source1"] = "source";
        history.Structures.Extensions["ext1"] = new StructureExtension();
        history.Structures.Sources["source1"] = new StructureSource();

        ScreepsRoomHistoryHelper.RemoveFromRoomHistory("ext1", history);
        ScreepsRoomHistoryHelper.RemoveFromRoomHistory("source1", history);

        Assert.False(history.Structures.Extensions.ContainsKey("ext1"));
        Assert.False(history.Structures.Sources.ContainsKey("source1"));
        Assert.False(history.TypeMap.ContainsKey("ext1"));
        Assert.False(history.TypeMap.ContainsKey("source1"));
    }
}
