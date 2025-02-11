using System.Collections.Concurrent;
using System.Configuration;
using System.Timers;
using UserTrackerScreepsApi;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerShared.Models.ScreepsAPI;
using UserTrackerStates;
using Timer = System.Timers.Timer;

namespace UserTrackerShared.States
{
    public static class GameState
    {
        public static string ScreepsAPIUrl = "";
        public static string ScreepsAPIToken = "";
        public static string ScreepsAPIUsername = "";
        public static string ScreepsAPIPassword = "";

        public static List<SeaonListItem> CurrentLeaderboard { get; set; } = new List<SeaonListItem>();
        public static List<ShardState> Shards = new List<ShardState>();
        public static Dictionary<string, ScreepsUser> Users= new ();

        private static Timer? _onSetLeaderboardTimer;

        public static async Task InitAsync()
        {
            ScreepsAPIUrl = ConfigurationManager.AppSettings["SCREEPS_API_URL"] ?? "";
            ScreepsAPIToken = ConfigurationManager.AppSettings["SCREEPS_API_TOKEN"] ?? "";
            ScreepsAPIUsername = ConfigurationManager.AppSettings["SCREEPS_API_USERNAME"] ?? "";
            ScreepsAPIPassword = ConfigurationManager.AppSettings["SCREEPS_API_PASSWORD"] ?? "";

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

            await UpdateCurrentLeaderboard();
            await GetAllUsers();

            if (ConfigSettingsState.StartsShards)
            {
                foreach (var shard in Shards)
                {
                    await shard.StartAsync();
                }
            }


            _onSetLeaderboardTimer = new Timer(300000);
            _onSetLeaderboardTimer.Elapsed += OnSetTimeTimer;
            _onSetLeaderboardTimer.AutoReset = true;
            _onSetLeaderboardTimer.Enabled = true;
        }

        public static async Task UpdateCurrentLeaderboard()
        {
            var currentLeaderboardResponse = await ScreepsAPI.GetCurrentSeasonLeaderboard("world");
            if (currentLeaderboardResponse != null)
            {
                CurrentLeaderboard = currentLeaderboardResponse;
                foreach (var leaderboardSpot in CurrentLeaderboard)
                {
                    if (Users.ContainsKey(leaderboardSpot.UserId)) continue;
                    var userResponse = await ScreepsAPI.GetUser(leaderboardSpot.UserId);
                    if (userResponse != null) {
                        Users[leaderboardSpot.UserId] = userResponse;
                        leaderboardSpot.UserName = userResponse.Username;
                    }
                }
            }
        }

        public static async Task GetAllUsers()
        {
            var LeaderboardsResponse = await ScreepsAPI.GetAllSeasonsLeaderboard("world");
            if (LeaderboardsResponse != null)
            {
                foreach (var leaderboardKVP in LeaderboardsResponse)
                {
                    foreach (var leaderboardSpot in leaderboardKVP.Value)
                    {
                        if (Users.ContainsKey(leaderboardSpot.UserId)) continue;
                        var userResponse = await ScreepsAPI.GetUser(leaderboardSpot.UserId);
                        if (userResponse != null)
                        {
                            Users[leaderboardSpot.UserId] = userResponse;
                            leaderboardSpot.UserName = userResponse.Username;
                        }
                    }
                }
            }
        }

        private static async void OnSetTimeTimer(Object? source, ElapsedEventArgs e)
        {
            await UpdateCurrentLeaderboard();
        }
    }
}
