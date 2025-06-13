using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTrackerShared.States
{
    public static class ConfigSettingsState
    {
        public static bool RunningHistoryTested { get; set; }

        public static string ScreepsToken { get; set; } = string.Empty;
        public static string ScreepsHttpsUrl { get; set; } = string.Empty;
        public static string ScreepsHttpUrl { get; set; } = string.Empty;
        public static bool ScreepsIsPrivateServer { get; set; }
        public static string ScreepsUsername { get; set; } = string.Empty;
        public static string ScreepsPassword { get; set; } = string.Empty;
        public static string ScreepsShardName { get; set; } = string.Empty;

        public static string ServerName { get; set; } = string.Empty;

        public static bool InfluxDbEnabled { get; set; }
        public static string InfluxDbHost { get; set; } = string.Empty;
        public static string InfluxDbToken { get; set; } = string.Empty;

        public static bool GraphiteDbEnabled { get; set; }
        public static string GraphiteDbHost { get; set; } = string.Empty;
        public static int GraphiteDbPort { get; set; }

        public static bool TimeScaleDbEnabled { get; set; }
        public static int TimeScaleDbPort { get; set; }
        public static string TimeScaleDbHost { get; set; } = string.Empty;
        public static string TimeScaleDbDBName { get; set; } = string.Empty;
        public static string TimeScaleDbUser { get; set; } = string.Empty;
        public static string TimeScaleDbPassword { get; set; } = string.Empty;

        public static int PullBackwardsTickAmount { get; set; }
        public static int TicksInFile { get; set; }
        public static bool GetAllUsers { get; set; }
        public static bool StartsShards { get; set; }
        public static string LogsFolder { get; set; } = string.Empty;


        public static bool WriteHistoryFiles { get; set; }
        public static bool WriteHistoryProperties { get; set; }
        public static string ObjectsFolder { get; set; } = string.Empty;
        public static bool LiveAssertRoomHistory { get; set; }

        public static void Init()
        {
            Init(ConfigurationManager.AppSettings);
        }
        private static void Init(NameValueCollection appSettings)
        {
            RunningHistoryTested = Convert.ToBoolean(appSettings["RUNNING_HISTORY_TESTED"]);

            ScreepsToken = appSettings["SCREEPS_API_TOKEN"] ?? "";
            ScreepsHttpsUrl = appSettings["SCREEPS_API_HTTPS_URL"] ?? "";
            ScreepsHttpUrl = appSettings["SCREEPS_API_HTTP_URL"] ?? "";
            ScreepsIsPrivateServer = ScreepsHttpsUrl != "https://screeps.com";
            ScreepsUsername = appSettings["SCREEPS_API_USERNAME"] ?? "";
            ScreepsPassword = appSettings["SCREEPS_API_PASSWORD"] ?? "";
            ScreepsShardName = appSettings["SCREEPS_SHARDNAME"] ?? "";

            ServerName = appSettings["SERVER_NAME"] ?? "";

            InfluxDbEnabled = Convert.ToBoolean(appSettings["INFLUXDB_ENABLED"]);
            InfluxDbHost = appSettings["INFLUXDB_HOST"] ?? "";
            InfluxDbToken = appSettings["INFLUXDB_TOKEN"] ?? "";

            GraphiteDbEnabled = Convert.ToBoolean(appSettings["GRAPHITE_ENABLED"]);
            GraphiteDbHost = appSettings["GRAPHITE_HOST"] ?? "";
            GraphiteDbPort = Convert.ToInt32(appSettings["GRAPHITE_PORT"] ?? "");
            
            TimeScaleDbEnabled = Convert.ToBoolean(appSettings["TIMESCALE_ENABLED"]);
            TimeScaleDbHost = appSettings["TIMESCALE_HOST"] ?? "";
            TimeScaleDbPort = Convert.ToInt32(appSettings["TIMESCALE_PORT"] ?? "");
            TimeScaleDbDBName = appSettings["TIMESCALE_DB"] ?? "";
            TimeScaleDbUser = appSettings["TIMESCALE_USERNAME"] ?? "";
            TimeScaleDbPassword = appSettings["TIMESCALE_PASSWORD"] ?? "";

            PullBackwardsTickAmount = Convert.ToInt32(appSettings["PULL_BACKWARDS_TICK_AMOUNT"]);
            TicksInFile = Convert.ToInt32(appSettings["TICKS_IN_FILE"]);
            GetAllUsers = Convert.ToBoolean(appSettings["GET_ALL_USERS"]);
            StartsShards = Convert.ToBoolean(appSettings["START_SHARDS"]);
            LogsFolder = appSettings["LOGS_FOLDER"] ?? "";
            if (LogsFolder == "") throw new ArgumentException("No logs folder provided");
            ObjectsFolder = appSettings["OBJECTS_FOLDER"] ?? "";
            if (ObjectsFolder == "") throw new ArgumentException("No objects folder provided");


            WriteHistoryFiles = Convert.ToBoolean(appSettings["WRITE_HISTORY_FILES"]);
            WriteHistoryProperties = Convert.ToBoolean(appSettings["WRITE_HISTORY_PROPERTIES"]);
            LiveAssertRoomHistory = Convert.ToBoolean(appSettings["LIVE_ASSERT_ROOM_HISTORY"]);
        }
        public static void InitTest(AppSettingsSection appSettingsSection)
        {
            var settings = appSettingsSection.Settings;
            TicksInFile = Convert.ToInt32(settings["TICKS_IN_FILE"].Value);
        }
    }
}
