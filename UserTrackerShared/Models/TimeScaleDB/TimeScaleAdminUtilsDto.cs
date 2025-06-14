using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using UserTrackerShared.Models.ScreepsAPI;

namespace UserTrackerShared.Models.TimeScaleDB
{
    public class TimeScaleAdminUtilsDto : AdminUtilsDto
    {
        [Key]
        public int Id { get; set; }
        public string Server { get; set; } = null!;
        public DateTime Time { get; set; }
        [Column(TypeName = "jsonb")]
        public string ObjectsJson { get; set; } = "{}";

        [NotMapped]
        public new AdminUtilsObjects Objects
        {
            get => JsonSerializer
                     .Deserialize<AdminUtilsObjects>(ObjectsJson)
                   ?? new();
            set => ObjectsJson = JsonSerializer.Serialize(value);
        }
        [Column(TypeName = "jsonb")]
        public string TicksJson { get; set; } = "{}";

        [NotMapped]
        public new AdminUtilsTicks Ticks
        {
            get => JsonSerializer
                     .Deserialize<AdminUtilsTicks>(TicksJson)
                   ?? new();
            set => TicksJson = JsonSerializer.Serialize(value);
        }
        [Column(TypeName = "jsonb")]
        public string UsersJson { get; set; } = "{}";

        [NotMapped]
        public new Dictionary<string, AdminUtilsUser> Users
        {
            get => JsonSerializer
                     .Deserialize<Dictionary<string, AdminUtilsUser>>(UsersJson)
                   ?? new();
            set => UsersJson = JsonSerializer.Serialize(value);
        }
    }
}
