using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using UserTrackerShared;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerShared.Models.ScreepsAPI;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace UserTrackerShared.States
{
    public static class JsonConvertHelper
    {
        public static async Task<T?> ReadAndConvertStream<T>(HttpContent httpContent)
        {
            using Stream responseStream = await httpContent.ReadAsStreamAsync().ConfigureAwait(false);
            using StreamReader reader = new(responseStream);
            using JsonTextReader jsonReader = new(reader);

            JsonSerializer serializer = new();
            return serializer.Deserialize<T>(jsonReader);
        }
    }
    public static class ScreepsApi
    {
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.ScreepsAPI);
        private static readonly SemaphoreSlim _normalThrottler = new(10);

        private static readonly HttpClient _normalHttpClient = new(new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            ConnectTimeout = TimeSpan.FromSeconds(5),
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            MaxConnectionsPerServer = 100,
            EnableMultipleHttp2Connections = true
        })
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        private static readonly HttpClient _filesHttpClient = new(new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            ConnectTimeout = TimeSpan.FromSeconds(30),
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5)
        });
        
        private static async Task<(HttpResponseMessage? response, int retryCount)> RequestAsync(HttpRequestMessage request)
        {
            int maxRetries = 3;
            int retryCount = 0;
            int delayMs = 100;

            while (retryCount < maxRetries)
            {
                try
                {
                    using var clonedRequest = CloneHttpRequestMessage(request);
                    var response = await _normalHttpClient.SendAsync(clonedRequest);
                    return (response, retryCount);
                }
                catch (HttpRequestException ex) when (ex.InnerException is IOException || ex.InnerException is SocketException)
                {
                    // Network errors like "broken pipe" should be retried
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        _logger.Error(ex, "Failed to send request after {MaxRetries} retries: {RequestUri}", maxRetries, request.RequestUri);
                        throw;
                    }

                    _logger.Warning("Network error on attempt {RetryCount}/{MaxRetries}, retrying in {Delay}ms: {RequestUri}",
                        retryCount, maxRetries, delayMs, request.RequestUri);

                    await Task.Delay(delayMs);
                    delayMs *= 2;
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    // Timeout errors should also be retried
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        _logger.Error(ex, "Request timeout after {MaxRetries} retries: {RequestUri}", maxRetries, request.RequestUri);
                        throw;
                    }

                    _logger.Warning("Timeout on attempt {RetryCount}/{MaxRetries}, retrying in {Delay}ms: {RequestUri}",
                        retryCount, maxRetries, delayMs, request.RequestUri);

                    await Task.Delay(delayMs);
                    delayMs *= 2;
                }
            }

            // This should never be reached, but just in case
            throw new InvalidOperationException("Unexpected end of retry loop");
        }
        private static async Task<(HttpResponseMessage? response, int retryCount)> ThrottledRequestAsync(HttpRequestMessage request)
        {
            await _normalThrottler.WaitAsync();
            try
            {
                var retryCount = 0;
                HttpResponseMessage? response = null;
                int maxRetries = 3;
                int delayMs = 1000; 

                while (retryCount < maxRetries)
                {
                    try
                    {
                        await Task.Delay(delayMs);
                        using (var clonedRequest = CloneHttpRequestMessage(request))
                        {
                            response = await _normalHttpClient.SendAsync(clonedRequest);
                            if (response.IsSuccessStatusCode || (int)response.StatusCode >= 500)
                                return (response, retryCount);
                        }
                        retryCount += 1;
                        delayMs *= 2;
                    }
                    catch (HttpRequestException ex) when (ex.InnerException is IOException || ex.InnerException is SocketException)
                    {
                        // Network errors like "broken pipe" should be retried
                        retryCount++;
                        if (retryCount >= maxRetries)
                        {
                            _logger.Error(ex, "Failed to send throttled request after {MaxRetries} retries: {RequestUri}", maxRetries, request.RequestUri);
                            return (null, retryCount);
                        }
                        
                        _logger.Warning("Network error on throttled attempt {RetryCount}/{MaxRetries}, retrying in {Delay}ms: {RequestUri}", 
                            retryCount, maxRetries, delayMs, request.RequestUri);
                        
                        await Task.Delay(delayMs);
                        delayMs *= 2;
                    }
                    catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                    {
                        // Timeout errors should also be retried
                        retryCount++;
                        if (retryCount >= maxRetries)
                        {
                            _logger.Error(ex, "Throttled request timeout after {MaxRetries} retries: {RequestUri}", maxRetries, request.RequestUri);
                            return (null, retryCount);
                        }
                        
                        _logger.Warning("Timeout on throttled attempt {RetryCount}/{MaxRetries}, retrying in {Delay}ms: {RequestUri}", 
                            retryCount, maxRetries, delayMs, request.RequestUri);
                        
                        await Task.Delay(delayMs);
                        delayMs *= 2;
                    }
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

        public static string ScreepsAPIUrl => ConfigSettingsState.ScreepsHttpsUrl;
        public static string ScreepsAPIHTTPUrl => ConfigSettingsState.ScreepsHttpUrl;
        public static string ScreepsAPIToken => ConfigSettingsState.ScreepsToken;

        private static async Task<(T? Result, HttpStatusCode Status)> ExecuteRequestAsync<T>(HttpMethod method, string path, StringContent? httpContent = null, bool isHistoryRequest = false)
        {
            string reqUrl;
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
                throw new ArgumentNullException(nameof(httpContent));
            }

            try
            {
                var retryCount = 0;
                HttpResponseMessage? response = null;
                if (isHistoryRequest)
                {
                    try
                    {
                        response = await _filesHttpClient.GetAsync(reqUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, reqUrl);
                        return (default, HttpStatusCode.InternalServerError);
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

                if (response?.IsSuccessStatusCode ?? false)
                {
                    var result = await JsonConvertHelper.ReadAndConvertStream<T>(response.Content);
                    var statusCode = response.StatusCode;
                    response?.Dispose(); // Properly dispose the response to return connection to pool
                    return (result, statusCode);
                }
                else
                {
                    var statusCode = response?.StatusCode ?? HttpStatusCode.InternalServerError;
                    response?.Dispose(); // Properly dispose the response to return connection to pool
                    return (default, statusCode);
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

            var (Result, _) = await ExecuteRequestAsync<SignInResponse>(HttpMethod.Post, path, body);
            return Result;
        }

        public static async Task<TimeResponse?> GetTimeOfShard(string shard)
        {
            var path = $"/api/game/time?shard={shard}";

            var (Result, _) = await ExecuteRequestAsync<TimeResponse>(HttpMethod.Get, path);
            return Result;
        }

        public static async Task<SeasonsReponse?> GetSeasons()
        {
            var path = $"/api/leaderboard/seasons";

            var (Result, _) = await ExecuteRequestAsync<SeasonsReponse>(HttpMethod.Get, path);
            return Result;
        }

        private static async Task<SeasonListResponse?> GetCurrentSeasonLeaderboard(string mode, string season, int offset, int limit)
        {
            var path = $"/api/leaderboard/list?limit={limit}&mode={mode}&offset={offset}&season={season}";

            var (Result, _) = await ExecuteRequestAsync<SeasonListResponse>(HttpMethod.Get, path);
            return Result;
        }
        public static async Task<(List<SeasonListItem> gcl, List<SeasonListItem> power)> GetCurrentSeasonLeaderboard()
        {
            var season = DateTime.Now.ToString("yyyy-MM");
            var gclLeaderboardList = new List<SeasonListItem>();
            var powerLeaderboardList = new List<SeasonListItem>();

            int offset = 0;
            int limit = 20;
            while (true)
            {
                var gclListResponse = await GetCurrentSeasonLeaderboard("world", season, offset, limit);
                var powerListResponse = await GetCurrentSeasonLeaderboard("power", season, offset, limit);
                var didSomething = false;
                if (gclListResponse != null && gclListResponse.List.Count > 0)
                {
                    gclLeaderboardList.AddRange(gclListResponse.List);
                    didSomething = true;
                }
                if (powerListResponse != null && powerListResponse.List.Count > 0)
                {
                    powerLeaderboardList.AddRange(powerListResponse.List);
                    didSomething = true;
                }
                if (!didSomething) break;
                offset += limit;
            }

            return (gclLeaderboardList.OrderBy(s => s.Rank).ToList(), powerLeaderboardList.OrderBy(s => s.Rank).ToList());
        }

        public static async Task<Dictionary<string, (List<SeasonListItem> gcl, List<SeasonListItem> power)>> GetAllSeasonsLeaderboard()
        {
            var leaderboardsList = new Dictionary<string, (List<SeasonListItem> gcl, List<SeasonListItem> power)>();

            var lastSeasonEmpty = false;
            var season = DateTime.Now.ToString("yyyy-MM");
            while (!lastSeasonEmpty)
            {
                var gclLeaderboardList = new List<SeasonListItem>();
                var powerLeaderboardList = new List<SeasonListItem>();

                int offset = 0;
                int limit = 20;
                while (true)
                {
                    var gclListResponse = await GetCurrentSeasonLeaderboard("world", season, offset, limit);
                    var powerListResponse = await GetCurrentSeasonLeaderboard("power", season, offset, limit);
                    var didSomething = false;
                    if (gclListResponse != null && gclListResponse.List.Count > 0)
                    {
                        gclLeaderboardList.AddRange(gclListResponse.List);
                        didSomething = true;
                    }
                    if (powerListResponse != null && powerListResponse.List.Count > 0)
                    {
                        powerLeaderboardList.AddRange(powerListResponse.List);
                        didSomething = true;
                    }
                    if (!didSomething) break;
                    offset += limit;
                }

                if (gclLeaderboardList.Count + powerLeaderboardList.Count == 0)
                {
                    lastSeasonEmpty = true;
                }
                else
                {
                    leaderboardsList.Add(season, (gclLeaderboardList, powerLeaderboardList));
                    season = DateTime.Parse(season, CultureInfo.InvariantCulture).AddMonths(-1).ToString("yyyy-MM", CultureInfo.InvariantCulture);
                }
            }


            return leaderboardsList;
        }

        public static async Task<SeasonListResponse?> GetLeaderboardsOfUser(string mode, string username)
        {
            var path = $"/api/leaderboard/find?mode={mode}&username={username}";

            var (Result, _) = await ExecuteRequestAsync<SeasonListResponse>(HttpMethod.Get, path);
            return Result;
        }

        public static async Task<ScreepsUser?> GetUser(string userId)
        {
            var path = $"/api/user/find?id={userId}";

            var (Result, Status) = await ExecuteRequestAsync<GetUserResponse>(HttpMethod.Get, path);
            return Status == HttpStatusCode.OK && Result?.Ok == 1 ? Result.User : null;
        }

        public static async Task SendConsoleExpression(string shard, string expression)
        {
            var content = new Dictionary<string, object>()
            {
                { "shard", shard },
                { "expression", expression }
            };
            var jsonString = JsonConvert.SerializeObject(content);
            var body = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var path = $"/api/user/console";
            var (_, _) = await ExecuteRequestAsync<OkResponse>(HttpMethod.Post, path, body);
        }

        public static async Task<(JObject? Result, HttpStatusCode Status)> GetHistory(string shard, string room, long tick)
        {
            var path = !ConfigSettingsState.ScreepsIsPrivateServer ? $"/room-history/{shard}/{room}/{tick}.json" : $"/room-history?room={room}&time={tick}";
            return await ExecuteRequestAsync<JObject>(HttpMethod.Get, path, isHistoryRequest: true);
        }

        public static async Task<AdminUtilsResponse?> GetAdminUtilsStats()
        {
            var path = "/stats";

            var (Result, _) = await ExecuteRequestAsync<AdminUtilsResponse>(HttpMethod.Get, path);
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

            var (Result, _) = await ExecuteRequestAsync<MapStatsResponse>(HttpMethod.Post, path, body);
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

            HashSet<string> rooms = [];
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

