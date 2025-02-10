using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTrackerShared.Helpers
{
    public static class ConfigSettingsState
    {
        public static bool InfluxDbEnabled { get; set; }
        public static string InfluxDbToken { get; set; }
        public static string InfluxDbServer { get; set; }

        public static bool WriteHistoryFiles { get; set; }
        public static bool WriteHistoryProperties { get; set; }
        public static bool WriteHistoryTypeProperties { get; set; }

        public static void Init() {
            InfluxDbEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["INFLUXDB_ENABLED"]);
            InfluxDbToken = ConfigurationManager.AppSettings["INFLUXDB_TOKEN"] ?? "";
            InfluxDbServer = ConfigurationManager.AppSettings["INFLUXDB_SERVER"] ?? "";

            WriteHistoryFiles = Convert.ToBoolean(ConfigurationManager.AppSettings["WRITE_HISTORY_FILES"]);
            WriteHistoryProperties = Convert.ToBoolean(ConfigurationManager.AppSettings["WRITE_HISTORY_PROPERTIES"]);
            WriteHistoryTypeProperties = Convert.ToBoolean(ConfigurationManager.AppSettings["WRITE_HISTORY_TYPE_PROPERTIES"]);
        }
    }
}
