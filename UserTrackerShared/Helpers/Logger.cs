using Serilog;

namespace UserTrackerShared.Helpers
{
    using Serilog;
    using System;
    using System.IO;

    public static class Logger
    {
        public static ILogger GetLogger(LogCategory category)
        {
            string logDirectory = Path.Combine(ConfigSettingsState.LogsFolder, category.ToString().ToLower());
            Directory.CreateDirectory(logDirectory);

            string logFilePath = Path.Combine(logDirectory, "log-.txt");

            return new LoggerConfiguration()
                .WriteTo.File(
                    logFilePath,
                    rollingInterval: RollingInterval.Day,  // Create a new file each day
                    retainedFileCountLimit: 31,             // Keep last 31 log files
                    fileSizeLimitBytes: 20_000_000,        // Max 20MB per file
                    shared: true,                         // Allow multiple processes to log
                    flushToDiskInterval: TimeSpan.FromSeconds(1) // Flush logs quickly
                )
                .CreateLogger();
        }
    }

    public enum LogCategory
    {
        ScreepsAPI,
        States,
        PullPerformance,
        InfluxDB,
        GraphiteDB
    }

}
