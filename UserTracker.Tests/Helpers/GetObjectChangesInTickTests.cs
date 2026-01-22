using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UserTrackerShared.Helpers.Tests;
using Xunit;

namespace UserTracker.Tests.Helpers;

public class GetObjectChangesInTickTests
{
    [Fact]
    public void GetById_FlattensObjectFields()
    {
        var tick = JObject.Parse("""
        {
            "creep1": {
                "_id": "creep1",
                "stats": {
                    "hits": 50
                },
                "arr": [1, 2]
            }
        }
        """);

        var changes = GetObjectChangesInTick.GetById(tick, "creep1");

        Assert.Equal("creep1", changes["_id"]);
        Assert.Equal(50L, changes["stats.hits"]);
        Assert.Equal(1L, changes["arr.0"]);
        Assert.Equal(2L, changes["arr.1"]);
    }

    [Fact]
    public void GetById_ReturnsEmptyDictionaryWhenMissing()
    {
        var tick = JObject.Parse("""{ "other": { "value": 1 } }""");
        var changes = GetObjectChangesInTick.GetById(tick, "missing");
        Assert.Empty(changes);
    }
}
