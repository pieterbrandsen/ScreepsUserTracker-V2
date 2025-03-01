using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Text;
using UserTrackerShared;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerShared.Models.ScreepsAPI;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace UserTrackerScreepsApi
{
    public static class JSONConvertHelper
    {
        public static async Task<T?> ReadAndConvertStream<T>(HttpContent httpContent)
        {
            using Stream responseStream = await httpContent.ReadAsStreamAsync().ConfigureAwait(false);
            using StreamReader reader = new StreamReader(responseStream);
            using JsonTextReader jsonReader = new JsonTextReader(reader);

            JsonSerializer serializer = new JsonSerializer();
            return serializer.Deserialize<T>(jsonReader);
        }
    }
    public static class ScreepsAPI
    {
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.ScreepsAPI);
        private static SemaphoreSlim _normalThrottler = new SemaphoreSlim(1);
        private static SemaphoreSlim _filesThrottler = new SemaphoreSlim(500);

        private static HttpClient _normalHttpClient = new HttpClient();

        private static HttpClient _filesHttpClient = new(new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            ConnectTimeout = TimeSpan.FromSeconds(10),
        });
        private static async Task<(HttpResponseMessage response, int retyCount)> ThrottledRequestAsync(HttpRequestMessage request)
        {
            await _normalThrottler.WaitAsync();
            try
            {
                var retryCount = 0;
                HttpResponseMessage response = null;

                while (retryCount < 100)
                {
                    await Task.Delay(50);
                    using (var clonedRequest = CloneHttpRequestMessage(request))
                    {
                        response = await _normalHttpClient.SendAsync(clonedRequest);
                        if (response.IsSuccessStatusCode || (int)response.StatusCode >= 500)
                            return (response, retryCount);
                    }
                    retryCount += 1;
                }
                return (response, retryCount);
            }
            finally
            {
                _normalThrottler.Release();
            }
        }

        public static HttpRequestMessage CloneHttpRequestMessage(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Version = request.Version
            };

            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (request.Content != null)
            {
                var contentBytes = request.Content.ReadAsByteArrayAsync().Result;
                clone.Content = new ByteArrayContent(contentBytes);

                foreach (var header in request.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return clone;
        }

        public static string ScreepsAPIUrl = ConfigSettingsState.ScreepsHttpsUrl;
        public static string ScreepsAPIHTTPUrl = ConfigSettingsState.ScreepsHttpUrl;
        public static string ScreepsAPIToken => ConfigSettingsState.ScreepsToken;

        private static async Task<(T? Result, HttpStatusCode Status)> ExecuteRequestAsync<T>(HttpMethod method, string path, StringContent? httpContent = null, bool isHistoryRequest = false)
        {
            var reqUrl = "";
            if (isHistoryRequest)
            {
                reqUrl = ScreepsAPIHTTPUrl + path;
            }
            else
            {
                reqUrl = ScreepsAPIUrl + path;
            }
            if (method == HttpMethod.Post && httpContent == null)
            {
                throw new ArgumentNullException("No HttpContent provided");
            }

            try
            {
                var retryCount = 0;
                HttpResponseMessage response = null;
                if (isHistoryRequest)
                {
                    await _filesThrottler.WaitAsync();
                    try
                    {
                        response = await _filesHttpClient.GetAsync(reqUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, reqUrl);
                        return (default, HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        _filesThrottler.Release();
                    }
                }
                else
                {
                    var request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(reqUrl),
                        Method = method,
                    };
                    if (httpContent != null)
                    {
                        request.Content = httpContent;
                    }

                    request.Headers.Add("X-Token", ScreepsAPIToken);
                    request.Headers.Add("X-Username", ScreepsAPIToken);
                    (response, retryCount) = await ThrottledRequestAsync(request);
                }
                _logger.Information($"{reqUrl} - {response.StatusCode} - {retryCount}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await JSONConvertHelper.ReadAndConvertStream<T>(response.Content);
                    return (result, response.StatusCode);
                }
                else
                {
                    return (default, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, reqUrl);
                return (default, HttpStatusCode.InternalServerError);
            }
        }

        public static async Task<SignInResponse?> SignIn(string username, string password)
        {
            var path = $"/api/auth/signin?email={username}&password={password}";
            var body = new StringContent("", Encoding.UTF8, "application/json");

            var (Result, Status) = await ExecuteRequestAsync<SignInResponse>(HttpMethod.Post, path, body);
            return Result;
        }

        public static async Task<TimeResponse?> GetTimeOfShard(string shard)
        {
            var path = $"/api/game/time?shard={shard}";

            var (Result, Status) = await ExecuteRequestAsync<TimeResponse>(HttpMethod.Get, path);
            return Result;
        }

        public static async Task<SeasonsReponse?> GetSeasons()
        {
            var path = $"/api/leaderboard/seasons";

            var (Result, Status) = await ExecuteRequestAsync<SeasonsReponse>(HttpMethod.Get, path);
            return Result;
        }

        private static async Task<SeasonListResponse?> GetCurrentSeasonLeaderboard(string mode, string season, int offset, int limit)
        {
            var path = $"/api/leaderboard/list?limit={limit}&mode={mode}&offset={offset}&season={season}";

            var (Result, Status) = await ExecuteRequestAsync<SeasonListResponse>(HttpMethod.Get, path);
            return Result;
        }
        public static async Task<List<SeaonListItem>> GetCurrentSeasonLeaderboard(string mode)
        {
            var season = DateTime.Now.ToString("yyyy-MM");
            var leaderboardList = new List<SeaonListItem>();

            int offset = 0;
            int limit = 20;
            while (true)
            {
                var listResponse = await GetCurrentSeasonLeaderboard(mode, season, offset, limit);
                if (listResponse == null || listResponse.List.Count == 0) break;
                leaderboardList.AddRange(listResponse.List);

                offset += limit;
            }

            return leaderboardList.OrderBy(s => s.Rank).ToList();
        }

        public static async Task<Dictionary<string, List<SeaonListItem>>> GetAllSeasonsLeaderboard(string mode)
        {
            var leaderboardsList = new Dictionary<string, List<SeaonListItem>>();

            var lastSeasonEmpty = false;
            var season = DateTime.Now.ToString("yyyy-MM");
            while (!lastSeasonEmpty)
            {
                var leaderboardList = new List<SeaonListItem>();
                season = DateTime.Parse(season).AddMonths(-1).ToString("yyyy-MM");

                int offset = 0;
                int limit = 20;
                while (true)
                {
                    var listResponse = await GetCurrentSeasonLeaderboard(mode, season, offset, limit);
                    if (listResponse == null || listResponse.List.Count == 0) break;
                    leaderboardList.AddRange(listResponse.List);

                    offset += limit;
                }

                if (leaderboardList.Count == 0)
                {
                    lastSeasonEmpty = true;
                }
                else
                {
                    leaderboardsList[season] = leaderboardList;
                }
            }


            return leaderboardsList;
        }

        public static async Task<SeasonListResponse?> GetLeaderboardsOfUser(string mode, string username)
        {
            var path = $"/api/leaderboard/find?mode={mode}&username={username}";

            var (Result, Status) = await ExecuteRequestAsync<SeasonListResponse>(HttpMethod.Get, path);
            return Result;
        }

        public static async Task<ScreepsUser?> GetUser(string userId)
        {
            var path = $"/api/user/find?id={userId}";

            var (Result, Status) = await ExecuteRequestAsync<GetUserResponse>(HttpMethod.Get, path);
            return Status == HttpStatusCode.OK && Result.Ok == 1 ? Result.User : null;
        }

        public static async Task<(JObject? Result, HttpStatusCode Status)> GetHistory(string shard, string room, long tick)
        {
            var path = !ConfigSettingsState.ScreepsIsPrivateServer ? $"/room-history/{shard}/{room}/{tick}.json" : $"/room-history?room={room}&time={tick}";
            return await ExecuteRequestAsync<JObject>(HttpMethod.Get, path, isHistoryRequest: true);
        }

        public static async Task<AdminUtilsResponse?> GetAdminUtilsStats()
        {
            var path = "/stats";

            var (Result, Status) = await ExecuteRequestAsync<AdminUtilsResponse>(HttpMethod.Get, path);
            return Result;
        }

        public static async Task<MapStatsResponse?> GetMapStats(List<string> rooms, string shard, string statName)
        {
            var path = $"/api/game/map-stats";
            var content = new Dictionary<string, object>()
            {
                { "rooms", rooms },
                { "statName", statName },
                { "shard", shard }
            };
            var jsonString = JsonConvert.SerializeObject(content);
            var body = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var (Result, Status) = await ExecuteRequestAsync<MapStatsResponse>(HttpMethod.Post, path, body);
            if (Result != null)
            {
                Result.Rooms = Result.Rooms.Where(s => s.Value.Status != "out of borders").ToDictionary();
            }
            return Result;
        }

        private static async Task<MapStatsResponse?> GetAllMapsStatsOfDirection(
            string shard,
            string statName,
            bool isNorth,
            bool isEast,
            int startIndex,
            MapStatsResponse? mapStatsResponse = null)
        {
            mapStatsResponse ??= new MapStatsResponse();

            HashSet<string> rooms = new HashSet<string>();
            int layerSize = 10;

            int min = startIndex;
            int max = startIndex + layerSize;

            // Corner
            for (int x = min; x < max; x++)
            {
                for (int y = min; y < max; y++)
                {
                    rooms.Add($"{(isEast ? 'E' : 'W')}{x}{(isNorth ? 'N' : 'S')}{y}");
                }
            }
            for (int x = min; x < max; x++)
            {
                for (int y = 0; y < max; y++)
                {
                    rooms.Add($"{(isEast ? 'E' : 'W')}{x}{(isNorth ? 'N' : 'S')}{y}");
                }
            }
            for (int x = 0; x < max; x++)
            {
                for (int y = min; y < max; y++)
                {
                    rooms.Add($"{(isEast ? 'E' : 'W')}{x}{(isNorth ? 'N' : 'S')}{y}");
                }
            }

            var anySucceeded = false;

            for (int i = 0; i < rooms.Count; i += 500)
            {
                var roomsPart = rooms.Skip(i).Take(500).ToList();

                var mapStats = await GetMapStats(roomsPart, shard, statName);
                if (mapStats != null && mapStats.Rooms.Count > 0)
                {
                    anySucceeded = true;
                    mapStatsResponse.Rooms = mapStatsResponse.Rooms.Concat(mapStats.Rooms).ToDictionary();
                    mapStatsResponse.Users = mapStatsResponse.Users
                        .Concat(mapStats.Users.Where(user => !mapStatsResponse.Users.ContainsKey(user.Key)))
                        .ToDictionary();
                }
                else
                {

                }
            }

            if (!anySucceeded)
            {
                return mapStatsResponse;
            }

            return await GetAllMapsStatsOfDirection(shard, statName, isNorth, isEast, startIndex + layerSize, mapStatsResponse);
        }

        public static async Task<MapStatsResponse> GetAllMapStats(string shard, string statName)
        {
            MapStatsResponse mapStatsResponse = new();


            // N & E
            var data = await GetAllMapsStatsOfDirection(shard, statName, true, true, 0);
            if (data != null)
            {
                mapStatsResponse.Rooms = mapStatsResponse.Rooms
                    .Concat(data.Rooms.Where(room => !mapStatsResponse.Rooms.ContainsKey(room.Key)))
                    .ToDictionary();
                mapStatsResponse.Users = mapStatsResponse.Users
                    .Concat(data.Users.Where(user => !mapStatsResponse.Users.ContainsKey(user.Key)))
                    .ToDictionary();
            }
            //// S & E
            data = await GetAllMapsStatsOfDirection(shard, statName, false, true, 0);
            if (data != null)
            {
                mapStatsResponse.Rooms = mapStatsResponse.Rooms
                    .Concat(data.Rooms.Where(room => !mapStatsResponse.Rooms.ContainsKey(room.Key)))
                    .ToDictionary();
                mapStatsResponse.Users = mapStatsResponse.Users
                    .Concat(data.Users.Where(user => !mapStatsResponse.Users.ContainsKey(user.Key)))
                    .ToDictionary();
            }

            //// N & E
            data = await GetAllMapsStatsOfDirection(shard, statName, true, false, 0);
            if (data != null)
            {
                mapStatsResponse.Rooms = mapStatsResponse.Rooms
                    .Concat(data.Rooms.Where(room => !mapStatsResponse.Rooms.ContainsKey(room.Key)))
                    .ToDictionary();
                mapStatsResponse.Users = mapStatsResponse.Users
                    .Concat(data.Users.Where(user => !mapStatsResponse.Users.ContainsKey(user.Key)))
                    .ToDictionary();
            }
            //// S & W
            data = await GetAllMapsStatsOfDirection(shard, statName, false, false, 0);
            if (data != null)
            {
                mapStatsResponse.Rooms = mapStatsResponse.Rooms
                    .Concat(data.Rooms.Where(room => !mapStatsResponse.Rooms.ContainsKey(room.Key)))
                    .ToDictionary();
                mapStatsResponse.Users = mapStatsResponse.Users
                    .Concat(data.Users.Where(user => !mapStatsResponse.Users.ContainsKey(user.Key)))
                    .ToDictionary();
            }

            return mapStatsResponse;
        }
    }
}

