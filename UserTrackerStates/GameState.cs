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

        public static List<SeaonListItem> CurrentLeaderboard { get; set; } = new List<SeaonListItem>();
        public static List<ShardState> Shards = new List<ShardState>();
        public static Dictionary<string, ScreepsUser> Users = new();

        private static Timer? _onSetLeaderboardTimer;
        private static Timer? _onSetAdminUtilsDataTimer;

        public static async Task InitAsync()
        {
            bool isPrivateServer = ConfigSettingsState.ScreepsIsPrivateServer;
            if (isPrivateServer)
            {
                var signinReponse = await ScreepsAPI.SignIn(ConfigSettingsState.ScreepsUsername, ConfigSettingsState.ScreepsPassword);

                if (signinReponse == null)
                    throw new Exception("Failed to sign in");
                ConfigSettingsState.ScreepsToken = signinReponse.Token;

                Shards.Add(new ShardState(ConfigSettingsState.ScreepsShardName));

                OnUpdateAdminUtilsDataTimer(null, null);
                _onSetAdminUtilsDataTimer = new Timer(60 * 1000);
                _onSetAdminUtilsDataTimer.AutoReset = true;
                _onSetAdminUtilsDataTimer.Enabled = true;
                _onSetAdminUtilsDataTimer.Elapsed += OnUpdateAdminUtilsDataTimer;
            }
            else
            {
                for (int i = 0; i <= 3; i++)
                {
                    Shards.Add(new ShardState($"shard{i}"));
                }
            }


            _onSetLeaderboardTimer = new Timer(1000);
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

        private static async Task<string?> GetUser(string userId)
        {
            var userResponse = await ScreepsAPI.GetUser(userId);
            if (userResponse != null)
            {
                Users[userId] = userResponse;
                return userResponse.Username;
            }
            return null;
        }
        private static void WriteAllUsers()
        {
            foreach (var user in Users)
            {
                DBClient.WriteSingleUserData(user.Value);
            }
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
                    await GetUser(leaderboardSpot.UserId);
                    Users[leaderboardSpot.UserId].GCLRank = leaderboardSpot.Rank;
                }
                WriteAllUsers();
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
                            await GetUser(leaderboardSpot.UserId);
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

            var gclSorted = Users.Values.OrderByDescending(x => x.GCL).ToList();
            var powerSorted = Users.Values.OrderByDescending(x => x.Power).ToList();
            foreach (var user in Users)
            {
                user.Value.GCLRank = gclSorted.FindIndex(x => x.Id == user.Value.Id) + 1;
                user.Value.PowerRank = powerSorted.FindIndex(x => x.Id == user.Value.Id) + 1;
            }
            WriteAllUsers();
        }
        private static async void OnUpdateAdminUtilsDataTimer(Object? source, ElapsedEventArgs e)
        {
            var adminUtilsResponse = await ScreepsAPI.GetAdminUtilsStats();
            if (adminUtilsResponse != null)
            {
                DBClient.WriteAdminUtilsData(adminUtilsResponse);
            }
        }
    }
}
