using System.Collections.Concurrent;
using System.Configuration;
using System.Timers;
using UserTrackerScreepsApi;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerShared.Models.ScreepsAPI;
using UserTrackerStates;
using UserTrackerStates.DBClients;
using Timer = System.Timers.Timer;

namespace UserTrackerShared.States
{
    public static class GameState
    {
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.States);

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

            if (ConfigSettingsState.LoadSeasonalLeaderboard) await UpdateCurrentLeaderboard();
            if (ConfigSettingsState.GetAllUsers) await GetAllUsers();

            if (ConfigSettingsState.StartsShards)
            {
                foreach (var shard in Shards)
                {
                    await shard.StartAsync();
                }
            }


            _onSetLeaderboardTimer = new Timer(60*60*1000);
            _onSetLeaderboardTimer.Elapsed += OnSetTimeTimer;
            _onSetLeaderboardTimer.AutoReset = true;
            _onSetLeaderboardTimer.Enabled = true;
        }

        public static async Task UpdateCurrentLeaderboard()
        {
            var currentLeaderboardResponse = await ScreepsAPI.GetCurrentSeasonLeaderboard("world");

            if (currentLeaderboardResponse != null)
            {
                _logger.Information("Started updating current leaderboard");
                CurrentLeaderboard = currentLeaderboardResponse;
                foreach (var leaderboardSpot in CurrentLeaderboard)
                {
                    var userResponse = await ScreepsAPI.GetUser(leaderboardSpot.UserId);
                    if (userResponse != null) {
                        userResponse.Rank = leaderboardSpot.Rank;
                        Users[leaderboardSpot.UserId] = userResponse;
                        
                        leaderboardSpot.UserName = userResponse.Username;
                        DBClient.WriteSingleUserdData(userResponse);
                    }
                }
            }
            else
            {
                _logger.Information("Failed to do api to update current leaderboard");
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
                        if (!Users.ContainsKey(leaderboardSpot.UserId)) {
                            var userResponse = await ScreepsAPI.GetUser(leaderboardSpot.UserId);
                            if (userResponse != null)
                            {
                                Users[leaderboardSpot.UserId] = userResponse;
                                leaderboardSpot.UserName = userResponse.Username;
                            }
                        }

                        DBClient.WriteLeaderboardData(leaderboardSpot);
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
