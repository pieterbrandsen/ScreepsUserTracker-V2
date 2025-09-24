using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reactive;
using UserTrackerShared.DBClients.TimeScale;
using UserTrackerShared.DBClients.TimeScaleDB;
using UserTrackerShared.Models;
using UserTrackerShared.Models.ScreepsAPI;
using UserTrackerShared.States;
using UserTrackerShared.Utilities;

namespace UserTrackerShared.DBClients
{
    public static class DBClient
    {
        public static void Init()
        {
            if (ConfigSettingsState.InfluxDbEnabled)
            {
                InfluxDBClientWriter.Init();
            }
            if (ConfigSettingsState.GraphiteDbEnabled)
            {
                GraphiteDBClientWriter.Init();
            }
            if (ConfigSettingsState.TimeScaleDbEnabled)
            {
                Task.Delay(30000).Wait();
                var connString = $"Host={ConfigSettingsState.TimeScaleDbHost};Port={ConfigSettingsState.TimeScaleDbPort};Database={ConfigSettingsState.TimeScaleDbDBName};Username={ConfigSettingsState.TimeScaleDbUser};Password={ConfigSettingsState.TimeScaleDbPassword};";
                Screen.AddLog($"TimeScaleDB Connection String: {connString}");
                var host = Host.CreateDefaultBuilder()
                    .ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                        logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
                    })
                  .ConfigureServices((ctx, services) =>
                    {
                        services.AddDbContext<AppDbContext>(opts =>
                            opts.UseNpgsql(connString));
                    })
                    .Build();

