using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UserTrackerShared.Models
{
    public class TimeScalePerformanceClassDto : PerformanceClassDto
    {
        [Key]
        public int Id { get; set; }
        public DateTime Time { get; set; }
        [Required]
        public string Server { get; set; } = null!;
        [Column(TypeName = "jsonb")]
        public string ResultCodesJson { get; set; } = "{}";

        [NotMapped]
        public new ConcurrentDictionary<int, int> ResultCodes
        {
            get => JsonSerializer
                     .Deserialize<ConcurrentDictionary<int, int>>(ResultCodesJson)
                   ?? new();
            set => ResultCodesJson = JsonSerializer.Serialize(value);
        }
    }
}
