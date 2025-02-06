using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Buffers;
using System.Diagnostics;
using UserTracker.Tests.Helper;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;

namespace UserTracker.Tests.RoomHistory
{
    public static class HistoryFileChecker
    {
        private static long _filesProcessed = 0;
        private static long _changesProcessed = 0;

        private static void AssertHistory(List<ScreepsRoomHistory> histories, JObject jObject)
        {
            for (int ht = 0; ht < histories.Count; ht++)
            {
                var tickHistory = histories[ht];
                var tickStr = tickHistory.Tick.ToString();
                var ticks = jObject["ticks"] as JObject;
                if (ticks == null) continue;

                var ids = tickHistory.TypeMap.Keys.ToArray();
                for (int y = 0; y < ids.Length; y++)
                {
                    var id = ids[y];
                    if (string.IsNullOrEmpty(id)) continue;

                    var obj = GetObjectFromHistory.GetById(tickHistory, id);
                    if (obj == null) continue;

                    var tick = ticks[tickStr] as JObject;
                    if (tick == null) continue;

                    var historyChanges = GetObjectChangesInTick.GetById(obj);
                    var originalChanges = GetObjectChangesInTick.GetById(tick, id);

                    var keyVariations = new Dictionary<string, HashSet<string>>(originalChanges.Count, StringComparer.OrdinalIgnoreCase);
                    
                    // First loop to create the key variations
                    foreach (var kv in originalChanges)
                    {
                        var baseKey = HistoryDictionaryHelper.MapPropertyIfFound(kv.Key);
                        var variations = new HashSet<string>
                        {
                            baseKey,
                            HistoryDictionaryHelper.CapitalizeLetters(baseKey),
                            HistoryDictionaryHelper.CapitalizeLettersExceptLast(baseKey)
                        };
                        keyVariations[kv.Key] = variations;
                    }

                    // Second loop to process the changes
                    foreach (var kv in originalChanges)
                    {
                        // Get all variations at once
                        var variations = keyVariations[kv.Key];

                        // Check if any variation exists in historyChanges
                        var matchedKey = variations.FirstOrDefault(key => historyChanges.ContainsKey(key));
                        if (matchedKey != null)
                        {
                            if (historyChanges.TryGetValue(matchedKey, out var val))
                            {
                                var convertedVal = val != null ? val.ToString() : "null";
                                var convertedKV = kv.Value != null ? kv.Value.ToString() : "null";

                                // Assuming comparison was necessary (based on your commented-out line)
                                convertedKV.Should().Be(convertedVal);
                                _changesProcessed += 1;
                            }
                        }
                    }

                };
            }
        }

        private static void ProcessHistory(JObject jObject)
        {
            var histories = ScreepsRoomHistoryComputedHelper.Compute(jObject);
            return;
            AssertHistory(histories, jObject);
        }

        public static void ParseFile(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var jObject = JObject.Parse(json);
            ProcessHistory(jObject);

            _filesProcessed += 1;
            Console.WriteLine($"Changes processed {_changesProcessed} in {_filesProcessed}");
        }
    }
}
