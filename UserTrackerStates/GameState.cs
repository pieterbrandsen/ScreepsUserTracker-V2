using System.Configuration;
using System.Timers;
using UserTrackerScreepsApi;
using UserTrackerShared.Models.ScreepsAPI;
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

        public static List<SeaonListItem> CurrentLeaderboard { get; set; }

        public static List<ShardState> Shards = new List<ShardState>();
        private static Timer? _onSetLeaderboardTimer;

        public static async void Init()
        {
            // debug purposes only
            for (int i = 0; i < 50; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    for (int k = 0; k < 10; k++)
                    {
                        await ScreepsAPI.GetHistory("shard0", $"E{i}N{j}", 64920600 - (k * 100));
                        await Task.Delay(500);

                        await ScreepsAPI.GetHistory("shard0", $"E{i}S{j}", 64920600 - (k * 100));
                        await Task.Delay(500);

                        await ScreepsAPI.GetHistory("shard0", $"W{i}N{j}", 64920600 - (k * 100));
                        await Task.Delay(500);

                        await ScreepsAPI.GetHistory("shard0", $"W{i}S{j}", 64920600 - (k * 100));
                        await Task.Delay(500);
                    }
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
