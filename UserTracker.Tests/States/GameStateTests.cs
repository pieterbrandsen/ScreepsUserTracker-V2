using System.Collections.Concurrent;
using UserTrackerShared.Models;
using UserTrackerShared.States;

namespace UserTracker.Tests.States
{
    public class GameStateTests : IDisposable
    {
        public GameStateTests()
        {
            GameState.Users = new ConcurrentDictionary<string, ScreepsUser>();
        }

        public void Dispose()
        {
            GameState.Users = new ConcurrentDictionary<string, ScreepsUser>();
        }

        [Fact]
        public void GetUsersForWrite_DeduplicatesAndSkipsMissingUsers()
        {
            GameState.Users["u1"] = new ScreepsUser { Username = "one" };
            GameState.Users["u2"] = new ScreepsUser { Username = "two" };

            var users = GameState.GetUsersForWrite(new[] { "u1", "u1", "missing", "", "u2" });

            Assert.Equal(2, users.Count);
            Assert.Collection(
                users,
                user => Assert.Equal("one", user.Username),
                user => Assert.Equal("two", user.Username));
        }

        [Fact]
        public async Task WriteUsersByIds_WritesOnlySelectedUsers()
        {
            GameState.Users["u1"] = new ScreepsUser { Username = "one" };
            GameState.Users["u2"] = new ScreepsUser { Username = "two" };
            GameState.Users["u3"] = new ScreepsUser { Username = "three" };

            var writtenUsers = new List<string>();
            var count = await GameState.WriteUsersByIds(
                new[] { "u2", "u2", "u1", "missing" },
                user =>
                {
                    writtenUsers.Add(user.Username);
                    return Task.CompletedTask;
                });

            Assert.Equal(2, count);
            Assert.Equal(new[] { "two", "one" }, writtenUsers);
            Assert.DoesNotContain("three", writtenUsers);
        }

        [Fact]
        public async Task TryRunWithLeaderboardLock_SkipsSecondConcurrentInvocation()
        {
            var firstStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseFirst = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var first = GameState.TryRunWithLeaderboardLock("first", async () =>
            {
                firstStarted.SetResult(true);
                await releaseFirst.Task;
            });

            await firstStarted.Task;

            var secondActionRan = false;
            var second = await GameState.TryRunWithLeaderboardLock("second", () =>
            {
                secondActionRan = true;
                return Task.CompletedTask;
            });

            Assert.False(second);
            Assert.False(secondActionRan);

            releaseFirst.SetResult(true);
            Assert.True(await first);
        }

        [Fact]
        public async Task MergeUsersBackfillingGclAsync_UpsertsAndBackfillsId()
        {
            var merged = await GameState.MergeUsersBackfillingGclAsync(new Dictionary<string, ScreepsUser>
            {
                ["u1"] = new ScreepsUser
                {
                    Username = "one",
                    Id = "",
                    GCL = 1
                },
                ["u2"] = new ScreepsUser
                {
                    Username = "two",
                    Id = "u2",
                    GCL = 2
                }
            });

            Assert.Equal(2, merged);
            Assert.True(GameState.Users.TryGetValue("u1", out var user1));
            Assert.True(GameState.Users.TryGetValue("u2", out var user2));
            Assert.Equal("u1", user1?.Id);
            Assert.Equal("one", user1?.Username);
            Assert.Equal("u2", user2?.Id);
            Assert.Equal("two", user2?.Username);
        }

        [Fact]
        public async Task MergeUsersBackfillingGclAsync_SkipsInvalidEntries()
        {
            var merged = await GameState.MergeUsersBackfillingGclAsync(new Dictionary<string, ScreepsUser>
            {
                [""] = new ScreepsUser { Username = "invalid-id" },
                ["u1"] = new ScreepsUser { Username = "" },
                ["u2"] = new ScreepsUser { Username = "ok", GCL = 1 }
            });

            Assert.Equal(1, merged);
            Assert.Single(GameState.Users);
            Assert.True(GameState.Users.ContainsKey("u2"));
        }

        [Fact]
        public async Task MergeUsersBackfillingGclAsync_FetchesUsersWithZeroGcl()
        {
            var fetchedUserIds = new List<string>();

            var merged = await GameState.MergeUsersBackfillingGclAsync(
                new Dictionary<string, ScreepsUser>
                {
                    ["u1"] = new ScreepsUser
                    {
                        Username = "one",
                        GCL = 0
                    },
                    ["u2"] = new ScreepsUser
                    {
                        Username = "two",
                        GCL = 42
                    }
                },
                userId =>
                {
                    fetchedUserIds.Add(userId);
                    return Task.FromResult<ScreepsUser?>(new ScreepsUser
                    {
                        Id = userId,
                        Username = "one",
                        GCL = 123
                    });
                });

            Assert.Equal(2, merged);
            Assert.Equal(new[] { "u1" }, fetchedUserIds);
            Assert.Equal(123, GameState.Users["u1"].GCL);
            Assert.Equal(42, GameState.Users["u2"].GCL);
        }
    }
}
