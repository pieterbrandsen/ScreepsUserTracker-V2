using Serilog;

namespace UserTrackerShared.Helpers
{
    using Serilog;
    using System;
    using System.IO;
    using UserTrackerShared.States;

    public static class Logger
    {
        public static ILogger GetLogger(LogCategory category)
        {
            if (ConfigSettingsState.LogsFolder == null)
            {
                throw new InvalidOperationException("Logs folder is not configured.");
            }
            string logDirectory = Path.Combine(ConfigSettingsState.LogsFolder, category.ToString().ToLower());
            Directory.CreateDirectory(logDirectory);

            string logFilePath = Path.Combine(logDirectory, "log-.txt");

            return new LoggerConfiguration()
                .WriteTo.File(
                    logFilePath,
                    rollingInterval: RollingInterval.Day,  // Create a new file each day
                    retainedFileCountLimit: 31,             // Keep last 31 log files
                    fileSizeLimitBytes: 100_000_000,        // Max 20MB per file
                    shared: true,                         // Allow multiple processes to log
                    flushToDiskInterval: TimeSpan.FromSeconds(1) // Flush logs quickly
                )
                .CreateLogger();
        }
    }

    public enum LogCategory
    {
        // Main
        General,
        // Detailed
        Leaderboard,
        Shard,
        ScreepsAPI,
        HistoryProcessor,
        PullPerformance,
        // Db's
        InfluxDB,
        GraphiteDB,
        TimeScaleDB,
        QuestDB
    }
}
