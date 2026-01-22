using System;
using UserTrackerShared.Helpers.Tests;
using UserTrackerShared.Models;
using Xunit;

namespace UserTracker.Tests.Helpers;

public class GetObjectFromHistoryTests
{
    [Fact]
    public void GetById_ReturnsStructureWhenPresent()
    {
        var history = new ScreepsRoomHistory();
        history.TypeMap["wall1"] = "constructedWall";
        history.Structures.Walls["wall1"] = new StructureWall { Id = "wall1", Hits = 10 };

        var result = GetObjectFromHistory.GetById(history, "wall1");

        var wall = Assert.IsType<StructureWall>(result);
        Assert.Equal(10, wall.Hits);
    }

    [Fact]
    public void GetById_ReturnsCreepFromOwnedOrEnemy()
    {
        var history = new ScreepsRoomHistory();
        history.TypeMap["creepA"] = "creep";
        history.Creeps.OwnedCreeps["creepA"] = new Creep { Id = "creepA", Hits = 1 };

        var result = GetObjectFromHistory.GetById(history, "creepA");

        var creep = Assert.IsType<Creep>(result);
        Assert.Equal(1, creep.Hits);
    }

    [Fact]
    public void GetById_ReturnsControllerWhenIdMatches()
    {
        var history = new ScreepsRoomHistory();
        history.TypeMap["ctrl"] = "controller";
        history.Structures.Controller = new StructureController { Id = "ctrl", Level = 2 };

        var result = GetObjectFromHistory.GetById(history, "ctrl");

        var controller = Assert.IsType<StructureController>(result);
        Assert.Equal(2, controller.Level);
    }

    [Fact]
    public void GetById_ThrowsForUnsupportedType()
    {
        var history = new ScreepsRoomHistory();
        history.TypeMap["unknown1"] = "mystery";

        var ex = Assert.Throws<ArgumentException>(() => GetObjectFromHistory.GetById(history, "unknown1"));
        Assert.Contains("Unsupported type", ex.Message);
    }
}
