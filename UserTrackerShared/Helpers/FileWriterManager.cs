using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Timers;
using UserTrackerShared.States;
using UserTrackerShared.Utilities;
using Timer = System.Timers.Timer;

namespace UserTrackerShared.Helpers
{
    public static class FileWriterManager
    {
        private static readonly string HistoryDirectoryPath = @$"{ConfigSettingsState.ObjectsFolder}\History";
        private static readonly string KeysDirectoryPath = @$"{ConfigSettingsState.ObjectsFolder}\Keys";

        private static readonly ConcurrentDictionary<string, JObject> HistoryCache = new();

        private static bool _isFlushing;
        private static bool _isInitialized;
        public static void EnsureInitialized()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            Init();
        }

        static void Init()
        {
            Directory.CreateDirectory(HistoryDirectoryPath);
            Directory.CreateDirectory(KeysDirectoryPath);

            var backgroundFlushTimer = new Timer(10 * 1000);
            backgroundFlushTimer.Elapsed += OnBackgroundFlushTimer;
            backgroundFlushTimer.AutoReset = true;
            backgroundFlushTimer.Enabled = true;
        }

        public static void GenerateHistoryFile(JObject roomData)
        {
            EnsureInitialized();

            var room = "";
            roomData.TryGetValue("room", out JToken? jTokenRoom);
            if (jTokenRoom != null) room = jTokenRoom.Value<string>();

            long baseTick = 0;
            roomData.TryGetValue("base", out JToken? jTokenBase);
            if (jTokenBase != null) baseTick = jTokenBase.Value<long>();

            string filePath = Path.Combine(HistoryDirectoryPath, $"{baseTick}/{room}.json");
            if (File.Exists(filePath)) return;
            HistoryCache.AddOrUpdate($"{baseTick}/{room}", _ => roomData, (_, existing) =>
            {
                return existing;
            });
        }

        private static async Task WriteHistoryFile(string key, SemaphoreSlim semaphore)
        {
            try
            {
                if (HistoryCache.TryRemove(key, out JObject? obj))
                {
                    if (obj == null) return;
                    var charArr = new char[] { '/', '\\' };
                    string[] parts = key.Split(charArr, StringSplitOptions.RemoveEmptyEntries);

                    string tickDir = Path.Combine(HistoryDirectoryPath, parts[0]);
                    if (!Directory.Exists(tickDir))
                    {
                        Directory.CreateDirectory(tickDir);
                    }
                    string filePath = Path.Combine(HistoryDirectoryPath, $"{key}.json");

                    var json = JsonConvert.SerializeObject(obj, Formatting.Indented);

                    try
                    {
                        await File.WriteAllTextAsync(filePath, json);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }
            }
            catch (Exception ex)
            {
                Screen.AddLog($"Error writing file for key {key}: {ex.Message}");
            }
        }

        private static async void OnBackgroundFlushTimer(Object? source, ElapsedEventArgs? e)
        {
            if (_isFlushing) return;
            _isFlushing = true;
            try
            {
                SemaphoreSlim semaphore = new SemaphoreSlim(10000); // Limit concurrent writes to avoid overloading the disk

                IEnumerable<string> historyKeys;
                lock (HistoryCache)
                {
                    // Extract keys upfront to minimize lock time
                    historyKeys = HistoryCache.Keys.ToArray();
                }

                var historyTasks = new List<Task>();
                foreach (var key in historyKeys)
                {
                    await semaphore.WaitAsync(); // Throttle writes
                    historyTasks.Add(WriteHistoryFile(key, semaphore));
                }
                await Task.WhenAll(historyTasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in BackgroundFlush: {ex.Message}");
            }
            _isFlushing = false;
        }
    }
}
