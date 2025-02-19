using Newtonsoft.Json.Linq;
using UserTrackerScreepsApi;
using UserTrackerShared.Models;
using UserTrackerStates;

namespace UserTrackerShared.Helpers
{
    public static class RoomDataHelper
    {
        public static async Task<bool> GetAndHandleRoomData(string shard, string name, long tick)
        {
            var roomData = await ScreepsAPI.GetHistory(shard, name, tick);
            if (roomData == null) return false;

            var roomHistory = new ScreepsRoomHistory();
            var roomHistoryDTO = new ScreepsRoomHistoryDTO();
            roomData.TryGetValue("timestamp", out JToken? jTokenTime);
            if (jTokenTime != null) roomHistory.TimeStamp = jTokenTime.Value<long>();

            roomData.TryGetValue("base", out JToken? jTokenBase);
            if (jTokenBase != null) roomHistory.Base = jTokenBase.Value<long>();

            if (roomData.TryGetValue("ticks", out JToken? jTokenTicks) && jTokenTicks is JObject jObjectTicks)
            {
                for (int i = 0; i < 100; i++)
                {
                    long tickNumber = roomHistory.Base + i;
                    roomHistory.Tick = tickNumber;

                    if (jObjectTicks.TryGetValue(tickNumber.ToString(), out JToken? tickObject) && tickObject != null)
                    {
                        try
                        {
                            roomHistory = ScreespRoomHistoryHelper.ComputeTick(tickObject, roomHistory);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, $"Error processing tick {tickNumber} for room {name}");
                        }
                        
                        if (ConfigSettingsState.InfluxDbEnabled)
                        {
                            roomHistoryDTO.Update(roomHistory);
                            await InfluxDBClientState.WriteScreepsRoomHistory(shard, name, roomHistory.Tick, roomHistory.TimeStamp, roomHistoryDTO);
                        }
                    }
                }
            }

            if (ConfigSettingsState.WriteHistoryFiles) FileWriterManager.GenerateHistoryFile(roomData);
            return true;
        }
    }
}
