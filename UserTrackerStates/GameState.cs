using System.Collections.Concurrent;
using System.Configuration;
using System.Timers;
using UserTrackerScreepsApi;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models.ScreepsAPI;
using UserTrackerStates;
using Timer = System.Timers.Timer;

namespace UserTrackerShared.States
{
    public static class GameState
    {
        public static string ScreepsAPIUrl = ConfigurationManager.AppSettings["SCREEPS_API_URL"] ?? "";
        public static string ScreepsAPIToken = ConfigurationManager.AppSettings["SCREEPS_API_TOKEN"] ?? "";
        public static string ScreepsAPIUsername = ConfigurationManager.AppSettings["SCREEPS_API_USERNAME"] ?? "";
        public static string ScreepsAPIPassword = ConfigurationManager.AppSettings["SCREEPS_API_PASSWORD"] ?? "";
        public static bool IsPrivateServer = ScreepsAPIUrl != "https://screeps.com";

        public static List<SeaonListItem> CurrentLeaderboard { get; set; } = new List<SeaonListItem>();
        public static ConcurrentBag<ProxyState> ProxyStates = new ConcurrentBag<ProxyState>();
        public static List<ShardState> Shards = new List<ShardState>();

        private static Timer? _onSetLeaderboardTimer;

        public static async void Init()
        {
            var proxies = await ProxyHelper.GetProxyIps();
            lock (ProxyStates)
            {
                foreach (var proxy in proxies)
                {
                    ProxyStates.Add(new ProxyState(new Uri($"http://{proxy}")));
                }
            }
            
            bool isPrivateServer = ScreepsAPIUrl != "https://screeps.com";
            if (isPrivateServer)
            {
                var signinReponse = await ScreepsAPI.SignIn(ScreepsAPIUsername, ScreepsAPIPassword);

                if (signinReponse == null)
                    throw new Exception("Failed to sign in");
                ConfigurationManager.AppSettings["SCREEPS_API_TOKEN"] = signinReponse.Token;

                Shards.Add(new ShardState(ConfigurationManager.AppSettings["SCREEPS_SHARDNAME"] ?? ""));
            }
            else
            {
                for (int i = 0; i <= 3; i++)
                {
                    Shards.Add(new ShardState($"shard{i}"));
                }
            }

            _onSetLeaderboardTimer = new Timer(300000);
            _onSetLeaderboardTimer.Elapsed += OnSetTimeTimer;
            _onSetLeaderboardTimer.AutoReset = true;
            _onSetLeaderboardTimer.Enabled = true;
            OnSetTimeTimer(null, null);
        }

        public static async Task<ProxyState> GetAvailableProxyAsync()
        {
            while (true)
            {
                foreach (var proxy in ProxyStates)
                {
                    if (!proxy.InUse)
                    {
                        proxy.SetUsed();
                        return proxy;
                    }
                }

                await Task.Delay(100);
            }
        }

        public static List<ProxyState> GetAvailableProxies(int max)
        {
            List<ProxyState> proxies = new List<ProxyState>();
            foreach (var proxy in ProxyStates)
            {
                if (!proxy.InUse)
                {
                    proxy.SetUsed();
                    proxies.Add(proxy);
                    if (proxies.Count >= max)
                    {
                        break;
                    }
                }
            }
            return proxies;
        }

        private static async void OnSetTimeTimer(Object? source, ElapsedEventArgs e)
        {
            var currentLeaderboardResponse = await ScreepsAPI.GetCurrentSeasonLeaderboard("world");
            if (currentLeaderboardResponse != null)
            {
                CurrentLeaderboard = currentLeaderboardResponse;
            }
        }
    }
}
