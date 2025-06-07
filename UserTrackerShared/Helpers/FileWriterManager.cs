using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Timers;
using Timer = System.Timers.Timer;

namespace UserTrackerShared.Helpers
{
    public static class FileWriterManager
    {
        private static readonly string HistoryDirectoryPath = @$"{ConfigSettingsState.ObjectsFolder}\History";
        private static readonly string KeysDirectoryPath = @$"{ConfigSettingsState.ObjectsFolder}\Keys";

        private static readonly ConcurrentDictionary<string, JObject> HistoryCache = new();
        private static readonly ConcurrentDictionary<string, JObject> KeyCache = new();

        private static Timer? _backgroundFlushTimer;
        public static bool StopFlushing;
        public static bool IsFlushing;

        static FileWriterManager()
        {
            Directory.CreateDirectory(HistoryDirectoryPath);
            Directory.CreateDirectory(KeysDirectoryPath);

            _backgroundFlushTimer = new Timer(10 * 1000);
            _backgroundFlushTimer.Elapsed += OnBackgroundFlushTimer;
            _backgroundFlushTimer.AutoReset = true;
            _backgroundFlushTimer.Enabled = true;
        }

        public static void GenerateHistoryFile(JObject roomData)
        {
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

        private static async void OnBackgroundFlushTimer(Object? source, ElapsedEventArgs? e)
        {
            if (IsFlushing || StopFlushing) return;
            IsFlushing = true;
            try
            {
                SemaphoreSlim semaphore = new SemaphoreSlim(10000); // Limit concurrent writes to avoid overloading the disk

                IEnumerable<string> keyKeys;
                lock (KeyCache)
                {
                    keyKeys = KeyCache.Keys.ToArray();
                }

                var keysTasks = new List<Task>();
                foreach (var key in keyKeys)
                {
                    await semaphore.WaitAsync(); // Throttle writes
                    keysTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            if (KeyCache.TryRemove(key, out JObject obj))
                            {
                                if (obj == null) return;
                                string filePath = Path.Combine(KeysDirectoryPath, $"{key}.json");

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
                    }));
                }
                await Task.WhenAll(keysTasks);


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
                    historyTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            if (HistoryCache.TryRemove(key, out JObject obj))
                            {
                                if (obj == null) return;
                                string[] parts = key.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

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
                    }));
                }
                await Task.WhenAll(historyTasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in BackgroundFlush: {ex.Message}");
            }
            IsFlushing = false;
        }
    }
}
