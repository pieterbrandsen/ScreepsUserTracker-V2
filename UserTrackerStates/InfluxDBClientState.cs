using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserTrackerShared.Models;


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
            var token = ConfigurationManager.AppSettings["INFLUXDB_TOKEN"] ?? "";
            _influxDBClient = new InfluxDBClient("http://influxdb.pandascreeps.com:8086", token);
            _writeAPI = _influxDBClient.GetWriteApi();
        }

        public static void WriteScreepsRoomHistory(string server, string shard, string room, long tick, long timestamp, ScreepsRoomHistoryDTO screepsRoomHistory)
        {
            try
            {
                var points  = new List<PointData>();
                var flattenedData = new Dictionary<string, object>();

                var writer = new JTokenWriter();
                _serializer.Serialize(writer, screepsRoomHistory);
                FlattenJson(writer.Token!, new StringBuilder(), flattenedData);

                foreach (var kvp in flattenedData)
                {
                    if (kvp.Value is double || kvp.Value is float || kvp.Value is int || kvp.Value is long)
                    {
                        var point = PointData
                            .Measurement(server)
                            .Tag("shard", shard)
                            .Tag("room", room)
                            .Field("tick", tick)
                            .Field(kvp.Key.ToLower(), Convert.ToInt64(kvp.Value))
                            .Timestamp(timestamp, WritePrecision.Ms);
                        points.Add(point);
                    }
                }

                _writeAPI.WritePoints(points, bucket: "history", org: "screeps");
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private static void FlattenJson(JToken token, StringBuilder currentPath, IDictionary<string, object> dict)
        {
            switch (token)
            {
                case JObject obj:
                    foreach (var prop in obj.Properties())
                    {
                        if (prop.Name == "PropertiesListDictionary" || prop.Name == "TypeMap" || prop.Name == "UserMap") continue;
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
