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
        public ScreepsRoomHistory? RoomData = null;
        public ScreepsRoomHistory? LastRoomData = null;

        public RoomState(string name, string shard)
        {
            Name = name;
            Shard = shard;
        }

        public async Task UpdateRoomData(long tick, ProxyState proxy)
        {
            JObject? roomData = null;
            if (proxy == null)
            {
                roomData = await ScreepsAPI.GetHistory(Shard, Name, tick);
            }
            else
            {
                roomData = await proxy.GetHistory(Shard, Name, tick);
            }
            if (roomData != null)
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
                    }
                }

                //RoomData = roomHistory;
                //LastRoomData = roomHistory;
            }
            else
            {
                LastRoomData = null;
            }
        }
    }
}
