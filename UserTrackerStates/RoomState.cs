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
                var roomHistories = ScreepsRoomHistoryComputedHelper.Compute(roomData);

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
