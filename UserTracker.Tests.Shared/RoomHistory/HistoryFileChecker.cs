using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserTrackerShared.Helpers;
using UserTrackerShared.Helpers.Tests;
using UserTrackerShared.Models;
using UserTrackerShared.States;

namespace UserTracker.Tests.RoomHistory
{
    public static class HistoryFileChecker
    {
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
                        var (vChangesProcessed, vSeenProcessed) = AssertHistoryHelper.AssertHistory(roomHistory, tickObject, filePath);
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
