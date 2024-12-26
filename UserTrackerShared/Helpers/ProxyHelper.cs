using System.Configuration;

namespace UserTrackerShared.Helpers
{
    public static class ProxyHelper
    {
        public static string APIToken = ConfigurationManager.AppSettings["PROXYSCRAPE_API_TOKEN"] ?? "";
        public static async Task<List<string>> GetProxyIps()
        {
            var client = new HttpClient();
            var url = $"https://api.proxyscrape.com/v2/account/datacenter_shared/proxy-list?auth={APIToken}&type=getproxies&country[]=de&protocol=http&format=normal&status=all";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return body.Split("\r\n").ToList().Where(w => !string.IsNullOrEmpty(w)).ToList();
        }
    }
}
