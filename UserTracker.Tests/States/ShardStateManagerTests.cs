using System.Collections.Concurrent;
using System.Net;
using Newtonsoft.Json.Linq;
using UserTrackerShared.Helpers;
using UserTrackerShared.Managers;
using UserTrackerShared.Models;
using UserTrackerShared.States;

namespace UserTracker.Tests.States;

public class ShardStateManagerTests : IDisposable
{
    private readonly int previousFile = ConfigSettingsState.TicksInFile;
    private readonly int previousWindow = ConfigSettingsState.TicksInObject;
    private readonly int previousPull = ConfigSettingsState.PullBackwardsTickAmount;
    private readonly bool previousWriteProperties = ConfigSettingsState.WriteHistoryProperties;
    private readonly string userId = Guid.NewGuid().ToString();
    private readonly ConcurrentBag<(string Room, long Tick)> requests = new();
    private readonly List<(string Room, long Tick, ScreepsRoomHistoryDto Data)> roomRows = [];
    private readonly List<(long Tick, ScreepsRoomHistoryDto Data)> userRows = [];
    private readonly List<(long Tick, ScreepsRoomHistoryDto Data)> globalRows = [];

    public ShardStateManagerTests()
    {
        ConfigSettingsState.WriteHistoryProperties = false;
        GameState.Users[userId] = new ScreepsUser { Id = userId, Username = "window-test" };
        RoomDataHelper.SetHistoryFetcher((_, room, tick) =>
        {
            requests.Add((room, tick));
            return Task.FromResult<(JObject?, HttpStatusCode)>((History(tick), HttpStatusCode.OK));
        });
    }

    public void Dispose()
    {
        RoomDataHelper.ResetHistoryFetcher();
        GameState.Users.TryRemove(userId, out _);
        ConfigSettingsState.TicksInFile = previousFile;
        ConfigSettingsState.TicksInObject = previousWindow;
        ConfigSettingsState.PullBackwardsTickAmount = previousPull;
        ConfigSettingsState.WriteHistoryProperties = previousWriteProperties;
    }

    private ShardStateManager CreateShard(int file, int window)
    {
        ConfigSettingsState.TicksInFile = file;
        ConfigSettingsState.TicksInObject = window;
        ConfigSettingsState.PullBackwardsTickAmount = file;
        return new ShardStateManager("window-test")
        {
            Rooms = ["W0N0", "W1N0"],
            RoomHistoryWriter = (_, room, tick, timestamp, data) =>
            {
                Assert.Equal(data.TimeStamp, timestamp);
                roomRows.Add((room, tick, data));
                return Task.CompletedTask;
            },
            UserHistoryWriter = (_, username, tick, timestamp, data) =>
            {
                Assert.Equal("window-test", username);
                Assert.Equal(data.TimeStamp, timestamp);
                userRows.Add((tick, data));
            },
            GlobalHistoryWriter = (_, tick, timestamp, data) => globalRows.Add((tick, data))
        };
    }

    [Theory]
    [InlineData(100, 1)]
    [InlineData(100, 10)]
    [InlineData(100, 100)]
    [InlineData(100, 1000)]
    [InlineData(20, 5)]
    [InlineData(20, 20)]
    [InlineData(20, 100)]
    public async Task Sync_EmitsCompleteWindowsAndPreservesRoomUserAndGlobalAverages(int file, int window)
    {
        var shard = CreateShard(file, window);
        var totalTicks = 2 * Math.Max(file, window);
        for (var end = file; end <= totalTicks; end += file)
        {
            shard.Time = end + 500;
            await shard.StartSync();
            Assert.Equal(end / window * 2, roomRows.Count);
            Assert.Equal(end / window, userRows.Count);
            Assert.Equal(end / window, globalRows.Count);
        }

        Assert.Equal(totalTicks / file * 2, requests.Count);
        Assert.Equal(requests.Count, requests.Distinct().Count());
        Assert.Equal(Enumerable.Range(0, totalTicks / file).Select(i => (long)i * file),
            requests.Where(r => r.Room == "W0N0").Select(r => r.Tick).Order());
        Assert.Equal(Enumerable.Range(0, totalTicks / window).Select(i => (long)i * window),
            userRows.Select(r => r.Tick));
        Assert.All(roomRows, row =>
        {
            Assert.Equal(row.Tick + window - 1, row.Data.Tick);
            AssertAverages(row.Data, 1);
        });
        Assert.All(userRows, row => AssertAverages(row.Data, 2));
        Assert.All(globalRows, row => AssertAverages(row.Data, 2));

        await shard.StartSync();
        Assert.Equal(totalTicks / file * 2, requests.Count);
        Assert.Equal(totalTicks / window * 2, roomRows.Count);
    }

