using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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

            roomData.TryGetValue("ticks", out JToken? jTokenTicks);
            if (jTokenTicks != null)
            {
                var jTokenTicksValues = jTokenTicks.Values<JToken>();
                for (int i = 0; i < jTokenTicksValues.Count(); i++)
                {
                    long tickNumber = roomHistory.Base + i;
                    roomHistory.Tick = tickNumber;
                    var tickObject = jTokenTicksValues.FirstOrDefault(t => t.Path.EndsWith($".{tickNumber}"));
                    if (tickObject == null) continue;
                    try
                    {
                        roomHistory = ScreespRoomHistoryHelper.ComputeTick(tickObject, roomHistory);
                    }
                    catch (Exception e)
                    {
                        //throw;
                    }
                    if (ConfigSettingsState.InfluxDbEnabled)
                    {
                        roomHistoryDTO.Update(roomHistory);
                        await InfluxDBClientState.WriteScreepsRoomHistory(shard, name, roomHistory.Tick, roomHistory.TimeStamp, roomHistoryDTO);
                    }
                }
            }

            if (ConfigSettingsState.WriteHistoryFiles) FileWriterManager.GenerateHistoryFile(roomData);
            return true;
        }
    }
}
