using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserTrackerShared.Models;
using UserTrackerShared.Models.TimeScaleDB;

namespace UserTrackerShared.DBClients.TimeScaleDB
{
    public class AppDbContext : DbContext
    {
        public DbSet<TimeScalePerformanceClassDto> PerformanceStats { get; set; }
        public DbSet<TimeScaleScreepsRoomHistoryDto> ScreepsRoomHistory { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> opts)
            : base(opts)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Make Time default to now() in Postgres
            builder.Entity<TimeScalePerformanceClassDto>()
                   .Property(e => e.Time)
                   .HasDefaultValueSql("now()");
            builder.Entity<TimeScaleScreepsRoomHistoryDto>();
        }
    }
}
