using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTrackerShared.Helpers
{
    public static class HistoryConfigSettingsState
    {
        public static string LoopStrategy { get; set; } = string.Empty;
        public static string HistoryBasePath { get; set; } = string.Empty;
        public static bool InluceGoodFiles { get; set; }
        public static bool IncludeBadFiles { get; set; }
        public static bool IncludeUnknownFiles { get; set; }
        public static bool ThrowOnBadFile { get; set; }

        public static void Init()
        {
            LoopStrategy = ConfigurationManager.AppSettings["LOOP_STRATEGY"] ?? "";
            HistoryBasePath = ConfigurationManager.AppSettings["HISTORY_BASE_PATH"] ?? "";
            InluceGoodFiles = Convert.ToBoolean(ConfigurationManager.AppSettings["INCLUDE_GOOD_FILES"]);
            IncludeBadFiles = Convert.ToBoolean(ConfigurationManager.AppSettings["INCLUDE_BAD_FILES"]);
            IncludeUnknownFiles = Convert.ToBoolean(ConfigurationManager.AppSettings["INCLUDE_UNKNOWN_FILES"]);
            ThrowOnBadFile = Convert.ToBoolean(ConfigurationManager.AppSettings["THROW_ON_BAD_FILE"]);
        }
    }
}
