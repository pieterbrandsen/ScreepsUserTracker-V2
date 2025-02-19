using InfluxDB3.Client;
using InfluxDB3.Client.Write;
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
        private static InfluxDBClient _client;
        private static InfluxDBClient _clientPerformance;

        public static void Init()
        {
            string host = "http://influxdb:8181";
            string token = ConfigSettingsState.InfluxDbToken;

            _client = new InfluxDBClient(host, token: token, database: "history");
            _clientPerformance = new InfluxDBClient(host, token: token, database: "history_performance");
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
                    var apiUser = ScreepsAPI.GetUser(userId).GetAwaiter().GetResult();
                    if (apiUser != null)
                    {
                        GameState.Users.Add(userId, apiUser);
                    }
                }

                _logger.Information($"Trying to upload {shard}/{room}/{tick}{(!string.IsNullOrEmpty(username) ? $"from {username}" : "")}");
                
                var flattenedData = new Dictionary<string, object>();
                var writer = new JTokenWriter();
                _serializer.Serialize(writer, screepsRoomHistory);
                FlattenJson(writer.Token!, new StringBuilder(), flattenedData);

                var points = new List<PointData>();
                foreach (var kvp in flattenedData)
                {
                    if (kvp.Value is double || kvp.Value is float || kvp.Value is int || kvp.Value is long)
                    {
                        var point = PointData.Measurement(ConfigSettingsState.InfluxDbServer)
                            .SetTag("shard", shard)
                            .SetTag("room", room)
                            .SetField(kvp.Key, Convert.ToInt64(kvp.Value))
                            .SetField("tick", tick)
                            .SetTimestamp(timestamp);

                        if (!string.IsNullOrEmpty(username))
                        {
                            point = point.SetTag("user", username);
                        }
                        points.Add(point);

                         if (points.Count >= 100)
                        {
                            await _client.WritePointsAsync(points, headers:new Dictionary<string, string>(){ { "Content-Encoding", "gzip" }});
                            points.Clear(); // Clear the batch after uploading
                        }
                    }
                }

                if (points.Count > 0)
                {
                    await _client.WritePointsAsync(points, headers:new Dictionary<string, string>(){ { "Content-Encoding", "gzip" }});
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error uploading {shard}/{room}/{tick}");
            }
        }

        public static async Task WritePerformanceData(PerformanceClassDTO performanceClassDTO)
        {
            try
            {
                var point = PointData
                            .Measurement(ConfigSettingsState.InfluxDbServer)
                            .SetTag("shard", performanceClassDTO.Shard)
                            .SetField("TicksBehind", performanceClassDTO.TicksBehind)
                            .SetField("TimeTakenMs", performanceClassDTO.TimeTakenMs)
                            .SetField("TotalRooms", performanceClassDTO.TotalRooms)
                            .SetField("SuccessCount", performanceClassDTO.SuccessCount)
                            .SetField("FailedCount", performanceClassDTO.FailedCount)
                            .SetTimestamp(DateTime.UtcNow);

                await _clientPerformance.WritePointAsync(point);
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
