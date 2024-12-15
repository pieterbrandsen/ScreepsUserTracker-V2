using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserTrackerScreepsApi;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;

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

        public async Task UpdateRoomData(long tick)
        {
            await Task.Delay(500);
            var roomData = await ScreepsAPI.GetHistory(Shard, Name, tick);
            if (roomData != null)
            {
                var roomHistory = ScreepsRoomHistoryComputedHelper.Compute(roomData);

                RoomData = roomHistory;
                LastRoomData = roomHistory;
            }
            else
            {
                LastRoomData = null;
            }
        }
    }
}
