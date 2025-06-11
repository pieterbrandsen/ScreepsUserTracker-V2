using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTrackerShared.Models
{
    public class PerformanceClassDto
    {
        public required string Shard { get; set; }
        public long TicksBehind { get; set; }
        public long TimeTakenMs { get; set; }
        public int TotalRooms { get; set; }
        public required ConcurrentDictionary<int, int> ResultCodes { get; set; }
    }
}
