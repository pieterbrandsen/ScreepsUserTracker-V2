using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UserTrackerShared.Models.TimeScaleDB
{
    public class TimeScaleScreepsRoomHistoryDto : ScreepsRoomHistoryDto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Server { get; set; } = null!;
        [Required]
        public string Shard { get; set; } = null!;
        [Required]
        public string Room { get; set; } = null!;
        [Required]
        public string Username { get; set; } = null!;

        [Column(TypeName = "jsonb")]
        public string GroundResourcesJson { get; set; } = "{}";

        [NotMapped]
        public new Dictionary<string, decimal> GroundResources
        {
            get => JsonSerializer
                     .Deserialize<Dictionary<string, decimal>>(GroundResourcesJson)
                   ?? new();
            set => GroundResourcesJson = JsonSerializer.Serialize(value);
        }

        [Column(TypeName = "jsonb")]
        public string CreepsJson { get; set; } = "{}";

        [NotMapped]
        public new CreepsDto Creeps
        {
            get => JsonSerializer
                     .Deserialize<CreepsDto>(CreepsJson)
                   ?? new();
            set => CreepsJson = JsonSerializer.Serialize(value);
        }

        [Column(TypeName = "jsonb")]
        public string StructuresJson { get; set; } = "{}";

        [NotMapped]
        public new StructuresDto Structures
        {
            get => JsonSerializer
                     .Deserialize<StructuresDto>(StructuresJson)
                   ?? new();
            set => StructuresJson = JsonSerializer.Serialize(value);
        }
    }
}
