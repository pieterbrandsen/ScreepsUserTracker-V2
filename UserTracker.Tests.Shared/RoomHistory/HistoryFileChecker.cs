using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Buffers;
using UserTracker.Tests.Helper;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;

namespace UserTracker.Tests.RoomHistory
{
    public static class HistoryFileChecker
    {
        private static (long, Dictionary<string, long>) AssertHistory(ScreepsRoomHistory history, JToken jTokenTick, string filePath)
        {
            var seenProperties = new Dictionary<string, long>();
            long changesProcessed = 0;
            var ids = history.TypeMap.Keys.ToArray();
            for (int y = 0; y < ids.Length; y++)
            {
                var id = ids[y];
                var obj = GetObjectFromHistory.GetById(history, id) ?? throw new Exception("obj was null");
                if (history.HistoryChangesDictionary == null) throw new Exception("history.HistoryChangesDictionary was null");

                history.HistoryChangesDictionary.TryGetValue(id, out var historyChanges);
                if (historyChanges == null) throw new Exception("historyChanges was null");
                var originalChanges = GetObjectChangesInTick.GetById(jTokenTick, id);

                var keyVariations = new Dictionary<string, HashSet<string>>(originalChanges.Count);

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
                    if (matchedKey != null && historyChanges.TryGetValue(matchedKey, out var val))
                    {
                        var convertedVal = val != null ? val.ToString() : "null";
                        var convertedKV = kv.Value?.ToString() ?? "null";

                        if (!convertedKV.Equals(convertedVal))
                        {
                            throw new Exception($"Values do not match : {filePath}/{history.Tick} : {id}/{matchedKey} from {string.Join(",", variations)} : {convertedKV} vs {convertedVal}");
                        }
                        changesProcessed += 1;

                        seenProperties.TryGetValue(matchedKey, out long count);
                        seenProperties[matchedKey] = count + 1;

                    }
                }
            }

            return (changesProcessed, seenProperties);
        }

        private static (long, Dictionary<string, long>) ProcessHistory(JObject roomData, string filePath)
        {
            var seenProperties = new Dictionary<string, long>();
            long changesProcessed = 0;
            var roomHistory = new ScreepsRoomHistory();
            roomHistory.HistoryChangesDictionary = new Dictionary<string, Dictionary<string, object?>>();
            var roomHistoryDto = new ScreepsRoomHistoryDto();

            roomData.TryGetValue("timestamp", out JToken? jTokenTime);
            if (jTokenTime != null) roomHistory.TimeStamp = jTokenTime.Value<long>();
            roomData.TryGetValue("base", out JToken? jTokenBase);
            if (jTokenBase != null) roomHistory.Base = jTokenBase.Value<long>();

            if (roomData.TryGetValue("ticks", out JToken? jTokenTicks) && jTokenTicks is JObject jObjectTicks)
            {
                for (int i = 0; i < ConfigSettingsState.TicksInFile; i++)
                {
                    long tickNumber = roomHistory.Base + i;
                    roomHistory.Tick = tickNumber;

                    if (jObjectTicks.TryGetValue(tickNumber.ToString(), out JToken? tickObject) && tickObject != null)
                    {
                        roomHistory = ScreepsRoomHistoryHelper.ComputeTick(tickObject, roomHistory);
                        var (vChangesProcessed, vSeenProcessed) = AssertHistory(roomHistory, tickObject, filePath);
                        changesProcessed += vChangesProcessed;
                        foreach (var kv in vSeenProcessed)
                        {
                            seenProperties.TryGetValue(kv.Key, out long count);
                            seenProperties[kv.Key] = count + kv.Value;
                        }
                    }
                    roomHistoryDto.Update(roomHistory);
                }
            }

            return (changesProcessed, seenProperties);
        }

        public static (long, Dictionary<string, long>) ParseFile(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var jsonReader = new JsonTextReader(reader);
            return ProcessHistory(JObject.Load(jsonReader), filePath);
        }
    }
}
