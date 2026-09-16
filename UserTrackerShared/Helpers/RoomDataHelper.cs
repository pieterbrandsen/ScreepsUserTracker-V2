using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Net;
using UserTrackerShared.Models;
using UserTrackerShared.States;
using UserTrackerShared.Utilities;

namespace UserTrackerShared.Helpers
{
    public static class RoomDataHelper
    {
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.Shard);
        internal static Func<string, string, long, Task<(JObject?, HttpStatusCode)>> HistoryFetcher { get; set; } = ScreepsAPI.GetHistory;

        internal static void SetHistoryFetcher(Func<string, string, long, Task<(JObject?, HttpStatusCode)>> fetcher)
        {
            HistoryFetcher = fetcher;
        }

        internal static void ResetHistoryFetcher()
        {
            HistoryFetcher = ScreepsAPI.GetHistory;
        }

        public static async Task<int> GetAndHandleRoomData(string shard, string name, long tick,
            ConcurrentDictionary<long, ConcurrentDictionary<string, ScreepsRoomHistoryDto>> dataByWindow)
        {
            try
            {
                var (roomData, Result) = await HistoryFetcher(shard, name, tick);
                if (roomData == null)
                {
                    return (int)Result;
                }

                var roomHistory = new ScreepsRoomHistory();
                roomData.TryGetValue("timestamp", out JToken? jTokenTime);
                if (jTokenTime != null) roomHistory.TimeStamp = jTokenTime.Value<long>();

                roomData.TryGetValue("base", out JToken? jTokenBase);
                if (jTokenBase != null) roomHistory.Base = jTokenBase.Value<long>();

                if (roomData.TryGetValue("ticks", out JToken? jTokenTicks) && jTokenTicks is JObject jObjectTicks)
                {
                    for (int i = 0; i < ConfigSettingsState.TicksInFile; i++)
                    {
                        long tickNumber = roomHistory.Base + i;
                        roomHistory.Tick = tickNumber;
                        // Keep reconstructed history across windows, but average each window separately.
                        var windowStart = tickNumber - tickNumber % ConfigSettingsState.TicksInObject;
                        var dataByRoom = dataByWindow.GetOrAdd(windowStart, _ => new());
                        var roomHistoryDto = dataByRoom.GetOrAdd(name, _ => new());

                        if (jObjectTicks.TryGetValue(tickNumber.ToString(), out JToken? tickObject) && tickObject != null)
                        {
                            try
                            {
                                roomHistory = ScreepsRoomHistoryHelper.ComputeTick(tickObject, roomHistory);
                            }
                            catch (Exception e)
                            {
                                var message = $"Error processing tick {tickNumber} for room {name}: {e.Message}";
                                _logger.Error(e, message);
                            }
                        }

                        roomHistoryDto.Update(roomHistory);
                    }
                }

                if (ConfigSettingsState.WriteHistoryProperties) FileWriterManager.GenerateHistoryFile(roomData);
                return 200;
            }
            catch (Exception e)
            {
                var message = $"Error processing room {name} at tick {tick}: {e.Message}";
                _logger.Error(e, message);
                return (int)HttpStatusCode.InternalServerError;
            }
        }
    }
}
