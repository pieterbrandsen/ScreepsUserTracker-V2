using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserTrackerShared.DBClients;
using UserTrackerShared.Models;
using UserTrackerShared.Models.ScreepsAPI;
using UserTrackerShared.States;
using Xunit;

namespace UserTracker.Tests.DBClients
{
    public class GraphiteDBClientStateTests : IDisposable
    {
        private const string ServerName = "unitTestServer";
        private readonly RecordingGraphiteBatchClient _recorder;

        public GraphiteDBClientStateTests()
        {
            _recorder = new RecordingGraphiteBatchClient();
            GraphiteDBClientWriter.ResetClientForTesting();
            GraphiteDBClientWriter.SetClientForTesting(_recorder);
            ConfigSettingsState.ServerName = ServerName;
            GameState.Users.Clear();
        }

        public void Dispose()
        {
            GraphiteDBClientWriter.ResetClientForTesting();
            GameState.Users.Clear();
            ConfigSettingsState.ServerName = string.Empty;
        }

        [Fact]
        public async Task WriteScreepsRoomHistory_UsesTrackedUserName()
        {
            const long timestamp = 5_000_000;
            const string shard = "shardX";
            const string room = "E1S1";
            const string userId = "user-42";
            GameState.Users[userId] = new ScreepsUser
            {
                Username = "player42",
                Badge = new MapStatUserBadge()
            };

            var dto = new ScreepsRoomHistoryDto();
            dto.Structures.Controller.UserId = userId;
            dto.Structures.Controller.Level = 3;

            await GraphiteDBClientState.WriteScreepsRoomHistory(shard, room, 100, timestamp, dto);

            var prefix = $"history.{ServerName}.data.{shard}.player42.{room}.";
            Assert.Contains(_recorder.Metrics, m => m.Path.StartsWith(prefix));
            Assert.All(_recorder.Metrics, m => Assert.Equal(timestamp, m.Timestamp));
        }

        [Fact]
        public async Task WriteScreepsRoomHistory_UsesFallbackUsername()
        {
            const long timestamp = 7_000;
            const string shard = "shardF";
            const string room = "E2N3";

            var dto = new ScreepsRoomHistoryDto();
            dto.Structures.Controller.UserId = string.Empty;
            dto.Structures.Controller.Level = 1;

            await GraphiteDBClientState.WriteScreepsRoomHistory(shard, room, 99, timestamp, dto);

            var prefix = $"history.{ServerName}.data.{shard}.none.{room}.";
            Assert.Contains(_recorder.Metrics, m => m.Path.StartsWith(prefix));
        }

        [Fact]
        public void WritePerformanceData_AppendsShardPrefix()
        {
            var dto = new PerformanceClassDto
            {
                Shard = "shardPerf",
                TicksBehind = 1,
                TimeTakenMs = 2,
                TotalRooms = 3
            };

            var before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            GraphiteDBClientState.WritePerformanceData(dto);
            var after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var prefix = $"history.{ServerName}.performance.{dto.Shard}.";
            Assert.Contains(_recorder.Metrics, m => m.Path.StartsWith(prefix));
            Assert.All(_recorder.Metrics, m => Assert.InRange(m.Timestamp, before, after));
        }

        [Fact]
        public void WriteHistoricalLeaderboardData_CalculatesTimestamp()
        {
            var seasonItem = new SeasonListItem
            {
                Season = "2030-07",
                Type = "power",
                UserName = "hero",
                UserId = "uid",
                Id = "seasonId",
                Score = 42,
                Rank = 1,
                Timestamp = DateTime.UtcNow
            };

            GraphiteDBClientState.WriteHistoricalLeaderboardData(seasonItem);

            var expectedTimestamp = new DateTime(2030, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            var expectedMs = new DateTimeOffset(expectedTimestamp).ToUnixTimeMilliseconds();
            var prefix = $"history.{ServerName}.leaderboard.{seasonItem.Type}.{seasonItem.UserName}.";
            var metric = _recorder.Metrics.First(m => m.Path.StartsWith(prefix));
            Assert.Equal(expectedMs, metric.Timestamp);
        }

        [Fact]
        public void WriteSingleUserData_IncludesUsernamePrefix()
        {
            var user = new ScreepsUser
            {
                Username = "solo",
                Badge = new MapStatUserBadge()
            };

            var before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            GraphiteDBClientState.WriteSingleUserData(user);
            var after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var prefix = $"history.{ServerName}.users.{user.Username}.";
            Assert.NotEmpty(_recorder.Metrics);
            Assert.All(_recorder.Metrics, m => Assert.StartsWith(prefix, m.Path));
            Assert.All(_recorder.Metrics, m => Assert.InRange(m.Timestamp, before, after));
        }

        [Fact]
        public void WriteAdminUtilsData_IncludesAdminPrefix()
        {
            var data = new AdminUtilsDto
            {
                ActiveUsers = 1,
                ActiveRooms = 2,
                TotalRooms = 3,
                OwnedRooms = 4,
                GameTime = 99,
                Objects = new AdminUtilsObjects
                {
                    Total = 10,
                    Creeps = 5
                },
                Ticks = new AdminUtilsTicks
                {
                    Average = 1.1,
                    Minimum = 1,
                    Maximum = 2,
                    MaxDeviation = 1,
                    Stages = new AdminUtilsStages()
                },
                Users = new Dictionary<string, AdminUtilsUser>()
            };

            var before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            GraphiteDBClientState.WriteAdminUtilsData(data);
            var after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var prefix = $"history.{ServerName}.adminutils.";
            Assert.Contains(_recorder.Metrics, m => m.Path.StartsWith(prefix));
            Assert.All(_recorder.Metrics, m => Assert.InRange(m.Timestamp, before, after));
        }

        private sealed class RecordingGraphiteBatchClient : IGraphiteBatchClient
        {
            public List<RecordedMetric> Metrics { get; } = new();

            public void AddMetric(string metricPath, double value, long timestamp)
            {
                Metrics.Add(new RecordedMetric(metricPath, value, timestamp));
            }

            public void Flush()
            {
            }
        }

        private readonly record struct RecordedMetric(string Path, double Value, long Timestamp);
    }
}
