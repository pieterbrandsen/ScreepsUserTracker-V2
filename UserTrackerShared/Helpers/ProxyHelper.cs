using System.Configuration;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UserTrackerShared.Helpers
{
    public static class ProxyHelper
    {
        public static string APIToken = ConfigurationManager.AppSettings["PROXYSCRAPE_API_TOKEN"] ?? "";
        public static List<string> Proxies;

        public static async Task<List<string>> GetProxyIps()
        {
            if (Proxies != null) return Proxies;

            var client = new HttpClient();
            var url = $"https://api.proxyscrape.com/v2/account/datacenter_shared/proxy-list?auth={APIToken}&type=getproxies&country[]=de&protocol=http&format=normal&status=all";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();

            Proxies = body.Split("\r\n").Where(w => !string.IsNullOrEmpty(w)).ToList();
            return Proxies;
        }
    }
}
