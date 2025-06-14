using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using UserTrackerShared.Models.ScreepsAPI;

namespace UserTrackerShared.Models.TimeScaleDB
{
    public class TimeScaleSeasonItem : SeasonListItem
    {
        [Key]
        public new int Id { get; set; }
        public DateTime Time { get; set; }
        public string Server { get; set; } = null!;
    }
}
