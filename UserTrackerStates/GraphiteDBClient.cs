using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using UserTrackerScreepsApi;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerShared.States;


namespace UserTrackerStates
{
    public static class GraphiteDBClientWriter
    {
        private static readonly JsonSerializer _serializer = JsonSerializer.CreateDefault();
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.GraphiteDB);
        private static bool _isInitialized = false;

        // Counters for statistics.
        private static long _flushedPointCount = 0;
        private static long _pendingPointCount = 0;

        public static void Init()
        {
            if (_isInitialized)
            {
                _logger.Debug("GraphiteDB client already initialized, skipping initialization.");
                return;
            }

            _logger.Information("Initializing InfluxDB client...");
            string host = "http://influxdb:8086";
            string token = ConfigSettingsState.InfluxDbToken;

            try
            {
                _isInitialized = true;
                _logger.Information("GraphiteDB client connected to host {Host}", host);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error initializing GraphiteDB client.");
                throw;
            }

            // Start a background task to log status.
            Task.Run(LogStatusPeriodically);

            _logger.Information("Worker tasks started.");
        }

        public static void UploadData()
        {
            try
            {
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UploadData");
            }
        }

        private static async Task WorkerLoop()
        {
        }

        private static async Task LogStatusPeriodically()
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            var flushed = Interlocked.Exchange(ref _flushedPointCount, 0);
            var pending = Interlocked.Read(ref _pendingPointCount);
            _logger.Information("Flushed {Flushed} points in the last 10 seconds. Pending points: {Pending}", flushed, pending);
        }
    }


    public static class GraphiteDBClientState
    {
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.GraphiteDB);
        public static async Task WriteScreepsRoomHistory(string shard, string room, long tick, long timestamp, ScreepsRoomHistoryDTO screepsRoomHistory)
        {
            try
            {
                var userId = screepsRoomHistory.Structures.Controller?.UserId ?? "";
                var username = "";
                GameState.Users.TryGetValue(userId, out var user);
                if (user != null)
                {
                    username = user.Username;
                }
                else if (!string.IsNullOrEmpty(userId))
                {
                    var apiUser = await ScreepsAPI.GetUser(userId);
                    if (apiUser != null)
                    {
                        GameState.Users.Add(userId, apiUser);
                    }
                }

                //if (screepsRoomHistory.Structures.Controller != null) InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_controller", screepsRoomHistory.Structures.Controller);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_mineral", screepsRoomHistory.Structures.Mineral);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_deposit", screepsRoomHistory.Structures.Deposit);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_wall", screepsRoomHistory.Structures.Wall);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_constructionsite", screepsRoomHistory.Structures.ConstructionSite);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_container", screepsRoomHistory.Structures.Container);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_extension", screepsRoomHistory.Structures.Extension);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_extractor", screepsRoomHistory.Structures.Extractor);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_factory", screepsRoomHistory.Structures.Factory);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_invadercore", screepsRoomHistory.Structures.InvaderCore);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_keeperlair", screepsRoomHistory.Structures.KeeperLair);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_lab", screepsRoomHistory.Structures.Lab);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_link", screepsRoomHistory.Structures.Link);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_observer", screepsRoomHistory.Structures.Observer);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_portal", screepsRoomHistory.Structures.Portal);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_powerbank", screepsRoomHistory.Structures.PowerBank);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_powerspawn", screepsRoomHistory.Structures.PowerSpawn);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_rampart", screepsRoomHistory.Structures.Rampart);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_road", screepsRoomHistory.Structures.Road);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_ruin", screepsRoomHistory.Structures.Ruin);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_source", screepsRoomHistory.Structures.Source);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_spawn", screepsRoomHistory.Structures.Spawn);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_storage", screepsRoomHistory.Structures.Storage);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_terminal", screepsRoomHistory.Structures.Terminal);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_tombstone", screepsRoomHistory.Structures.Tombstone);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_nuker", screepsRoomHistory.Structures.Nuker);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_nuke", screepsRoomHistory.Structures.Nuke);

                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_creep_owned", screepsRoomHistory.Creeps.OwnedCreeps);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_creep_enemy", screepsRoomHistory.Creeps.EnemyCreeps);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_creep_other", screepsRoomHistory.Creeps.OtherCreeps);
                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_creep_power", screepsRoomHistory.Creeps.PowerCreeps);

                //InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_groundresource", screepsRoomHistory.GroundResources);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error uploading {shard}/{room}/{tick}");
            }
        }

        public static void WritePerformanceData(PerformanceClassDTO performanceClassDTO)
        {
            try
            {
                //var point = PointData
                //            .Measurement(ConfigSettingsState.InfluxDbServer)
                //            .Tag("shard", performanceClassDTO.Shard)
                //            .Field("TicksBehind", performanceClassDTO.TicksBehind)
                //            .Field("TimeTakenMs", performanceClassDTO.TimeTakenMs)
                //            .Field("TotalRooms", performanceClassDTO.TotalRooms)
                //            .Field("SuccessCount", performanceClassDTO.SuccessCount)
                //            .Field("FailedCount", performanceClassDTO.FailedCount)
                //            .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

                //InfluxDBClientWriter.AddPoint("history_performance", point);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error uploading performance data");
            }
        }

    }
}