                var scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();
                TimeScaleDBClientWriter.Init(scopeFactory);
                Screen.UpdateSize();
            }
            if (ConfigSettingsState.QuestDbEnabled)
            {
                QuestDBClientWriter.Init();
            }
        }

        public static void WriteScreepsRoomHistory(string shard, string room, long tick, long timestamp, ScreepsRoomHistoryDto screepsRoomHistory)
        {
            // if (ConfigSettingsState.InfluxDbEnabled)
            // {
            //     await InfluxDBClientState.WriteScreepsRoomHistory(shard, room, tick, timestamp, screepsRoomHistory);
            // }
            // if (ConfigSettingsState.GraphiteDbEnabled)
            // {
            //     await GraphiteDBClientState.WriteScreepsRoomHistory(shard, room, tick, timestamp, screepsRoomHistory);
            // }
            // if (ConfigSettingsState.TimeScaleDbEnabled)
            // {
            //     await TimeScaleDBClientState.WriteScreepsRoomHistory(shard, room, tick, timestamp, screepsRoomHistory);
            // }
            if (ConfigSettingsState.QuestDbEnabled)
            {
                QuestDBClientState.WriteScreepsRoomHistory(shard, room, tick, timestamp, screepsRoomHistory);
            }
        }

        public static void WriteScreepsUserHistory(string shard, string user, long tick, long timestamp, ScreepsRoomHistoryDto screepsRoomHistory)
        {
            //if (ConfigSettingsState.InfluxDbEnabled)
            //{
            //    await InfluxDBClientState.WriteScreepsRoomHistory(shard, room, tick, timestamp, screepsRoomHistory);
            //}
            //if (ConfigSettingsState.GraphiteDbEnabled)
            //{
            //    await GraphiteDBClientState.WriteScreepsRoomHistory(shard, room, tick, timestamp, screepsRoomHistory);
            //}
            //if (ConfigSettingsState.TimeScaleDbEnabled)
            //{
            //    await TimeScaleDBClientState.WriteScreepsRoomHistory(shard, room, tick, timestamp, screepsRoomHistory);
            //}
            if (ConfigSettingsState.QuestDbEnabled)
            {
                QuestDBClientState.WriteScreepsUserHistory(shard, user, tick, timestamp, screepsRoomHistory);
            }
        }

        public static void WriteScreepsGlobalHistory(string shard, long tick, long timestamp, ScreepsRoomHistoryDto screepsRoomHistory)
        {
            //if (ConfigSettingsState.InfluxDbEnabled)
            //{
            //    await InfluxDBClientState.WriteScreepsRoomHistory(shard, room, tick, timestamp, screepsRoomHistory);
            //}
            //if (ConfigSettingsState.GraphiteDbEnabled)
            //{
            //    await GraphiteDBClientState.WriteScreepsRoomHistory(shard, room, tick, timestamp, screepsRoomHistory);
            //}
            //if (ConfigSettingsState.TimeScaleDbEnabled)
            //{
            //    await TimeScaleDBClientState.WriteScreepsRoomHistory(shard, room, tick, timestamp, screepsRoomHistory);
            //}
            if (ConfigSettingsState.QuestDbEnabled)
            {
                QuestDBClientState.WriteScreepsGlobalHistory(shard, tick, timestamp, screepsRoomHistory);
            }
        }

        public static void WritePerformanceData(PerformanceClassDto PerformanceClassDto)
        {
            if (ConfigSettingsState.InfluxDbEnabled)
            {
                InfluxDBClientState.WritePerformanceData(PerformanceClassDto);
            }
            if (ConfigSettingsState.GraphiteDbEnabled)
            {
                GraphiteDBClientState.WritePerformanceData(PerformanceClassDto);
            }
            if (ConfigSettingsState.TimeScaleDbEnabled)
            {
                TimeScaleDBClientState.WritePerformanceData(PerformanceClassDto);
            }
            if (ConfigSettingsState.QuestDbEnabled)
            {
                _ = QuestDBClientState.WritePerformanceData(PerformanceClassDto);
            }
        }

        public static async Task WriteHistoricalLeaderboardData(SeasonListItem seasonItem)
        {
            if (ConfigSettingsState.InfluxDbEnabled)
            {
                // InfluxDBClientState.WriteHistoricalLeaderboardData(seasonItem);
            }
            if (ConfigSettingsState.GraphiteDbEnabled)
            {
                GraphiteDBClientState.WriteHistoricalLeaderboardData(seasonItem);
            }
            if (ConfigSettingsState.TimeScaleDbEnabled)
            {
                TimeScaleDBClientState.WriteHistoricalLeaderboardData(seasonItem);
            }
            if (ConfigSettingsState.QuestDbEnabled)
            {
                await QuestDBClientState.WriteHistoricalLeaderboardData(seasonItem);
            }
        }

        public static async Task WriteCurrentLeaderboardData(SeasonListItem seasonItem)
        {
            if (ConfigSettingsState.InfluxDbEnabled)
            {
            }
            if (ConfigSettingsState.GraphiteDbEnabled)
            {
            }
            if (ConfigSettingsState.TimeScaleDbEnabled)
            {
            }
            if (ConfigSettingsState.QuestDbEnabled)
            {
                await QuestDBClientState.WriteCurrentLeaderboardData(seasonItem);
            }
        }

        public static async Task WriteSingleUserData(ScreepsUser user)
        {
            if (ConfigSettingsState.InfluxDbEnabled)
            {
                // InfluxDBClientState.WriteSingleUserData(user);
            }
            if (ConfigSettingsState.GraphiteDbEnabled)
            {
                GraphiteDBClientState.WriteSingleUserData(user);
            }
            if (ConfigSettingsState.TimeScaleDbEnabled)
            {
                TimeScaleDBClientState.WriteSingleUserData(user);
            }
            if (ConfigSettingsState.QuestDbEnabled)
            {
                await QuestDBClientState.WriteSingleUserData(user);
            }
        }

        public static void WriteAdminUtilsData(AdminUtilsResponse data)
        {
            var dto = new AdminUtilsDto(data);
            if (ConfigSettingsState.InfluxDbEnabled)
            {
                // InfluxDBClientState.WriteSingleUserdData(user);
            }
            if (ConfigSettingsState.GraphiteDbEnabled)
            {
                GraphiteDBClientState.WriteAdminUtilsData(dto);
            }
            if (ConfigSettingsState.TimeScaleDbEnabled)
            {
                TimeScaleDBClientState.WriteAdminUtilsData(dto);
            }
            if (ConfigSettingsState.QuestDbEnabled)
            {
                QuestDBClientState.WriteAdminUtilsData(dto);
            }
        }
    }
}
