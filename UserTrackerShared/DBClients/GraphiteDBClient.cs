using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Sockets;
using System.Text;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerShared.Models.ScreepsAPI;
using UserTrackerShared.States;
using UserTrackerShared.Utilities;

namespace UserTrackerShared.DBClients
{
    public class GraphiteBatchClient : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private readonly int _batchSize;
        private readonly List<string> _metricsBuffer;

        public GraphiteBatchClient(string host, int port, int batchSize = 1000)
        {
            _host = host;
            _port = port;
            _batchSize = batchSize;
            _metricsBuffer = new List<string>();
        }

        /// <summary>
        /// Adds a metric to the buffer. If the buffer reaches the batch size, it flushes automatically.
        /// </summary>
        /// <param name="metricPath">Metric path (dot-separated string).</param>
        /// <param name="value">Metric value.</param>
        /// <param name="timestamp">Unix timestamp.</param>
        public void AddMetric(string metricPath, double value, long timestamp)
        {
            string metricLine = $"{metricPath} {value} {timestamp / 1000}\n";
            _metricsBuffer.Add(metricLine);

            if (_metricsBuffer.Count >= _batchSize)
            {
                Flush();
            }
        }

        /// <summary>
        /// Sends all buffered metrics in one batch to the configured host/port.
        /// </summary>
        public void Flush()
        {
            if (_metricsBuffer.Count == 0)
                return;

            // Combine all metrics into one payload string.
            string payload = string.Join("", _metricsBuffer);

            try
            {
                using (TcpClient client = new TcpClient(_host, _port))
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] data = Encoding.ASCII.GetBytes(payload);
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions as needed (logging, retry logic, etc.)
                Console.WriteLine($"Error sending metrics: {ex.Message}");
            }
            finally
            {
                _metricsBuffer.Clear();
            }
        }

        /// <summary>
        /// Dispose method to flush any remaining metrics.
        /// </summary>
        public void Dispose()
        {
            Flush();
            GC.SuppressFinalize(this);
        }
    }

    public static class GraphiteDBClientWriter
    {
        private static readonly JsonSerializer _serializer = JsonSerializer.CreateDefault();
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.GraphiteDB);
        private static bool _isInitialized = false;
        private static GraphiteBatchClient _client = new GraphiteBatchClient(ConfigSettingsState.GraphiteDbHost, ConfigSettingsState.GraphiteDbPort);

        // Counters for statistics.
        private static long _flushedPointCount = 0;

        public static void Init()
        {
            if (_isInitialized)
            {
                _logger.Debug("GraphiteDB client already initialized, skipping initialization.");
                return;
            }

            _logger.Information("Initializing GraphiteDB client...");

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

            // Start a background task to log status.
            // Task.Run(LogStatusPeriodically);

            _logger.Information("Worker tasks started.");
        }

        public static void UploadData(string prefix, object obj, long timestamp)
        {
            try
            {
                var flattenedData = new Dictionary<string, object?>();
                var writer = new JTokenWriter();
                _serializer.Serialize(writer, obj);
                JsonHelper.FlattenJson(writer.Token!, new StringBuilder(), flattenedData);

                foreach (var kvp in flattenedData.Where(kvp => kvp.Value is long || kvp.Value is int || kvp.Value is double || kvp.Value is decimal))
                {
                    _client.AddMetric($"{prefix}{kvp.Key}", Convert.ToInt64(kvp.Value), timestamp);
                }
                Interlocked.Add(ref _flushedPointCount, flattenedData.Count);
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
                var flattenedData = new Dictionary<string, object?>();
                var writer = new JTokenWriter();
                _serializer.Serialize(writer, obj);
                JsonHelper.FlattenJson(writer.Token!, new StringBuilder(), flattenedData);

                foreach (var kvp in flattenedData.Where(kvp => kvp.Value is long || kvp.Value is int || kvp.Value is double || kvp.Value is decimal))
                {
                    _client.AddMetric($"{prefix}{shard}.{username}.{room}.{kvp.Key}", Convert.ToInt64(kvp.Value), timestamp);
                }
                Interlocked.Add(ref _flushedPointCount, flattenedData.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UploadData");
            }
        }

        // private static async Task LogStatusPeriodically()
        // {
        //     while (true)
        //     {
        //         await Task.Delay(TimeSpan.FromSeconds(10));
        //         var flushed = Interlocked.Exchange(ref _flushedPointCount, 0);
        //         _logger.Information("Flushed {Flushed} points in the last 10 seconds", flushed);
        //     }
        // }
    }

    public static class GraphiteDBClientState
    {
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.GraphiteDB);

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
                    var apiUser = await ScreepsAPI.GetUser(userId);
                    if (apiUser != null)
                    {
                        GameState.Users.AddOrUpdate(userId, apiUser, (key, oldValue) => apiUser);
                    }
                }

                GraphiteDBClientWriter.UploadData($"history.{ConfigSettingsState.ServerName}.data.", shard, room, timestamp, username, screepsRoomHistory);
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
                GraphiteDBClientWriter.UploadData($"history.{ConfigSettingsState.ServerName}.performance.{PerformanceClassDto.Shard}.", PerformanceClassDto, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error uploading performance data");
            }
        }

        public static void WriteHistoricalLeaderboardData(SeasonListItem seasonItem)
        {
            try
            {
                string[] parts = seasonItem.Season.Split('-');
                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);

                DateTime dateTime = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                var timestamp = ((DateTimeOffset)dateTime).ToUnixTimeMilliseconds();
                GraphiteDBClientWriter.UploadData($"history.{ConfigSettingsState.ServerName}.leaderboard.{seasonItem.Type}.{seasonItem.UserName}.", seasonItem, timestamp);
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
                GraphiteDBClientWriter.UploadData($"history.{ConfigSettingsState.ServerName}.users.{user.Username}.", user, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
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
                GraphiteDBClientWriter.UploadData($"history.{ConfigSettingsState.ServerName}.adminutils.", data, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error uploading performance data");
            }
        }
    }
}
