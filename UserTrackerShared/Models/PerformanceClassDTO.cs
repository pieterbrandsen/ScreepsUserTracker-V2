using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTrackerShared.Models
{
    public class PerformanceClassDTO
    {
        public string Shard { get; set; }
        public long TicksBehind { get; set; }
        public long TimeTakenMs { get; set; }
        public int TotalRooms { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount => TotalRooms - SuccessCount;
    }
}
