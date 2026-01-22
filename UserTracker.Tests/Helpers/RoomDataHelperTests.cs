using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerShared.States;
using Xunit;

namespace UserTracker.Tests.Helpers;

public class RoomDataHelperTests : IDisposable
{
    public RoomDataHelperTests()
    {
        ConfigSettingsState.TicksInFile = 2;
        ConfigSettingsState.TicksInObject = 2;
        ConfigSettingsState.WriteHistoryProperties = false;
    }

    public void Dispose()
    {
        RoomDataHelper.ResetHistoryFetcher();
    }

    [Fact]
    public async Task GetAndHandleRoomData_Returns200AndUpdatesDto()
    {
        var roomData = JObject.Parse("""
        {
            "timestamp": 123,
            "base": 0,
            "ticks": {
                "0": {
                    "controller": {
                        "_id": "controller",
                        "type": "controller",
                        "user": "owner",
                        "level": 4
                    }
                },
                "1": {
                    "wall": {
                        "_id": "wall",
                        "type": "constructedWall",
                        "hits": 200
                    }
                }
            }
        }
        """);

        RoomDataHelper.SetHistoryFetcher((shard, room, tick) => Task.FromResult<(JObject?, HttpStatusCode)>((roomData, HttpStatusCode.OK)));

        var dataByRoom = new ConcurrentDictionary<string, ScreepsRoomHistoryDto>();
        var userLocks = new ConcurrentDictionary<string, object>();

        var status = await RoomDataHelper.GetAndHandleRoomData("shard0", "roomA", 1, dataByRoom, userLocks);

        Assert.Equal(200, status);
        Assert.Contains("roomA", dataByRoom.Keys);
        var dto = dataByRoom["roomA"];
        Assert.Equal(1, dto.Tick);
        Assert.Equal(4m, dto.Structures.Controller.Level);
        Assert.Equal(1m / ConfigSettingsState.TicksInObject, dto.Structures.Wall.Count);
    }

    [Fact]
    public async Task GetAndHandleRoomData_ReturnsFetcherStatusWhenNoData()
    {
        RoomDataHelper.SetHistoryFetcher((_, _, _) => Task.FromResult<(JObject?, HttpStatusCode)>((null, HttpStatusCode.NotFound)));

        var dataByRoom = new ConcurrentDictionary<string, ScreepsRoomHistoryDto>();
        var userLocks = new ConcurrentDictionary<string, object>();

        var status = await RoomDataHelper.GetAndHandleRoomData("shard0", "roomB", 1, dataByRoom, userLocks);

        Assert.Equal((int)HttpStatusCode.NotFound, status);
    }
}
