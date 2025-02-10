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
        public static string HistoryFilesLocation { get; set; }
        public static bool InluceGoodFiles { get; set; }
        public static bool IncludeBadFiles { get; set; }
        public static bool IncludeUnknownFiles { get; set; }

        public static void Init() {
            HistoryFilesLocation = ConfigurationManager.AppSettings["HISTORY_FILES_LOCATION"] ?? "";
            InluceGoodFiles = Convert.ToBoolean(ConfigurationManager.AppSettings["INCLUDE_GOOD_FILES"]);
            IncludeBadFiles = Convert.ToBoolean(ConfigurationManager.AppSettings["INCLUDE_BAD_FILES"]);
            IncludeUnknownFiles = Convert.ToBoolean(ConfigurationManager.AppSettings["INCLUDE_UNKNOWN_FILES"]);
        }
    }
}
