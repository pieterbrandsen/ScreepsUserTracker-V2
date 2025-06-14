using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Reactive;
using UserTrackerShared.DBClients.TimeScaleDB;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerShared.Models.ScreepsAPI;
using UserTrackerShared.Models.TimeScaleDB;
using UserTrackerShared.States;


namespace UserTrackerShared.DBClients.TimeScale
{
    public static class TimeScaleDBClientWriter
    {
        private static IServiceScopeFactory _scopeFactory = null!;
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.TimeScaleDB);
        private static bool _isInitialized = false;
        private static long _writtenDataCount = 0;

        public static void Init(IServiceScopeFactory scopeFactory)
        {
            if (_isInitialized)
            {
                _logger.Debug("TimeScaleDB client already initialized, skipping initialization.");
                return;
            }

            _scopeFactory = scopeFactory;
            _logger.Information("Initializing TimeScaleDB client...");

            try
            {
                _isInitialized = true;
                _logger.Information("TimeScaleDB client connected");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error initializing TimeScaleDB client.");
                throw;
            }


            Task.Run(LogStatusPeriodically);
            using var masterConn = new NpgsqlConnection(
                            new NpgsqlConnectionStringBuilder
                            {
                                Host = ConfigSettingsState.TimeScaleDbHost,
                                Port = ConfigSettingsState.TimeScaleDbPort,
                                Database = "postgres",
                                Username = ConfigSettingsState.TimeScaleDbUser,
                                Password = ConfigSettingsState.TimeScaleDbPassword
                            }.ConnectionString);

            masterConn.Open();
            using (var cmd = masterConn.CreateCommand())
            {
                cmd.CommandText =
                  $@"CREATE DATABASE ""{ConfigSettingsState.TimeScaleDbDBName}"";";
                try { cmd.ExecuteNonQuery(); }
                catch (PostgresException ex) when (ex.SqlState == "42P04") { /* already exists */ }
            }

            // Apply EF Core migrations in a scope
            using var migrateScope = _scopeFactory.CreateScope();
            var migrator = migrateScope.ServiceProvider.GetRequiredService<AppDbContext>();
            migrator.Database.Migrate();

            _logger.Information("Worker tasks started.");
        }

        public static void UploadPerformanceData(string server, PerformanceClassDto obj)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var entity = new TimeScalePerformanceClassDto
                {
                    Server = server,
                    ResultCodes = obj.ResultCodes,
                    Shard = obj.Shard,
                    TicksBehind = obj.TicksBehind,
                    TimeTakenMs = obj.TimeTakenMs,
                    TotalRooms = obj.TotalRooms,
                };

