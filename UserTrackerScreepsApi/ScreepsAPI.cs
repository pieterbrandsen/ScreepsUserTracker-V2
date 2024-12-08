using System.Net;
using UserTrackerShared.Models.ScreepsAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Net.Http.Headers;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static UserTrackerShared.Models.ScreepsAPI.Season;
using UserTrackerShared;

namespace UserTrackerScreepsApi
{
    public class ScreepsAPI
    {
        public static string ScreepsAPIUrl = ConfigurationManager.AppSettings["SCREEPS_API_URL"] ?? "";
        public static string ScreepsAPIToken = ConfigurationManager.AppSettings["SCREEPS_API_TOKEN"] ?? "";


        private static Uri CombineUrl(string path)
        {
            return new Uri(ScreepsAPIUrl + path);
        }

        private static async Task<(T? Result, HttpStatusCode Status)> ExecuteRequestAsync<T>(HttpMethod method, string path, HttpContent? httpContent = null)
        {
            if (method == HttpMethod.Post && httpContent == null)
            {
                throw new ArgumentNullException("No HttpContent provided");
            }

            try
            {
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

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    T? result = JsonConvert.DeserializeObject<T>(responseContent);

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
            var content = new FormUrlEncodedContent(new Dictionary<string,string>());

            var (Result, Status) = await ExecuteRequestAsync<SignInResponse>(HttpMethod.Post, path, content);
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
    }
}
