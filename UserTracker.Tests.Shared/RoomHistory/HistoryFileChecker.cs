using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Buffers;
using System.Threading.Tasks;
using UserTracker.Tests.Helper;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerStates.DBClients;

namespace UserTracker.Tests.RoomHistory
{
    public static class HistoryFileChecker
    {
        private static (long, List<string>) AssertHistory(ScreepsRoomHistory history, JToken jTokenTick, string filePath)
        {
            var seenProperties = new List<string>();
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
                    if (matchedKey != null)
                    {
                        if (historyChanges.TryGetValue(matchedKey, out var val))
                        {
                            var convertedVal = val != null ? val.ToString() : "null";
                            var convertedKV = kv.Value?.ToString() ?? "null";

                            if (!convertedKV.Equals(convertedVal))
                            {
                                throw new Exception($"Values do not match : {filePath}/{history.Tick} : {id}/{matchedKey} from {string.Join(",", variations)} : {convertedKV} vs {convertedVal}");
                            }
                            changesProcessed += 1;
                            seenProperties.Add(matchedKey);

                        }
                    }
                }
            }

            return (changesProcessed, seenProperties);
        }

        private static (long, List<string>) ProcessHistory(JObject roomData, string filePath)
        {
            var seenProperties = new List<string>();
            long changesProcessed = 0;
            var roomHistory = new ScreepsRoomHistory();
            roomHistory.HistoryChangesDictionary = new Dictionary<string, Dictionary<string, object?>>();
            var roomHistoryDTO = new ScreepsRoomHistoryDTO();

            roomData.TryGetValue("timestamp", out JToken? jTokenTime);
            if (jTokenTime != null) roomHistory.TimeStamp = jTokenTime.Value<long>();
            roomData.TryGetValue("base", out JToken? jTokenBase);
            if (jTokenBase != null) roomHistory.Base = jTokenBase.Value<long>();

            var room = "";
            roomData.TryGetValue("room", out JToken? jTokenRoom);
            if (jTokenRoom != null) room = jTokenRoom.Value<string>() ?? "";

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
                        seenProperties = seenProperties.Concat(vSeenProcessed).ToList();
                    }
                    roomHistoryDTO.Update(roomHistory);
                }
            }

            return (changesProcessed, seenProperties);
        }

        public static (long, List<string>) ParseFile(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var jsonReader = new JsonTextReader(reader);
            return ProcessHistory(JObject.Load(jsonReader), filePath);
        }
    }
}
