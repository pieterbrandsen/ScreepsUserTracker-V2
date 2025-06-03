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
        private static long AssertHistory(ScreepsRoomHistory history, JToken jTokenTick, string filePath)
        {
            var changesProcessed = 0;
            var ids = history.TypeMap.Keys.ToArray();
            for (int y = 0; y < ids.Length; y++)
            {
                var id = ids[y];
                if (string.IsNullOrEmpty(id)) continue;

                var obj = GetObjectFromHistory.GetById(history, id);
                if (obj == null) continue;

                history.PropertiesListDictionary.TryGetValue(id, out var historyChanges);
                if (historyChanges == null) continue;
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
                            var convertedKV = kv.Value != null ? kv.Value.ToString() : "null";

                            if (!convertedKV.Equals(convertedVal))
                            {
                                throw new Exception($"Values do not match : {filePath}/{history.Tick} : {id}/{matchedKey} from {string.Join(",", variations)} : {convertedKV} vs {convertedVal}");
                            }
                            changesProcessed += 1;
                        }
                    }
                }
            }

            return changesProcessed;
        }

        private static long ProcessHistory(JObject roomData, string filePath)
        {
            long changesProcessed = 0;
            var roomHistory = new ScreepsRoomHistory();
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
                        changesProcessed += AssertHistory(roomHistory, tickObject, filePath);
                    }
                    roomHistoryDTO.Update(roomHistory);
                }
            }

            return changesProcessed;
        }

        public static long ParseFile(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var jsonReader = new JsonTextReader(reader);
            return ProcessHistory(JObject.Load(jsonReader), filePath);
        }
    }
}
