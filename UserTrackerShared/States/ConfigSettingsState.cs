using System;
using System.Configuration;
using UserTrackerShared.Helpers;
using AppSettingsReader = UserTrackerShared.Helpers.AppSettingsReader;

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

        public static bool QuestDbEnabled { get; set; }
        public static int QuestDbPort { get; set; }
        public static string QuestDbHost { get; set; } = string.Empty;
        public static string QuestDbUser { get; set; } = string.Empty;
        public static string QuestDbPassword { get; set; } = string.Empty;

        public static int PullBackwardsTickAmount { get; set; }
        public static int TicksInObject { get; set; }
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
            Init(new AppSettingsReader(ConfigurationManager.AppSettings));
        }

        private static void Init(AppSettingsReader reader)
        {
            RunningHistoryTested = reader.GetRequiredBool("RUNNING_HISTORY_TESTED");

            ScreepsToken = reader.GetString("SCREEPS_API_TOKEN");
            ScreepsHttpsUrl = reader.GetString("SCREEPS_API_HTTPS_URL");
            ScreepsHttpUrl = reader.GetString("SCREEPS_API_HTTP_URL");
            ScreepsIsPrivateServer = ScreepsHttpsUrl != "https://screeps.com";
            ScreepsUsername = reader.GetString("SCREEPS_API_USERNAME");
            ScreepsPassword = reader.GetString("SCREEPS_API_PASSWORD");
            ScreepsShardName = reader.GetString("SCREEPS_SHARDNAME");

            ServerName = reader.GetString("SERVER_NAME");

            InfluxDbEnabled = reader.GetRequiredBool("INFLUXDB_ENABLED");
            InfluxDbHost = reader.GetString("INFLUXDB_HOST");
            InfluxDbToken = reader.GetString("INFLUXDB_TOKEN");

            GraphiteDbEnabled = reader.GetRequiredBool("GRAPHITE_ENABLED");
            GraphiteDbHost = reader.GetString("GRAPHITE_HOST");
            GraphiteDbPort = reader.GetRequiredInt("GRAPHITE_PORT");

            TimeScaleDbEnabled = reader.GetRequiredBool("TIMESCALE_ENABLED");
            TimeScaleDbHost = reader.GetString("TIMESCALE_HOST");
            TimeScaleDbPort = reader.GetRequiredInt("TIMESCALE_PORT");
            TimeScaleDbDBName = reader.GetString("TIMESCALE_DB");
            TimeScaleDbUser = reader.GetString("TIMESCALE_USERNAME");
            TimeScaleDbPassword = reader.GetString("TIMESCALE_PASSWORD");

            QuestDbEnabled = reader.GetRequiredBool("QUESTDB_ENABLED");
            QuestDbHost = reader.GetString("QUESTDB_HOST");
            QuestDbPort = reader.GetRequiredInt("QUESTDB_PORT");
            QuestDbUser = reader.GetString("QUESTDB_USERNAME");
            QuestDbPassword = reader.GetString("QUESTDB_PASSWORD");

            PullBackwardsTickAmount = reader.GetRequiredInt("PULL_BACKWARDS_TICK_AMOUNT");
            TicksInFile = reader.GetRequiredInt("TICKS_IN_FILE");
            TicksInObject = reader.GetRequiredInt("TICKS_IN_OBJECT");
            GetAllUsers = reader.GetRequiredBool("GET_ALL_USERS");
            StartsShards = reader.GetRequiredBool("START_SHARDS");
            LogsFolder = reader.GetRequiredString("LOGS_FOLDER");
            ObjectsFolder = reader.GetRequiredString("OBJECTS_FOLDER");

            WriteHistoryFiles = reader.GetRequiredBool("WRITE_HISTORY_FILES");
            WriteHistoryProperties = reader.GetRequiredBool("WRITE_HISTORY_PROPERTIES");
            LiveAssertRoomHistory = reader.GetRequiredBool("LIVE_ASSERT_ROOM_HISTORY");
        }

        public static void InitTest(AppSettingsSection appSettingsSection)
        {
            var settings = appSettingsSection.Settings;
            TicksInFile = Convert.ToInt32(settings["TICKS_IN_FILE"].Value);
            TicksInObject = Convert.ToInt32(settings["TICKS_IN_OBJECT"].Value);
        }
    }
}
