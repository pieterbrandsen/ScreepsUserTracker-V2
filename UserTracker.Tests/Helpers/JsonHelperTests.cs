using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using UserTrackerShared.Helpers;
using Xunit;

namespace UserTracker.Tests.Helpers;

public class JsonHelperTests
{
    [Fact]
    public void FlattenJson_HandlesNestedObjectsAndArrays()
    {
        var json = JObject.Parse("""
        {
            "team": {
                "score": 7,
                "player": {
                    "name": "alpha",
                    "stats": [1, 2, {"health": 100}]
                }
            },
            "list": ["x", "y"]
        }
        """);

        var buffer = new StringBuilder();
        var result = new Dictionary<string, object?>();

        JsonHelper.FlattenJson(json, buffer, result);

        Assert.Equal(7, result.Count);
        Assert.Equal(7L, result["team.score"]);
        Assert.Equal("alpha", result["team.player.name"]);
        Assert.Equal(1L, result["team.player.stats[0]"]);
        Assert.Equal(2L, result["team.player.stats[1]"]);
        Assert.Equal(100L, result["team.player.stats[2].health"]);
        Assert.Equal("y", result["list[1]"]);
        Assert.Equal("x", result["list[0]"]);
    }
}
