using System.Net;

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

        private async Task<HttpStatusCode> ExecuteGetRequest(string path)
        {
            try
            {
                HttpClient client = new();

                var response = await client.GetAsync(CombineUrl(path));
                return response.StatusCode;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<HttpStatusCode> ExecutePostRequest(string path, HttpContent httpContent)
        {
            try
            {
                HttpClient client = new();

                var response = await client.PostAsync(CombineUrl(path), httpContent);
                return response.StatusCode;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task SignIn(string username, string password)
        {
            var path = "/api/auth/signin";

            var values = new Dictionary<string, string>
            {
                { "email", username },
                { "password", password }
            };

            var content = new FormUrlEncodedContent(values);

            var response = await ExecutePostRequest(path, content);
        }

        public void GetTimeOfShard(string shard)
        {
            var path = "/";
        }
    }
}
