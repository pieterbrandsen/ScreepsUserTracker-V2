using System.Timers;
using UserTrackerShared.DBClients;
using UserTrackerShared.Managers;
using UserTrackerShared.Models;
using Timer = System.Timers.Timer;

namespace UserTrackerShared.States
{
    public static class GameState
    {
        public static List<ShardStateManager> Shards { get; set; } = new List<ShardStateManager>();
        public static Dictionary<string, ScreepsUser> Users { get; set; } = new();

        public static async Task InitAsync()
        {
            bool isPrivateServer = ConfigSettingsState.ScreepsIsPrivateServer;
            if (isPrivateServer)
            {
                var signinReponse = await ScreepsApi.SignIn(ConfigSettingsState.ScreepsUsername, ConfigSettingsState.ScreepsPassword);

                if (signinReponse == null)
                    throw new Exception("Failed to sign in");
                ConfigSettingsState.ScreepsToken = signinReponse.Token;

                Shards.Add(new ShardStateManager(ConfigSettingsState.ScreepsShardName));

                OnUpdateAdminUtilsDataTimer();
                var onSetAdminUtilsDataTimer = new Timer(60 * 1000);
                onSetAdminUtilsDataTimer.AutoReset = true;
                onSetAdminUtilsDataTimer.Enabled = true;
                onSetAdminUtilsDataTimer.Elapsed += (s, e) => OnUpdateAdminUtilsDataTimer();
            }
            else
            {
                for (int i = 0; i <= 3; i++)
                {
                    Shards.Add(new ShardStateManager($"shard{i}"));
                }
            }


            var onSetLeaderboardTimer = new Timer(60 * 60 * 1000);
            onSetLeaderboardTimer.AutoReset = true;
            onSetLeaderboardTimer.Enabled = true;
            onSetLeaderboardTimer.Elapsed += OnUpdateUsersLeaderboardTimer;

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
            var userResponse = await ScreepsApi.GetUser(userId);
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

        public static async Task GetAllUsers()
        {
            var LeaderboardsResponse = await ScreepsApi.GetAllSeasonsLeaderboard();
            if (LeaderboardsResponse != null)
            {
                foreach (var leaderboard in LeaderboardsResponse.Select(kv=>kv.Value))
                {
                    var gclLeaderbard = leaderboard.gcl;
                    foreach (var leaderboardSpot in gclLeaderbard)
                    {
                        if (!Users.TryGetValue(leaderboardSpot.UserId, out ScreepsUser? value))
                        {
                            await GetUser(leaderboardSpot.UserId);
                            Users.TryGetValue(leaderboardSpot.UserId, out value); // Retry after attempting to fetch
                        }

                        if (value != null)
                        {
                            leaderboardSpot.UserName = value.Username;
                            leaderboardSpot.Type = "gcl";
                            DBClient.WriteLeaderboardData(leaderboardSpot);
                        }
                    }

                    var powerLeaderboard = leaderboard.power;
                    foreach (var leaderboardSpot in powerLeaderboard)
                    {
                        if (!Users.TryGetValue(leaderboardSpot.UserId, out ScreepsUser? value))
                        {
                            await GetUser(leaderboardSpot.UserId);
                            Users.TryGetValue(leaderboardSpot.UserId, out value); // Retry after attempting to fetch
                        }

                        if (value != null)
                        {
                            leaderboardSpot.UserName = value.Username;
                            leaderboardSpot.Type = "power";
                            DBClient.WriteLeaderboardData(leaderboardSpot);
                        }
                    }
                }
            }
        }

        private static async void OnUpdateUsersLeaderboardTimer(Object? source, ElapsedEventArgs e)
        {
            foreach (var user in Users)
            {
                await GetUser(user.Key);
            }

            var gclSorted = Users.Values.OrderByDescending(x => x.GCL).ToList();
            var powerSorted = Users.Values.OrderByDescending(x => x.Power).ToList();
            int gclRank = 1;
            foreach (var group in gclSorted.GroupBy(x => x.GCL))
            {
                foreach (var user in group)
                {
                    user.GCLRank = gclRank;
                }
                gclRank += group.Count();
            }

            int powerRank = 1;
            foreach (var group in powerSorted.GroupBy(x => x.Power))
            {
                foreach (var user in group)
                {
                    user.PowerRank = powerRank;
                }
                powerRank += group.Count();
            }
            WriteAllUsers();
        }
        private static async void OnUpdateAdminUtilsDataTimer()
        {
            var adminUtilsResponse = await ScreepsApi.GetAdminUtilsStats();
            if (adminUtilsResponse != null)
            {
                DBClient.WriteAdminUtilsData(adminUtilsResponse);
            }
        }
    }
}
