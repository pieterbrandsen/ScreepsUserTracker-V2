using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserTrackerScreepsApi;
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
            var roomData = await ScreepsAPI.GetHistory(Shard, Name, tick);
            if (roomData != null)
            {

            }
            else
            {
                LastRoomData = null;
            }
        } 
    }
}
