using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using UserTrackerScreepsApi;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerShared.States;


namespace UserTrackerStates
{
    public static class InfluxDBClientState
    {
        private static readonly JsonSerializer _serializer = JsonSerializer.CreateDefault();
        private static InfluxDBClient _influxDBClient;
        private static WriteApi _writeAPI;

        public static void Init()
        {
            // You can generate an API token from the "API Tokens Tab" in the UI
            _influxDBClient = new InfluxDBClient("http://influxdb.pandascreeps.com:8086", ConfigSettingsState.InfluxDbToken);
            _writeAPI = _influxDBClient.GetWriteApi();
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


                var points = new List<PointData>();
                var flattenedData = new Dictionary<string, object>();

                var writer = new JTokenWriter();
                _serializer.Serialize(writer, screepsRoomHistory);
                FlattenJson(writer.Token!, new StringBuilder(), flattenedData);

                foreach (var kvp in flattenedData)
                {
                    if (kvp.Value is double || kvp.Value is float || kvp.Value is int || kvp.Value is long)
                    {
                        var point = PointData
                            .Measurement(ConfigSettingsState.InfluxDbServer)
                            .Tag("shard", shard)
                            .Tag("room", room)
                            .Field(kvp.Key.ToLower(), Convert.ToInt64(kvp.Value))
                            .Field("tick", tick)
                            .Timestamp(timestamp, WritePrecision.Ms);

                        if (!string.IsNullOrEmpty(username))
                        {
                            point = point.Tag("user", username);
                        }

                        if (!string.IsNullOrEmpty(username))
                        {
                            point = point.Tag("user", username);
                        }
                        points.Add(point);
                    }
                }

                _writeAPI.WritePoints(points, bucket: "history", org: "screeps");
            }
            catch (Exception e)
            {
                throw;
            }

            await Task.CompletedTask;
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
                    dict[currentPath.ToString()] = jValue.Value;
                    break;
            }
        }
    }
}
