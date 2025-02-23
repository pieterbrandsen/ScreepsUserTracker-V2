using ahd.Graphite;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading.Channels;
using UserTrackerScreepsApi;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerShared.States;


namespace UserTrackerStates.DBClients
{
    public static class GraphiteDBClientWriter
    {
        private static readonly JsonSerializer _serializer = JsonSerializer.CreateDefault();
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.GraphiteDB);
        private static bool _isInitialized = false;
        private static CarbonClient _client = null;
        private static Channel<Datapoint> _channel;

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

            _logger.Information("Initializing GraphiteDB client...");

            _client = new CarbonClient(ConfigSettingsState.GraphiteDbHost);

            try
            {
                _isInitialized = true;
                _logger.Information("GraphiteDB client connected");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error initializing GraphiteDB client.");
                throw;
            }


            // Create an unbounded channel for point data.
            _channel = Channel.CreateUnbounded<Datapoint>(new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            });

            // Start a background task to log status.
            Task.Run(WorkerLoop);
            Task.Run(LogStatusPeriodically);

            _logger.Information("Worker tasks started.");
        }

        public static void UploadData(string prefix, object obj, DateTime timestamp)
        {
            try
            {
                var flattenedData = new Dictionary<string, object>();
                var writer = new JTokenWriter();
                _serializer.Serialize(writer, obj);
                InfluxDBPointHelper.FlattenJson(writer.Token!, new StringBuilder(), flattenedData);

                foreach (var kvp in flattenedData)
                {
                    if (kvp.Value is long)
                    {
                        // Increment pending counter when adding a new point.
                        Interlocked.Increment(ref _pendingPointCount);
                        _channel.Writer.TryWrite(new Datapoint($"{prefix}{kvp.Key}", Convert.ToInt64(kvp.Value), timestamp));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UploadData");
            }
        }

        public static void UploadData(string prefix, string shard, string room, long timestamp, string username, object obj)
        {
            try
            {
                var flattenedData = new Dictionary<string, object>();
                var writer = new JTokenWriter();
                _serializer.Serialize(writer, obj);
                InfluxDBPointHelper.FlattenJson(writer.Token!, new StringBuilder(), flattenedData);

                foreach (var kvp in flattenedData)
                {
                    if (kvp.Value is long)
                    {
                        // Increment pending counter when adding a new point.
                        Interlocked.Increment(ref _pendingPointCount);
                        _channel.Writer.TryWrite(new($"{prefix}{shard}.{room}.{kvp.Key}", Convert.ToInt64(kvp.Value), DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UploadData");
            }
        }

        private static async Task WorkerLoop()
        {
            // Create a temporary dictionary to group points per bucket.
            var datapointList = new List<Datapoint>();
            int batchCount = 0;

            // Wait for the first item in the batch.
            if (!await _channel.Reader.WaitToReadAsync())
                return;

            // Read items from the channel up to the batch size.
            while (_channel.Reader.TryRead(out var item))
            {
                datapointList.Add(item);
                batchCount++;

                // Decrement pending counter for each item read.
                Interlocked.Decrement(ref _pendingPointCount);
            }

            // Process each bucket's batch.
            Interlocked.Add(ref _flushedPointCount, datapointList.Count);
            try
            {
                await _client.SendAsync(datapointList);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error writing batch to Graphite. Re-enqueuing points.");
                // On failure, re-enqueue each point and update the pending counter.
                Interlocked.Increment(ref _pendingPointCount);
                foreach (var kvp in datapointList)
                {
                    _channel.Writer.TryWrite(kvp);
                }
            }
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
                var username = "none";
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

                GraphiteDBClientWriter.UploadData($"history.{ConfigSettingsState.ServerName}.", shard, room, timestamp, username, screepsRoomHistory);
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
                GraphiteDBClientWriter.UploadData($"historyPerformance.{ConfigSettingsState.ServerName}.", performanceClassDTO, DateTime.Now);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error uploading performance data");
            }
        }

    }
}
