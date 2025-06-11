using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTrackerShared.Helpers
{
    public static class ConfigSettingsState
    {
        public static bool RunningHistoryTested { get; set; }

        public static required string ScreepsToken { get; set; }
        public static required string ScreepsHttpsUrl { get; set; }
        public static required string ScreepsHttpUrl { get; set; }
        public static bool ScreepsIsPrivateServer { get; set; }
        public static required string ScreepsUsername { get; set; }
        public static required string ScreepsPassword { get; set; }
        public static required string ScreepsShardName { get; set; }

        public static required string ServerName { get; set; }

        public static bool InfluxDbEnabled { get; set; }
        public static required string InfluxDbHost { get; set; }
        public static required string InfluxDbToken { get; set; }

        public static bool GraphiteDbEnabled { get; set; }
        public static required string GraphiteDbHost { get; set; }
        public static int GraphiteDbPort { get; set; }

        public static int PullBackwardsTickAmount { get; set; }
        public static int TicksInFile { get; set; }
        public static bool GetAllUsers { get; set; }
        public static bool StartsShards { get; set; }
        public static string? LogsFolder { get; set; }


        public static bool WriteHistoryFiles { get; set; }
        public static bool WriteHistoryProperties { get; set; }
        public static string? ObjectsFolder { get; set; }

        public static void Init()
        {
            Init(ConfigurationManager.AppSettings);
        }
        private static void Init(NameValueCollection appsettings)
        {
            RunningHistoryTested = Convert.ToBoolean(appsettings["RUNNING_HISTORY_TESTED"]);

            ScreepsToken = appsettings["SCREEPS_API_TOKEN"] ?? "";
            ScreepsHttpsUrl = appsettings["SCREEPS_API_HTTPS_URL"] ?? "";
            ScreepsHttpUrl = appsettings["SCREEPS_API_HTTP_URL"] ?? "";
            ScreepsIsPrivateServer = ScreepsHttpsUrl != "https://screeps.com";
            ScreepsUsername = appsettings["SCREEPS_API_USERNAME"] ?? "";
            ScreepsPassword = appsettings["SCREEPS_API_PASSWORD"] ?? "";
            ScreepsShardName = appsettings["SCREEPS_SHARDNAME"] ?? "";

            ServerName = appsettings["SERVER_NAME"] ?? "";

            InfluxDbEnabled = Convert.ToBoolean(appsettings["INFLUXDB_ENABLED"]);
            InfluxDbHost = appsettings["INFLUXDB_HOST"] ?? "";
            InfluxDbToken = appsettings["INFLUXDB_TOKEN"] ?? "";

            GraphiteDbEnabled = Convert.ToBoolean(appsettings["GRAPHITE_ENABLED"]);
            GraphiteDbHost = appsettings["GRAPHITE_HOST"] ?? "";
            GraphiteDbPort = Convert.ToInt32(appsettings["GRAPHITE_PORT"] ?? "");

            PullBackwardsTickAmount = Convert.ToInt32(appsettings["PULL_BACKWARDS_TICK_AMOUNT"]);
            TicksInFile = Convert.ToInt32(appsettings["TICKS_IN_FILE"]);
            GetAllUsers = Convert.ToBoolean(appsettings["GET_ALL_USERS"]);
            StartsShards = Convert.ToBoolean(appsettings["START_SHARDS"]);
            LogsFolder = appsettings["LOGS_FOLDER"] ?? "";
            if (LogsFolder == "") throw new ArgumentException("No logs folder provided");
            ObjectsFolder = appsettings["OBJECTS_FOLDER"] ?? "";
            if (ObjectsFolder == "") throw new ArgumentException("No objects folder provided");


            WriteHistoryFiles = Convert.ToBoolean(appsettings["WRITE_HISTORY_FILES"]);
            WriteHistoryProperties = Convert.ToBoolean(appsettings["WRITE_HISTORY_PROPERTIES"]);
        }
        public static void InitTest(AppSettingsSection appSettingsSection)
        {
            var settings = appSettingsSection.Settings;
            TicksInFile = Convert.ToInt32(settings["TICKS_IN_FILE"].Value);
        }
    }
}
