using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.IO.Compression;
using System.Net;
using System.Text;
using UserTrackerShared;
using UserTrackerShared.Models.ScreepsAPI;

namespace UserTrackerScreepsApi
{
    internal static class JSONConvertHelper
    {
        public async static Task<T?> DeserializeObject<T>(HttpContent httpContent, string path)
        {
            var encoding = httpContent.Headers.ContentEncoding.ToString();
            byte[] responseData = await httpContent.ReadAsByteArrayAsync();

            var json = "";
            if (encoding != null && encoding.Contains("gzip"))
            {
                using var decompressedStream = new GZipStream(new MemoryStream(responseData), CompressionMode.Decompress);
                using var reader = new StreamReader(decompressedStream, Encoding.UTF8);
                json = await reader.ReadToEndAsync();
            }
            else
            {
                json = Encoding.UTF8.GetString(responseData);
            }

            JsonSerializerSettings settings = new JsonSerializerSettings();
            try
            {
                T? result = JsonConvert.DeserializeObject<T>(json, settings);
                return result;
            }
            catch (Exception ex)
            {
                Screen.LogsPart.AddLog(path);
                Console.Error.WriteLine(ex);
                return default(T);
            }
        }

    }
    public class ScreepsAPI
    {
        public static string ScreepsAPIUrl = ConfigurationManager.AppSettings["SCREEPS_API_URL"] ?? "";
        public static string ScreepsAPIToken = ConfigurationManager.AppSettings["SCREEPS_API_TOKEN"] ?? "";

        private static bool InUse = false;

        private static Uri CombineUrl(string path)
        {
            return new Uri(ScreepsAPIUrl + path);
        }

        private static async Task<(T? Result, HttpStatusCode Status)> ExecuteRequestAsync<T>(HttpMethod method, string path, StringContent? httpContent = null)
        {
            if (method == HttpMethod.Post && httpContent == null)
            {
                throw new ArgumentNullException("No HttpContent provided");
            }

            try
            {
                if (path.StartsWith("/api"))
                {
                    while (InUse)
                    {
                        Thread.Sleep(10);
                    }
                    InUse = true;
                }

                using HttpClient client = new();

                var request = new HttpRequestMessage()
                {
                    RequestUri = CombineUrl(path),
                    Method = method,
                };
                if (httpContent != null)
                {
                    request.Content = httpContent;
                }

                request.Headers.Add("X-Token", ScreepsAPIToken);
                request.Headers.Add("X-Username", ScreepsAPIToken);


                var response = await client.SendAsync(request);
                Screen.LogsPart.AddLog($"{path} - {response.StatusCode}");

                if (path.StartsWith("/api"))
                {
                    InUse = false;
                }
                if (response.IsSuccessStatusCode)
                {
                    var result = await JSONConvertHelper.DeserializeObject<T>(response.Content, path);
                    return (result, response.StatusCode);
                }
                else
                {
                    return (default, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
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

            return leaderboardList;
        }

        public static async Task<SeasonListResponse?> GetLeaderboardsOfUser(string mode, string username)
        {
            var path = $"/api/leaderboard/find?mode={mode}&username={username}";

            var (Result, Status) = await ExecuteRequestAsync<SeasonListResponse>(HttpMethod.Get, path);
            return Result;
        }

        public static async Task<JObject?> GetHistory(string shard, string room, long tick)
        {
            var path = $"/room-history{(!string.IsNullOrEmpty(shard) ? $"/{shard}" : "")}/{room}/{tick}.json";

            var (Result, Status) = await ExecuteRequestAsync<JObject>(HttpMethod.Get, path);
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

