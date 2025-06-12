using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading.Channels;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerShared.States;


namespace UserTrackerShared.DBClients
{
    public class PointDataParameter
    {
        public string Measurement { get; set; }
        public string Shard { get; set; }
        public string Room { get; set; }
        public long Tick { get; set; }
        public long Timestamp { get; set; }
        public string Username { get; set; }
        public string Field { get; set; }
        public object? Value { get; set; }

        public PointDataParameter(string measurement, string shard, string room, long tick, long timestamp, string username, string field, object? value)
        {
            Measurement = measurement;
            Shard = shard;
            Room = room;
            Tick = tick;
            Timestamp = timestamp;
            Username = username;
            Field = field;
            Value = value;
        }
    }
    public static class InfluxDBPointHelper
    {
        public static PointData CreatePoint(PointDataParameter parameters)
        {
            var point = PointData.Measurement(parameters.Measurement)
                .Tag("shard", parameters.Shard)
                .Tag("room", parameters.Room)
                .Field(parameters.Field, parameters.Value)
                .Field("tick", parameters.Tick)
                .Timestamp(parameters.Timestamp, WritePrecision.Ms);
            if (!string.IsNullOrEmpty(parameters.Username))
            {
                point = point.Tag("user", parameters.Username);
            }
            return point;
        }

        public static void FlattenJson(JToken token, StringBuilder currentPath, IDictionary<string, object?> dict)
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

    public static class InfluxDBClientWriter
    {
        private static readonly JsonSerializer _serializer = JsonSerializer.CreateDefault();
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.InfluxDB);
        private static InfluxDBClient _client = new InfluxDBClient(ConfigSettingsState.InfluxDbHost, ConfigSettingsState.InfluxDbToken);
        private static WriteApiAsync? _writeApi = null;
        private static bool _isInitialized = false;
        private static bool _isRunning = true;

        // Use an unbounded channel for high-performance concurrent writes.
        private static Channel<(string bucket, PointData point)>? _channel = null;

        // A pool of worker tasks that will process the channel.
        private static List<Task>? _workerTasks;
        private const int WorkerCount = 32;   // Number of concurrent worker tasks; tune per hardware.

        // Counters for statistics.
        private static long _flushedPointCount = 0;
        private static long _pendingPointCount = 0;

        public static void Init()
        {
            if (_isInitialized)
            {
                _logger.Debug("InfluxDB client already initialized, skipping initialization.");
                return;
            }

            _logger.Information("Initializing InfluxDB client...");
            try
            {
                _writeApi = _client.GetWriteApiAsync();
                _isInitialized = true;
                var message = string.Format("InfluxDB client connected to host {0}", ConfigSettingsState.InfluxDbHost);
                _logger.Information(message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error initializing InfluxDB client.");
                throw;
            }

            // Create an unbounded channel for point data.
            _channel = Channel.CreateUnbounded<(string bucket, PointData point)>(new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            });

            // Start worker tasks.
            _workerTasks = new List<Task>();
            for (int i = 0; i < WorkerCount; i++)
            {
                _workerTasks.Add(Task.Run(WorkerLoop));
            }

            // Start a background task to log status.
            Task.Run(LogStatusPeriodically);

            _logger.Information("Worker tasks started.");
        }

        public static void AddPoint(string bucket, PointData point)
        {
            // Increment pending counter when adding a new point.
            Interlocked.Increment(ref _pendingPointCount);
            _channel?.Writer.TryWrite((bucket, point));
        }

