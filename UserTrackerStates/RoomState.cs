using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserTrackerScreepsApi;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerShared.States;

namespace UserTrackerStates
{
    public class RoomState
    {
        public string Name;
        public string Shard;

        private JObject? _roomData = null;

        public RoomState(string name, string shard)
        {
            Name = name;
            Shard = shard;
        }

        public async Task<bool> GetRoomData(long tick)
        {
            _roomData = await ScreepsAPI.GetHistory(Shard, Name, tick);
            return _roomData != null;
        }

        public bool HandleRoomData()
        {
            if (_roomData == null) return false;

            var roomHistory = new ScreepsRoomHistory();
            _roomData.TryGetValue("timestamp", out JToken? jTokenTime);
            if (jTokenTime != null) roomHistory.TimeStamp = jTokenTime.Value<long>();

            _roomData.TryGetValue("base", out JToken? jTokenBase);
            if (jTokenBase != null) roomHistory.Base = jTokenBase.Value<long>();

            _roomData.TryGetValue("ticks", out JToken? jTokenTicks);
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
                        roomHistory = ScreepsRoomHistoryComputedHelper.ComputeTick(tickObject, roomHistory);
                    }
                    catch (Exception e)
                    {
                        //throw;
                    }
                    if (ConfigSettingsState.InfluxDbEnabled) {
                        var roomHistoryDTO = new ScreepsRoomHistoryDTO(roomHistory);
                        InfluxDBClientState.WriteScreepsRoomHistory(Shard, Name, roomHistory.Tick, roomHistory.TimeStamp, roomHistoryDTO);
                    }
                }
            }

            if (ConfigSettingsState.WriteHistoryFiles) FileWriterManager.GenerateHistoryFile(_roomData);
            return true;
        }
    }
}
