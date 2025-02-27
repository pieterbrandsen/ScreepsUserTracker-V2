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
        public static Dictionary<string, ScreepsUser> Users = new();

        private static Timer? _onSetLeaderboardTimer;

        public static async Task InitAsync()
        {
            ScreepsAPIUrl = ConfigurationManager.AppSettings["SCREEPS_API_HTTPS_URL"] ?? "";
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


            _onSetLeaderboardTimer = new Timer(60 * 60 * 1000);
            _onSetLeaderboardTimer.AutoReset = true;
            _onSetLeaderboardTimer.Enabled = true;
            if (ConfigSettingsState.LoadSeasonalLeaderboard)
            {
                await UpdateCurrentLeaderboard();
                _onSetLeaderboardTimer.Elapsed += OnUpdateSeasonalLeaderboardTimer;
            }
            else
            {
                _onSetLeaderboardTimer.Elapsed += OnUpdateUsersLeaderboardTimer;
            }
            if (ConfigSettingsState.GetAllUsers) await GetAllUsers();
            if (ConfigSettingsState.StartsShards)
            {
                foreach (var shard in Shards)
                {
                    await shard.StartAsync();
                }
            }
        }

        private static async Task<string?> GetUser(string userId, int rank = 0)
        {
            var userResponse = await ScreepsAPI.GetUser(userId);
            if (userResponse != null)
            {
                if (rank > 0) userResponse.Rank = rank;
                Users[userId] = userResponse;

                DBClient.WriteSingleUserdData(userResponse);
                return userResponse.Username;
            }
            return null;
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
                    await GetUser(leaderboardSpot.UserId, leaderboardSpot.Rank);
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
                        if (!Users.TryGetValue(leaderboardSpot.UserId, out ScreepsUser? value))
                        {
                            await GetUser(leaderboardSpot.UserId, leaderboardSpot.Rank);
                            Users.TryGetValue(leaderboardSpot.UserId, out value); // Retry after attempting to fetch
                        }

                        if (value != null)
                        {
                            leaderboardSpot.UserName = value.Username;
                            DBClient.WriteLeaderboardData(leaderboardSpot);
                        }
                    }
                }
            }
        }

        private static async void OnUpdateSeasonalLeaderboardTimer(Object? source, ElapsedEventArgs e)
        {
            await UpdateCurrentLeaderboard();
        }
        private static async void OnUpdateUsersLeaderboardTimer(Object? source, ElapsedEventArgs e)
        {
            foreach (var user in Users)
            {
                await GetUser(user.Key);
            }
        }
    }
}
