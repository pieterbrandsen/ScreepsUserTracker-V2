using System.Reactive;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerShared.Models.ScreepsAPI;

namespace UserTrackerStates.DBClients
{
    public static class DBClient
    {
        public static void Init()
        {
            if (ConfigSettingsState.InfluxDbEnabled)
            {
                InfluxDBClientWriter.Init();
            }
            if (ConfigSettingsState.GraphiteDbEnabled)
            {
                GraphiteDBClientWriter.Init();
            }
        }

        public static async Task WriteScreepsRoomHistory(string shard, string room, long tick, long timestamp, ScreepsRoomHistoryDTO screepsRoomHistory)
        {
            if (ConfigSettingsState.InfluxDbEnabled)
            {
                await InfluxDBClientState.WriteScreepsRoomHistory(shard, room, tick, timestamp, screepsRoomHistory);
            }
            if (ConfigSettingsState.GraphiteDbEnabled)
            {
                await GraphiteDBClientState.WriteScreepsRoomHistory(shard, room, tick, timestamp, screepsRoomHistory);
            }
        }

        public static void WritePerformanceData(PerformanceClassDto PerformanceClassDto)
        {
            if (ConfigSettingsState.InfluxDbEnabled)
            {
                InfluxDBClientState.WritePerformanceData(PerformanceClassDto);
            }
            if (ConfigSettingsState.GraphiteDbEnabled)
            {
                GraphiteDBClientState.WritePerformanceData(PerformanceClassDto);
            }
        }

        public static void WriteLeaderboardData(SeaonListItem seasonItem)
        {
            if (ConfigSettingsState.InfluxDbEnabled)
            {
                // InfluxDBClientState.WriteLeaderboardData(seasonItem);
            }
            if (ConfigSettingsState.GraphiteDbEnabled)
            {
                GraphiteDBClientState.WriteLeaderboardData(seasonItem);
            }
        }

        public static void WriteSingleUserData(ScreepsUser user)
        {
            if (ConfigSettingsState.InfluxDbEnabled)
            {
                // InfluxDBClientState.WriteSingleUserData(user);
            }
            if (ConfigSettingsState.GraphiteDbEnabled)
            {
                GraphiteDBClientState.WriteSingleUserData(user);
            }
        }

        public static void WriteAdminUtilsData(AdminUtilsResponse data)
        {
            var dto = new AdminUtilsDto(data);
            if (ConfigSettingsState.InfluxDbEnabled)
            {
                // InfluxDBClientState.WriteSingleUserdData(user);
            }
            if (ConfigSettingsState.GraphiteDbEnabled)
            {
                GraphiteDBClientState.WriteAdminUtilsData(dto);
            }
        }
    }
}
