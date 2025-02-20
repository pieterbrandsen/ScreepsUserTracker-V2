using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading.Tasks;
using UserTrackerScreepsApi;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerShared.States;


namespace UserTrackerStates
{
    public class PerformanceClassDTO
    {
        public string Shard { get; set; }
        public long TicksBehind { get; set; }
        public long TimeTakenMs { get; set; }
        public int TotalRooms { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount => TotalRooms - SuccessCount;
    }
    public static class InfluxDBClientState
    {
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.InfluxDB);

        private static readonly JsonSerializer _serializer = JsonSerializer.CreateDefault();
        private static WriteApi _writeApi;

        public static void Init()
        {
            string host = "http://influxdb:8061";
            string token = ConfigSettingsState.InfluxDbToken;
            
            var client = new InfluxDBClient(host, token);
            _writeApi = client.GetWriteApi();
        }

        private static PointData CreatePoint(string measurement, string shard, string room, long tick, long timestamp, string username, string field, object value)
        {
            var point = PointData.Measurement(measurement)
                .Tag("shard", shard)
                .Tag("room", room)
                .Field(field, value)
                .Field("tick", tick)
                .Timestamp(timestamp, InfluxDB.Client.Api.Domain.WritePrecision.Ms);
            if (!string.IsNullOrEmpty(username))
            {
                point = point.Tag("user", username);
            }
            return point;
        }
        private static void UploadData(string shard, string room, long tick, long timestamp, string username, string bucket, object obj)
        {
            var flattenedData = new Dictionary<string, object>();
            var writer = new JTokenWriter();
            _serializer.Serialize(writer, obj);
            FlattenJson(writer.Token!, new StringBuilder(), flattenedData);
            var points = new List<PointData>();
            foreach (var kvp in flattenedData)
            {
                if (kvp.Value is long)
                {
                    points.Add(CreatePoint(ConfigSettingsState.InfluxDbServer, shard, room, tick, timestamp, username, kvp.Key, kvp.Value));
                }
            }
            if (points.Count > 0)
            {
                _writeApi.WritePoints(points, bucket:bucket, org:"screeps");
            }
        }
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
                _logger.Information($"Trying to upload {shard}/{room}/{tick}{(!string.IsNullOrEmpty(username) ? $"from {username}" : "")}");


                if (screepsRoomHistory.Structures.Controller != null) UploadData(shard, room, tick, timestamp, username, "history_structure_controller", screepsRoomHistory.Structures.Controller);
                UploadData(shard, room, tick, timestamp, username, "history_structure_mineral", screepsRoomHistory.Structures.Mineral);
                UploadData(shard, room, tick, timestamp, username, "history_structure_deposit", screepsRoomHistory.Structures.Deposit);
                UploadData(shard, room, tick, timestamp, username, "history_structure_wall", screepsRoomHistory.Structures.Wall);
                UploadData(shard, room, tick, timestamp, username, "history_structure_constructionsite", screepsRoomHistory.Structures.ConstructionSite);
                UploadData(shard, room, tick, timestamp, username, "history_structure_container", screepsRoomHistory.Structures.Container);
                UploadData(shard, room, tick, timestamp, username, "history_structure_extension", screepsRoomHistory.Structures.Extension);
                UploadData(shard, room, tick, timestamp, username, "history_structure_extractor", screepsRoomHistory.Structures.Extractor);
                UploadData(shard, room, tick, timestamp, username, "history_structure_factory", screepsRoomHistory.Structures.Factory);
                UploadData(shard, room, tick, timestamp, username, "history_structure_invadercore", screepsRoomHistory.Structures.InvaderCore);
                UploadData(shard, room, tick, timestamp, username, "history_structure_keeperlair", screepsRoomHistory.Structures.KeeperLair);
                UploadData(shard, room, tick, timestamp, username, "history_structure_lab", screepsRoomHistory.Structures.Lab);
                UploadData(shard, room, tick, timestamp, username, "history_structure_link", screepsRoomHistory.Structures.Link);
                UploadData(shard, room, tick, timestamp, username, "history_structure_observer", screepsRoomHistory.Structures.Observer);
                UploadData(shard, room, tick, timestamp, username, "history_structure_portal", screepsRoomHistory.Structures.Portal);
                UploadData(shard, room, tick, timestamp, username, "history_structure_powerbank", screepsRoomHistory.Structures.PowerBank);
                UploadData(shard, room, tick, timestamp, username, "history_structure_powerspawn", screepsRoomHistory.Structures.PowerSpawn);
                UploadData(shard, room, tick, timestamp, username, "history_structure_rampart", screepsRoomHistory.Structures.Rampart);
                UploadData(shard, room, tick, timestamp, username, "history_structure_road", screepsRoomHistory.Structures.Road);
                UploadData(shard, room, tick, timestamp, username, "history_structure_ruin", screepsRoomHistory.Structures.Ruin);
                UploadData(shard, room, tick, timestamp, username, "history_structure_source", screepsRoomHistory.Structures.Source);
                UploadData(shard, room, tick, timestamp, username, "history_structure_spawn", screepsRoomHistory.Structures.Spawn);
                UploadData(shard, room, tick, timestamp, username, "history_structure_storage", screepsRoomHistory.Structures.Storage);
                UploadData(shard, room, tick, timestamp, username, "history_structure_terminal", screepsRoomHistory.Structures.Terminal);
                UploadData(shard, room, tick, timestamp, username, "history_structure_tombstone", screepsRoomHistory.Structures.Tombstone);
                UploadData(shard, room, tick, timestamp, username, "history_structure_nuker", screepsRoomHistory.Structures.Nuker);
                UploadData(shard, room, tick, timestamp, username, "history_structure_nuke", screepsRoomHistory.Structures.Nuke);

