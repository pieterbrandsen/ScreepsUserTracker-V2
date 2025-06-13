using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTrackerShared.DBClients.TimeScaleDB
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // 1) Try to take the conn-string from the first CLI arg:
            string connString = args.Length > 0
                ? args[0]
                // 2) Or fall back to an environment variable:
                : Environment.GetEnvironmentVariable("TIMESCALE__CONN")
                  ?? throw new InvalidOperationException(
                       "No connection string provided. " +
                       "Pass it as the first argument to dotnet ef, " +
                       "or set the TIMESCALE__CONN env var.");

            // 3) Build the EF options:
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(
                connString,
                npgsql =>
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory")
            );

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
