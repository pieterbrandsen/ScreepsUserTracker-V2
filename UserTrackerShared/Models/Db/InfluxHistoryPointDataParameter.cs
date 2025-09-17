using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTrackerShared.Models.Db
{
    public class InfluxHistoryPointDataParameter : BaseHistoryPointDataParameter
    {
        public InfluxHistoryPointDataParameter(string measurement, string shard, string room, long tick, long timestamp, string username, string field, double? value) : base(shard, room, tick, timestamp, username, measurement, field, value)
        {
        }
    }
}
