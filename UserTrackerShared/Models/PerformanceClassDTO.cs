using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public ConcurrentDictionary<int, int> ResultCodes { get; set; } = new ConcurrentDictionary<int, int>();
    }
}
