using System.Reactive;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;

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
                await InfluxDBClientState.WriteScreepsRoomHistory(shard, room, tick, timestamp, screepsRoomHistory);
            }
        }

        public static void WritePerformanceData(PerformanceClassDTO performanceClassDTO)
        {
            if (ConfigSettingsState.InfluxDbEnabled)
            {
                InfluxDBClientState.WritePerformanceData(performanceClassDTO);
            }
            if (ConfigSettingsState.GraphiteDbEnabled)
            {
                InfluxDBClientState.WritePerformanceData(performanceClassDTO);
            }
        }
    }
}
