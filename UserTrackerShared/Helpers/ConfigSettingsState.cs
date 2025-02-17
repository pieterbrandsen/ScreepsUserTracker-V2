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
        public static bool RunningHistoryTested { get; set; }
        
        
        public static bool InfluxDbEnabled { get; set; }
        public static string InfluxDbToken { get; set; }
        public static string InfluxDbServer { get; set; }


        public static bool GetAllUsers { get; set; }
        public static bool LoadSeasonalLeaderboard { get; set; }
        public static bool StartsShards { get; set; }
        public static string LogsFolder { get; set; }


        public static bool WriteHistoryFiles { get; set; }
        public static bool WriteHistoryProperties { get; set; }

        public static void Init() {
            RunningHistoryTested = Convert.ToBoolean(ConfigurationManager.AppSettings["RUNNING_HISTORY_TESTED"]);


            InfluxDbEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["INFLUXDB_ENABLED"]);
            InfluxDbToken = ConfigurationManager.AppSettings["INFLUXDB_TOKEN"] ?? "";
            InfluxDbServer = ConfigurationManager.AppSettings["INFLUXDB_SERVER"] ?? "";


            GetAllUsers = Convert.ToBoolean(ConfigurationManager.AppSettings["GET_ALL_USERS"]);
            LoadSeasonalLeaderboard = Convert.ToBoolean(ConfigurationManager.AppSettings["LOAD_SEASONAL_LEADERBOARD"]);
            StartsShards = Convert.ToBoolean(ConfigurationManager.AppSettings["START_SHARDS"]);
            LogsFolder = ConfigurationManager.AppSettings["LOGS_FOLDER"] ?? "";
            if (LogsFolder == "") throw new Exception("No logs folder provided");


            WriteHistoryFiles = Convert.ToBoolean(ConfigurationManager.AppSettings["WRITE_HISTORY_FILES"]);
            WriteHistoryProperties = Convert.ToBoolean(ConfigurationManager.AppSettings["WRITE_HISTORY_PROPERTIES"]);
        }
    }
}