    [Fact]
    public async Task Sync_RoomsWaitingForRetriesDoNotBlockOtherRoomsFromStarting()
    {
        var shard = CreateShard(100, 100);
        shard.Rooms = Enumerable.Range(0, 150).Select(i => $"room{i}").ToList();
        var allStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = 0;
        RoomDataHelper.SetHistoryFetcher(async (_, _, _) =>
        {
            if (Interlocked.Increment(ref started) == shard.Rooms.Count) allStarted.SetResult();
            await release.Task;
            return (null, HttpStatusCode.InternalServerError);
        });
        shard.Time = 600;
        var sync = shard.StartSync();
        try { await allStarted.Task.WaitAsync(TimeSpan.FromSeconds(5)); }
        finally
        {
            release.SetResult();
            await sync;
        }
        Assert.Equal(150, started);
        Assert.Empty(roomRows);
    }

    [Fact]
    public async Task Sync_SubFileWindowsKeepHistoryStateAcrossWindowBoundaries()
    {
        var shard = CreateShard(100, 10);
        RoomDataHelper.SetHistoryFetcher((_, _, tick) =>
        {
            var data = History(tick);
            // The first ten ticks have one creep; the rest retain its deletion.
            data["ticks"]!["10"] = new JObject { ["creep"] = JValue.CreateNull() };
            return Task.FromResult<(JObject?, HttpStatusCode)>((data, HttpStatusCode.OK));
        });
        shard.Time = 600;
        await shard.StartSync();

        Assert.Equal(20, roomRows.Count);
        Assert.All(roomRows.Where(r => r.Tick == 0), row => Assert.Equal(1m, row.Data.Creeps.OwnedCreeps.Count));
        Assert.All(roomRows.Where(r => r.Tick > 0), row => Assert.Equal(0m, row.Data.Creeps.OwnedCreeps.Count));
        Assert.All(roomRows, row => Assert.Equal(1m, row.Data.Structures.Controller.Count));
    }

    [Theory]
    [InlineData(100, 100, 155, 300)]
    [InlineData(100, 1000, 155, 0)]
    public async Task Sync_AlignsInitialLookbackToFileAndWindowBoundaries(int file, int window, int lookback, long firstTick)
    {
        var shard = CreateShard(file, window);
        ConfigSettingsState.PullBackwardsTickAmount = lookback;
        shard.Time = 1000;
        await shard.StartSync();
        Assert.Equal(firstTick, requests.Min(r => r.Tick));
        Assert.All(requests, request => Assert.Equal(0, request.Tick % file));
        Assert.Equal((500 - firstTick) / window * 2, roomRows.Count);
    }

    [Theory]
    [InlineData(20, 659, 140)]
    [InlineData(25, 699, 175)]
    [InlineData(100, 400, 0)]
    [InlineData(100, 500, 0)]
    [InlineData(100, 3000000599, 3000000000)]
    public void SyncTime_UsesConfiguredFileSizeAndLongTicks(int file, long time, long expected)
    {
        var shard = CreateShard(file, file);
        shard.Time = time;
        Assert.Equal(expected, shard.GetSyncTime());
    }

    private static void AssertAverages(ScreepsRoomHistoryDto data, int rooms)
    {
        Assert.Equal(rooms, data.Structures.Controller.Count);
        Assert.Equal(4m * rooms, data.Structures.Controller.Level);
        Assert.Equal(rooms, data.Creeps.OwnedCreeps.Count);
        Assert.Equal(rooms, data.Creeps.OwnedCreeps.ActionLog.Harvest.Count);
        Assert.Equal(3m * rooms, data.GroundResources["energy"]);
    }

    private JObject History(long tick)
    {
        var snapshot = JObject.Parse("""
        {
            "controller": { "type": "controller", "level": 4 },
            "creep": { "type": "creep", "body": [], "actionLog": { "harvest": { "x": 1, "y": 1 } } },
            "energy": { "type": "energy", "resourceType": "energy", "energy": 3 }
        }
        """);
        snapshot["controller"]!["user"] = userId;
        snapshot["creep"]!["user"] = userId;
        return new JObject
        {
            ["base"] = tick,
            ["timestamp"] = 1700000000000 + tick * 200,
            ["ticks"] = new JObject { [tick.ToString()] = snapshot }
        };
    }
}