                UploadData(shard, room, tick, timestamp, username, "history_creep_owned", screepsRoomHistory.Creeps.OwnedCreeps);
                UploadData(shard, room, tick, timestamp, username, "history_creep_enemy", screepsRoomHistory.Creeps.EnemyCreeps);
                UploadData(shard, room, tick, timestamp, username, "history_creep_other", screepsRoomHistory.Creeps.OtherCreeps);
                UploadData(shard, room, tick, timestamp, username, "history_creep_power", screepsRoomHistory.Creeps.PowerCreeps);

                UploadData(shard, room, tick, timestamp, username, "history_groundresource", screepsRoomHistory.GroundResources);
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
                var point = PointData
                            .Measurement(ConfigSettingsState.InfluxDbServer)
                            .Tag("shard", performanceClassDTO.Shard)
                            .Field("TicksBehind", performanceClassDTO.TicksBehind)
                            .Field("TimeTakenMs", performanceClassDTO.TimeTakenMs)
                            .Field("TotalRooms", performanceClassDTO.TotalRooms)
                            .Field("SuccessCount", performanceClassDTO.SuccessCount)
                            .Field("FailedCount", performanceClassDTO.FailedCount)
                            .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

                _writeApi.WritePoint(point, bucket:"history_performance",org:"screeps");
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error uploading performance data");
            }
        }

        private static void FlattenJson(JToken token, StringBuilder currentPath, IDictionary<string, object> dict)
        {
            switch (token)
            {
                case JObject obj:
                    foreach (var prop in obj.Properties())
                    {
                        int initialLen = currentPath.Length;
                        currentPath.Append($"{prop.Name}.");
                        FlattenJson(prop.Value, currentPath, dict);
                        currentPath.Length = initialLen; // Reset path
                    }
                    break;

                case JArray array:
                    for (int i = 0; i < array.Count; i++)
                    {
                        int initialLen = currentPath.Length;
                        currentPath.Append($"{i}.");
                        FlattenJson(array[i], currentPath, dict);
                        currentPath.Length = initialLen; // Reset path
                    }
                    break;

                case JValue jValue:
                    if (currentPath.Length > 0)
                        currentPath.Length--; // Remove trailing "."
                    dict[currentPath.ToString().ToLower()] = jValue.Value;
                    break;
            }
        }
    }
}
