using System.Collections.Concurrent;
using System.Timers;
using UserTrackerShared.DBClients;
using UserTrackerShared.Helpers;
using UserTrackerShared.Managers;
using UserTrackerShared.Models;
using Timer = System.Timers.Timer;

namespace UserTrackerShared.States
{
    public static class GameState
    {
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.States);
        public static List<ShardStateManager> Shards { get; set; } = new List<ShardStateManager>();
        public static ConcurrentDictionary<string, ScreepsUser> Users { get; set; } = new();

        public static async Task InitAsync()
        {
            bool isPrivateServer = ConfigSettingsState.ScreepsIsPrivateServer;
            if (isPrivateServer)
            {
                var signinResponse = await ScreepsApi.SignIn(ConfigSettingsState.ScreepsUsername, ConfigSettingsState.ScreepsPassword);

                if (signinResponse == null)
                    throw new Exception("Failed to sign in");
                ConfigSettingsState.ScreepsToken = signinResponse.Token;

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

            if (ConfigSettingsState.GetAllUsers) await GetAllUsers();
            if (ConfigSettingsState.StartsShards)
            {
                foreach (var shard in Shards)
                {
                    shard.Start();
                }
            }

            var onSetLeaderboardTimer = new TimerScheduleHelper(
                "UpdateUsersLeaderboard",
                OnUpdateUsersLeaderboardTimer,
                [0, 6, 12, 18]);
            var onGetAllUsersTimer = new TimerScheduleHelper(
                "GetAllUsers",
                OnGetAllUsersTimer,
                [0],
                daysOfMonth: [3, 10]);
        }

        private static async Task<string?> GetUser(string userId)
        {
            var userResponse = await ScreepsApi.GetUser(userId);
            if (userResponse != null)
            {
                Users.AddOrUpdate(userId, userResponse, (key, oldValue) => userResponse);
                return userResponse.Username;
            }
            return null;
        }
        private static async Task WriteAllUsers()
        {
            foreach (var user in Users)
            {
                await DBClient.WriteSingleUserData(user.Value);
            }
        }

        public static async void OnGetAllUsersTimer()
        {
            await GetAllUsers();
        }

        public static async Task GetAllUsers()
        {
            _logger.Information("Getting all users from leaderboard");
            var leaderboardsResponse = await ScreepsApi.GetAllSeasonsLeaderboard();
            if (leaderboardsResponse != null)
            {
                var seasons = leaderboardsResponse.Select(kv => kv.Key).OrderDescending().ToList();
                var currentSeason = seasons.FirstOrDefault();

                var leaderboardList = leaderboardsResponse.Where(kv => kv.Key != currentSeason).Select(kv => kv.Value).ToList();
                foreach (var (gclLeaderboard, powerLeaderboard) in leaderboardList)
                {
                    foreach (var leaderboardSpot in gclLeaderboard)
                    {
                        if (!Users.TryGetValue(leaderboardSpot.UserId, out ScreepsUser? value))
                        {
                            await GetUser(leaderboardSpot.UserId);
                            Users.TryGetValue(leaderboardSpot.UserId, out value); // Retry after attempting to fetch
                        }

                        if (value != null)
                        {
                            leaderboardSpot.Rank += 1;
                            leaderboardSpot.UserName = value.Username;
                            leaderboardSpot.Type = "gcl";
                            await DBClient.WriteHistoricalLeaderboardData(leaderboardSpot);
                        }
                    }

                    foreach (var leaderboardSpot in powerLeaderboard)
                    {
                        if (!Users.TryGetValue(leaderboardSpot.UserId, out ScreepsUser? value))
                        {
                            await GetUser(leaderboardSpot.UserId);
                            Users.TryGetValue(leaderboardSpot.UserId, out value); // Retry after attempting to fetch
                        }

                        if (value != null)
                        {
                            leaderboardSpot.Rank += 1;
                            leaderboardSpot.UserName = value.Username;
                            leaderboardSpot.Type = "power";
                            await DBClient.WriteHistoricalLeaderboardData(leaderboardSpot);
                        }
                    }
                }
            }
        }

        private static async void OnUpdateUsersLeaderboardTimer()
        {
            _logger.Information("Updating users leaderboard data");
            var userIdsUpdated = new HashSet<string>();

            var (gclLeaderboard, powerLeaderboard) = await ScreepsApi.GetCurrentSeasonLeaderboard();
            foreach (var leaderboardSpot in gclLeaderboard)
            {
                if (!Users.TryGetValue(leaderboardSpot.UserId, out ScreepsUser? value) || !userIdsUpdated.Contains(leaderboardSpot.UserId))
                {
                    await GetUser(leaderboardSpot.UserId);
                    Users.TryGetValue(leaderboardSpot.UserId, out value);
                    userIdsUpdated.Add(leaderboardSpot.UserId);
                }

                if (value != null)
                {
                    leaderboardSpot.Rank += 1;
                    leaderboardSpot.UserName = value.Username;
                    leaderboardSpot.Type = "gcl";
                    await DBClient.WriteCurrentLeaderboardData(leaderboardSpot);
                }
            }

            foreach (var leaderboardSpot in powerLeaderboard)
            {
                if (!Users.TryGetValue(leaderboardSpot.UserId, out ScreepsUser? value) || !userIdsUpdated.Contains(leaderboardSpot.UserId))
                {
                    await GetUser(leaderboardSpot.UserId);
                    Users.TryGetValue(leaderboardSpot.UserId, out value);
                    userIdsUpdated.Add(leaderboardSpot.UserId);
                }

                if (value != null)
                {
                    leaderboardSpot.Rank += 1;
                    leaderboardSpot.UserName = value.Username;
                    leaderboardSpot.Type = "power";
                    await DBClient.WriteCurrentLeaderboardData(leaderboardSpot);
                }
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
            await WriteAllUsers();
        }
        private static async void OnUpdateAdminUtilsDataTimer()
        {
            _logger.Information("Updating admin utils data");
            var adminUtilsResponse = await ScreepsApi.GetAdminUtilsStats();
            if (adminUtilsResponse != null)
            {
                DBClient.WriteAdminUtilsData(adminUtilsResponse);
            }
        }
    }
}
