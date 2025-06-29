﻿using Newtonsoft.Json.Linq;
using UserTrackerShared.Models;

namespace UserTrackerShared.Helpers.Tests
{
    public static class AssertHistoryHelper
    {
        public static (long, Dictionary<string, long>) AssertHistory(ScreepsRoomHistory history, JToken jTokenTick, string? filePath = "notSupplied")
        {
            var seenProperties = new Dictionary<string, long>();
            long changesProcessed = 0;
            var ids = history.TypeMap.Keys.ToArray();
            for (int y = 0; y < ids.Length; y++)
            {
                var id = ids[y];
                if (history.HistoryChangesDictionary == null) throw new Exception("history.HistoryChangesDictionary was null");

                history.HistoryChangesDictionary.TryGetValue(id, out var historyChanges);
                if (historyChanges == null) throw new Exception("historyChanges was null");
                var originalChanges = GetObjectChangesInTick.GetById(jTokenTick, id);

                var keyVariations = new Dictionary<string, HashSet<string>>(originalChanges.Count);

                // First loop to create the key variations
                foreach (var key in originalChanges.Select(kv=>kv.Key))
                {
                    var baseKey = HistoryDictionaryHelper.MapPropertyIfFound(key);
                    var variations = new HashSet<string>
                        {
                            baseKey,
                            HistoryDictionaryHelper.CapitalizeLetters(baseKey),
                            HistoryDictionaryHelper.CapitalizeLettersExceptLast(baseKey)
                        };
                    keyVariations[key] = variations;
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
    }
}
