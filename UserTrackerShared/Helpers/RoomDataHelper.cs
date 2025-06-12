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
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.States);

        public static async Task<int> GetAndHandleRoomData(string shard, string name, long tick, ConcurrentDictionary<string, ScreepsRoomHistoryDto> reservedRoomsByUser, ConcurrentDictionary<string, object> userLocks)
        {
            try
            {
                var isReservedRoom = false;
                var (roomData, Result) = await ScreepsAPI.GetHistory(shard, name, tick);
                if (roomData == null)
                {
                    return (int)Result;
                }

                var roomHistory = new ScreepsRoomHistory();
                var roomHistoryDto = new ScreepsRoomHistoryDto();
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

                        if (roomHistory.Structures?.Controller?.Reservation?.User != null)
                        {
                            isReservedRoom = true;
                            var userKey = roomHistory.Structures.Controller.Reservation.User;
                            var userLock = userLocks.GetOrAdd(userKey, _ => new object());
                            lock (userLock)
                            {
                                if (!reservedRoomsByUser.TryGetValue(userKey, out ScreepsRoomHistoryDto? value))
                                {
                                    value = new ScreepsRoomHistoryDto();
                                    reservedRoomsByUser[userKey] = value;
                                }

                                value.Update(roomHistory);
                            }
                        }
                        else
                        {
                            roomHistoryDto.Update(roomHistory);
                        }
                    }
                }

                if (!isReservedRoom) await DBClient.WriteScreepsRoomHistory(shard, name, roomHistory.Tick, roomHistory.TimeStamp, roomHistoryDto);
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
