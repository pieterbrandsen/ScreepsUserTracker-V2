using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UserTrackerShared.Models.ScreepsAPI;

namespace UserTrackerShared.Models.TimeScaleDB
{
    public class TimeScaleScreepsUser : ScreepsUser
    {
        [Key]
        public new int Id { get; set; }
        public string Server { get; set; } = null!;
        public DateTime Time { get; set; }
        [Column(TypeName = "jsonb")]
        public string BadgeJson { get; set; } = "{}";

        [NotMapped]
        public new MapStatUserBadge Badge
        {
            get => JsonSerializer
                     .Deserialize<MapStatUserBadge>(BadgeJson)
                   ?? new ();
            set => BadgeJson = JsonSerializer.Serialize(value);
        }
    }
}