                db.PerformanceStats.Add(entity);
                db.SaveChanges();
                Interlocked.Add(ref _writtenDataCount, 1);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UploadData");
            }
        }

        public static void UploadScreepsRoomHistory(string serverName, string shard, string room, string username, ScreepsRoomHistoryDto obj)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var entity = new TimeScaleScreepsRoomHistoryDto
                {
                    Server = serverName,
                    Shard = shard,
                    Room = room,
                    Username = username,
                    GroundResources = obj.GroundResources,
                    Creeps = obj.Creeps,
                    Structures = obj.Structures,
                };
                db.ScreepsRoomHistory.Add(entity);
                db.SaveChanges();
                Interlocked.Add(ref _writtenDataCount, 1);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UploadData");
            }
        }

        public static void UploadLeaderboardData(string serverName, SeasonListItem seasonItem)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var entity = new TimeScaleSeasonItem
                {
                    Server = serverName,
                    Type = seasonItem.Type,
                    Season = seasonItem.Season,
                    UserId = seasonItem.UserId,
                    UserName = seasonItem.UserName,
                    Score = seasonItem.Score,
                    Rank = seasonItem.Rank,
                };
                db.SeasonItems.Add(entity);
                db.SaveChanges();
                Interlocked.Add(ref _writtenDataCount, 1);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UploadData");
            }
        }

        public static void UploadSingleUserData(string serverName, ScreepsUser user)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var entity = new TimeScaleScreepsUser
                {
                    Server = serverName,
                    Badge = user.Badge,
                    Username = user.Username,
                    GCL = user.GCL,
                    Power = user.Power,
                    GCLRank = user.GCLRank,
                    PowerRank = user.PowerRank,
                };
                db.Users.Add(entity);
                db.SaveChanges();
                Interlocked.Add(ref _writtenDataCount, 1);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UploadData");
            }
        }

        public static void UploadAdminUtilsData(string serverName, AdminUtilsDto data)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var entity = new TimeScaleAdminUtilsDto
                {
                    Server = serverName,
                    Objects= data.Objects,
                    Ticks = data.Ticks,
                    Users = data.Users,
                    ActiveUsers = data.ActiveUsers,
                    ActiveRooms = data.ActiveRooms,
                    TotalRooms = data.TotalRooms,
                    OwnedRooms = data.OwnedRooms,
                    GameTime = data.GameTime,   
                };
                db.AdminUtilsData.Add(entity);
                db.SaveChanges();
                Interlocked.Add(ref _writtenDataCount, 1);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UploadData");
            }
        }

        private static async Task LogStatusPeriodically()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                using var scope = _scopeFactory.CreateScope();
                
                var flushed = Interlocked.Exchange(ref _writtenDataCount, 0);
                _logger.Information("Added {Flushed} rows in the last 10 seconds", flushed);
            }
        }
    }


    public static class TimeScaleDBClientState
    {
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.TimeScaleDB);
        public static async Task WriteScreepsRoomHistory(string shard, string room, long tick, long timestamp, ScreepsRoomHistoryDto screepsRoomHistory)
        {
            try
            {
                var userId = screepsRoomHistory.Structures.Controller?.UserId ?? screepsRoomHistory.Structures.Controller?.ReservationUserId ?? "";
                var username = "none";
                GameState.Users.TryGetValue(userId, out var user);
                if (user != null)
                {
                    username = user.Username;
                }
                else if (!string.IsNullOrEmpty(userId))
                {
                    var apiUser = await ScreepsApi.GetUser(userId);
                    if (apiUser != null)
                    {
                        GameState.Users.TryAdd(userId, apiUser);
                    }
                }

                TimeScaleDBClientWriter.UploadScreepsRoomHistory(ConfigSettingsState.ServerName, shard, room, username, screepsRoomHistory);
            }
            catch (Exception e)
            {
                var errorMessage = string.Format("Error uploading {0}/{1}/{2}", shard, room, tick);
                _logger.Error(e, errorMessage);
            }
        }

        public static void WritePerformanceData(PerformanceClassDto PerformanceClassDto)
        {
            try
            {
                TimeScaleDBClientWriter.UploadPerformanceData(ConfigSettingsState.ServerName, PerformanceClassDto);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error uploading performance data");
            }
        }

        public static void WriteLeaderboardData(SeasonListItem seasonItem)
        {
            try
            {
                string[] parts = seasonItem.Season.Split('-');
                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);

                DateTime dateTime = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                var timestamp = ((DateTimeOffset)dateTime).ToUnixTimeMilliseconds();
                TimeScaleDBClientWriter.UploadLeaderboardData(ConfigSettingsState.ServerName, seasonItem);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error uploading performance data");
            }
        }

        public static void WriteSingleUserData(ScreepsUser user)
        {
            try
            {
                TimeScaleDBClientWriter.UploadSingleUserData(ConfigSettingsState.ServerName, user);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error uploading performance data");
            }
        }

        public static void WriteAdminUtilsData(AdminUtilsDto data)
        {
            try
            {
                TimeScaleDBClientWriter.UploadAdminUtilsData(ConfigSettingsState.ServerName, data);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error uploading performance data");
            }
        }
    }
}
