using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Buffers;
using UserTracker.Tests.Helper;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;

namespace UserTracker.Tests.RoomHistory
{
    public static class HistoryFileChecker
    {
        private static long _filesProcessed = 0;
        private static long _changesProcessed = 0;

        private static void AssertHistory(ScreepsRoomHistory history, JToken jTokenTick)
        {
            var ids = history.TypeMap.Keys.ToArray();
            for (int y = 0; y < ids.Length; y++)
            {
                var id = ids[y];
                if (string.IsNullOrEmpty(id)) continue;

                var obj = GetObjectFromHistory.GetById(history, id);
                if (obj == null) continue;

                var tick = jTokenTick as JObject;

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
            }
        }

        private static void ProcessHistory(JObject roomData)
        {
            var roomHistory = new ScreepsRoomHistory();
            roomData.TryGetValue("timestamp", out JToken? jTokenTime);
            if (jTokenTime != null) roomHistory.TimeStamp = jTokenTime.Value<long>();

            roomData.TryGetValue("base", out JToken? jTokenBase);
            if (jTokenBase != null) roomHistory.Base = jTokenBase.Value<long>();

            roomData.TryGetValue("ticks", out JToken? jTokenTicks);
            if (jTokenTicks != null)
            {
                var jTokenTicksValues = jTokenTicks.Values<JToken>();
                for (int i = 0; i < 100; i++)
                {
                    long tickNumber = roomHistory.Base + i;
                    roomHistory.Tick = tickNumber;
                    var tickObject = jTokenTicksValues.FirstOrDefault(t => t.Path.EndsWith($".{tickNumber}"));
                    if (tickObject == null) continue;
                    roomHistory = ScreepsRoomHistoryComputedHelper.ComputeTick(tickObject, roomHistory);


                    AssertHistory(roomHistory, (jTokenTicks as JObject)[roomHistory.Tick.ToString()]);
                }
            }
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
