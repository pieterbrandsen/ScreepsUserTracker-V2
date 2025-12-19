using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Net;
using UserTrackerShared.DBClients;
using UserTrackerShared.Models;
using UserTrackerShared.States;

namespace UserTrackerShared.Helpers
{
    public static class RoomDataHelper
    {
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.Shard);

        public static async Task<int> GetAndHandleRoomData(string shard, string name, long tick, ConcurrentDictionary<string, ScreepsRoomHistoryDto> dataByRoom, ConcurrentDictionary<string, object> userLocks)
        {
            try
            {
                var (roomData, Result) = await ScreepsApi.GetHistory(shard, name, tick);
                if (roomData == null)
                {
                    return (int)Result;
                }

                var roomHistory = new ScreepsRoomHistory();
                if (!dataByRoom.TryGetValue(name, out ScreepsRoomHistoryDto? roomHistoryDto))
                {
                    roomHistoryDto = new ScreepsRoomHistoryDto();
                    dataByRoom[name] = roomHistoryDto;
                }
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
