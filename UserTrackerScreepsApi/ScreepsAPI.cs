using System.Net;
using UserTrackerShared.Models.ScreepsAPI;
using Newtonsoft.Json;

namespace UserTrackerScreepsApi
{
    public class ScreepsAPI
    {
        public ScreepsAPI(string apiUrl)
        {
            this.apiUrl = apiUrl;
        }

        private readonly string apiUrl;


        private Uri CombineUrl(string path)
        {
            return new Uri(apiUrl + path);
        }

        private async Task<(T? Result, HttpStatusCode Status)> ExecuteGetRequestAsync<T>(string path)
        {
            try
            {
                using HttpClient client = new();

                var response = await client.GetAsync(CombineUrl(path));

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

        private async Task<(T? Result, HttpStatusCode Status)> ExecutePostRequestAsync<T>(string path, HttpContent httpContent)
        {
            try
            {
                using HttpClient client = new();

                var response = await client.PostAsync(CombineUrl(path), httpContent);

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

        public async Task<SignInResponse?> SignIn(string username, string password)
        {
            var path = $"/api/auth/signin?email={username}&password={password}";
            var content = new FormUrlEncodedContent(new Dictionary<string,string>());

            var (Result, Status) = await ExecutePostRequestAsync<SignInResponse>(path, content);
            return Result;
        }

        public async Task<TimeResponse?> GetTimeOfShard(string shard)
        {
            var path = $"/api/game/time?shard={shard}";

            var (Result, Status) = await ExecuteGetRequestAsync<TimeResponse>(path);
            return Result;
        }
    }
}
