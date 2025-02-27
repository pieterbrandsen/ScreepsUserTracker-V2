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

        public static string ScreepsToken { get; set; }
        public static string ScreepsHttpsUrl { get; set; }
        public static string ScreepsHttpUrl { get; set; }
        public static bool ScreepsIsPrivateServer { get; set; }
        public static string ScreepsUsername { get; set; }
        public static string ScreepsPassword { get; set; }
        public static string ScreepsShardName { get; set; }

        public static string ServerName { get; set; }
        
        public static bool InfluxDbEnabled { get; set; }
        public static string InfluxDbHost { get; set; }
        public static string InfluxDbToken { get; set; }

        public static bool GraphiteDbEnabled { get; set; }
        public static string GraphiteDbHost { get; set; }
        public static int GraphiteDbPort { get; set; }

        public static int TicksInFile { get; set; }
        public static bool GetAllUsers { get; set; }
        public static bool LoadSeasonalLeaderboard { get; set; }
        public static bool StartsShards { get; set; }
        public static string LogsFolder { get; set; }


        public static bool WriteHistoryFiles { get; set; }
        public static bool WriteHistoryProperties { get; set; }

        public static void Init() {
            RunningHistoryTested = Convert.ToBoolean(ConfigurationManager.AppSettings["RUNNING_HISTORY_TESTED"]);

            ScreepsToken = ConfigurationManager.AppSettings["SCREEPS_API_TOKEN"] ?? "";
            ScreepsHttpsUrl = ConfigurationManager.AppSettings["SCREEPS_API_HTTPS_URL"] ?? "";
            ScreepsHttpUrl = ConfigurationManager.AppSettings["SCREEPS_API_HTTP_URL"] ?? "";
            ScreepsIsPrivateServer = ScreepsHttpsUrl != "https://screeps.com";
            ScreepsUsername = ConfigurationManager.AppSettings["SCREEPS_API_USERNAME"] ?? "";
            ScreepsPassword = ConfigurationManager.AppSettings["SCREEPS_API_PASSWORD"] ?? "";
            ScreepsShardName = ConfigurationManager.AppSettings["SCREEPS_SHARDNAME"] ?? "";

            ServerName = ConfigurationManager.AppSettings["SERVER_NAME"] ?? "";

            InfluxDbEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["INFLUXDB_ENABLED"]);
            InfluxDbHost = ConfigurationManager.AppSettings["INFLUXDB_HOST"] ?? "";
            InfluxDbToken = ConfigurationManager.AppSettings["INFLUXDB_TOKEN"] ?? "";

            GraphiteDbEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["GRAPHITE_ENABLED"]);
            GraphiteDbHost = ConfigurationManager.AppSettings["GRAPHITE_HOST"] ?? "";
            GraphiteDbPort = Convert.ToInt32(ConfigurationManager.AppSettings["GRAPHITE_PORT"] ?? "");

            TicksInFile = Convert.ToInt32(ConfigurationManager.AppSettings["TICKS_IN_FILE"]);
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
