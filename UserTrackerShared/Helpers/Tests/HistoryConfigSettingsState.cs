using System;
using System.Configuration;

namespace UserTrackerShared.Helpers
{
    public static class HistoryConfigSettingsState
    {
        public static string LoopStrategy { get; set; } = string.Empty;
        public static string HistoryBasePath { get; set; } = string.Empty;
        public static bool IncludeGoodFiles { get; set; }
        public static bool IncludeBadFiles { get; set; }
        public static bool IncludeUnknownFiles { get; set; }
        public static bool ThrowOnBadFile { get; set; }

        public static void Init()
        {
            var reader = new AppSettingsReader(ConfigurationManager.AppSettings);
            LoopStrategy = reader.GetString("LOOP_STRATEGY");
            HistoryBasePath = reader.GetRequiredString("HISTORY_BASE_PATH");
            IncludeGoodFiles = reader.GetRequiredBool("INCLUDE_GOOD_FILES");
            IncludeBadFiles = reader.GetRequiredBool("INCLUDE_BAD_FILES");
            IncludeUnknownFiles = reader.GetRequiredBool("INCLUDE_UNKNOWN_FILES");
            ThrowOnBadFile = reader.GetRequiredBool("THROW_ON_BAD_FILE");
        }
    }
}