        public static void UploadData(string shard, string room, long tick, long timestamp, string username, string bucket, object obj)
        {
            try
            {
                var flattenedData = new Dictionary<string, object?>();
                var writer = new JTokenWriter();
                _serializer.Serialize(writer, obj);
                InfluxDBPointHelper.FlattenJson(writer.Token!, new StringBuilder(), flattenedData);

                foreach (var kvp in flattenedData.Where(kvp => kvp.Value is long))
                {
                    var pointParameters = new PointDataParameter(
                        ConfigSettingsState.ServerName,
                        shard,
                        room,
                        tick,
                        timestamp,
                        username,
                        kvp.Key.ToLower(),
                        kvp.Value);
                    var point = InfluxDBPointHelper.CreatePoint(pointParameters);
                    // Increment pending counter when adding a new point.
                    Interlocked.Increment(ref _pendingPointCount);
                    _channel?.Writer.TryWrite((bucket, point));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UploadData for bucket: {Bucket}", bucket);
            }
        }

        private static async Task WorkerLoop()
        {
            while (_isRunning)
            {
                if (_channel == null || _writeApi == null) continue;
                // Create a temporary dictionary to group points per bucket.
                var batchDict = new Dictionary<string, List<PointData>>();
                int batchCount = 0;

                // Wait for the first item in the batch.
                if (!await _channel.Reader.WaitToReadAsync())
                    continue;

                // Read items from the channel up to the batch size.
                while (_channel.Reader.TryRead(out var item))
                {
                    if (!batchDict.ContainsKey(item.bucket))
                        batchDict[item.bucket] = new List<PointData>();

                    batchDict[item.bucket].Add(item.point);
                    batchCount++;

                    // Decrement pending counter for each item read.
                    Interlocked.Decrement(ref _pendingPointCount);
                }

                // Process each bucket's batch.
                foreach (var kvp in batchDict)
                {
                    try
                    {
                        await _writeApi.WritePointsAsync(kvp.Value, bucket: kvp.Key, org: "screeps");
                        Interlocked.Add(ref _flushedPointCount, kvp.Value.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error writing batch to InfluxDB bucket {Bucket}. Re-enqueuing points.", kvp.Key);
                        // On failure, re-enqueue each point and update the pending counter.
                        foreach (var point in kvp.Value)
                        {
                            Interlocked.Increment(ref _pendingPointCount);
                            _channel.Writer.TryWrite((kvp.Key, point));
                        }
                    }
                }
            }
        }

        private static async Task LogStatusPeriodically()
        {
            while (_isRunning)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                var flushed = Interlocked.Exchange(ref _flushedPointCount, 0);
                var pending = Interlocked.Read(ref _pendingPointCount);
                _logger.Information("Flushed {Flushed} points in the last 10 seconds. Pending points: {Pending}", flushed, pending);
            }
        }

        public static void Stop()
        {
            _isRunning = false;
            _channel?.Writer.Complete();
            if (_workerTasks == null) return;
            Task.WaitAll(_workerTasks.ToArray());
            _logger.Information("Worker tasks stopped.");
        }
    }


    public static class InfluxDBClientState
    {
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.InfluxDB);
        public static async Task WriteScreepsRoomHistory(string shard, string room, long tick, long timestamp, ScreepsRoomHistoryDto screepsRoomHistory)
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

                if (screepsRoomHistory.Structures.Controller != null) InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_controller", screepsRoomHistory.Structures.Controller);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_mineral", screepsRoomHistory.Structures.Mineral);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_deposit", screepsRoomHistory.Structures.Deposit);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_wall", screepsRoomHistory.Structures.Wall);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_constructionsite", screepsRoomHistory.Structures.ConstructionSite);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_container", screepsRoomHistory.Structures.Container);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_extension", screepsRoomHistory.Structures.Extension);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_extractor", screepsRoomHistory.Structures.Extractor);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_factory", screepsRoomHistory.Structures.Factory);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_invadercore", screepsRoomHistory.Structures.InvaderCore);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_keeperlair", screepsRoomHistory.Structures.KeeperLair);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_lab", screepsRoomHistory.Structures.Lab);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_link", screepsRoomHistory.Structures.Link);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_observer", screepsRoomHistory.Structures.Observer);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_portal", screepsRoomHistory.Structures.Portal);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_powerbank", screepsRoomHistory.Structures.PowerBank);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_powerspawn", screepsRoomHistory.Structures.PowerSpawn);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_rampart", screepsRoomHistory.Structures.Rampart);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_road", screepsRoomHistory.Structures.Road);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_ruin", screepsRoomHistory.Structures.Ruin);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_source", screepsRoomHistory.Structures.Source);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_spawn", screepsRoomHistory.Structures.Spawn);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_storage", screepsRoomHistory.Structures.Storage);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_terminal", screepsRoomHistory.Structures.Terminal);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_tombstone", screepsRoomHistory.Structures.Tombstone);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_nuker", screepsRoomHistory.Structures.Nuker);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_structure_nuke", screepsRoomHistory.Structures.Nuke);

                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_creep_owned", screepsRoomHistory.Creeps.OwnedCreeps);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_creep_enemy", screepsRoomHistory.Creeps.EnemyCreeps);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_creep_other", screepsRoomHistory.Creeps.OtherCreeps);
                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_creep_power", screepsRoomHistory.Creeps.PowerCreeps);

                InfluxDBClientWriter.UploadData(shard, room, tick, timestamp, username, "history_groundresource", screepsRoomHistory.GroundResources);
            }
            catch (Exception e)
            {
                var message = string.Format("Error uploading {0}/{1}/{2}", shard, room, tick);
                _logger.Error(e, message);
            }
        }

        public static void WritePerformanceData(PerformanceClassDto PerformanceClassDto)
        {
            try
            {
                var point = PointData
                            .Measurement(ConfigSettingsState.ServerName)
                            .Tag("shard", PerformanceClassDto.Shard)
                            .Field("TicksBehind", PerformanceClassDto.TicksBehind)
                            .Field("TimeTakenMs", PerformanceClassDto.TimeTakenMs)
                            .Field("TotalRooms", PerformanceClassDto.TotalRooms)
                            .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

                InfluxDBClientWriter.AddPoint("history_performance", point);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error uploading performance data");
            }
        }
    }
}
