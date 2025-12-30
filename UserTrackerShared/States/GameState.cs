using System.Collections.Concurrent;
using UserTrackerShared.DBClients;
using UserTrackerShared.Helpers;
using UserTrackerShared.Managers;
using UserTrackerShared.Models;
using UserTrackerShared.Utilities;
using Timer = System.Timers.Timer;

namespace UserTrackerShared.States
{
    public static class GameState
    {
        private static readonly Serilog.ILogger _leaderboardLogger = Logger.GetLogger(LogCategory.Leaderboard);
        public static List<ShardStateManager> Shards { get; set; } = new List<ShardStateManager>();
        public static ConcurrentDictionary<string, ScreepsUser> Users { get; set; } = new();

        public static async Task InitAsync()
        {
            bool isPrivateServer = ConfigSettingsState.ScreepsIsPrivateServer;
            if (isPrivateServer)
            {
                var signinResponse = await ScreepsAPI.SignIn(ConfigSettingsState.ScreepsUsername, ConfigSettingsState.ScreepsPassword) ?? throw new Exception("Failed to sign in");
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

            await UpdateUsersLeaderboard();
            var updateLeaderboardWorker = new CronWorker(
                "UpdateUsersLeaderboard",
                "* * * * *",
                OnUpdateUsersLeaderboardTimer);
            _ = updateLeaderboardWorker.StartAsync(new CancellationTokenSource().Token);


            if (ConfigSettingsState.GetAllUsers)
            {
                await GetAllUsers();

                var getAllUsersWorker = new CronWorker(
                    "GetAllUsers",
                    "0 0 0 1,11,21,31 * *",
                    OnGetAllUsersTimer);
                _ = getAllUsersWorker.StartAsync(new CancellationTokenSource().Token);

            }

            if (ConfigSettingsState.StartsShards)
            {
                foreach (var shard in Shards)
                {
                    shard.Start();
                }
            }
        }

        private static async Task<string?> GetUser(string userId)
        {
            var userResponse = await ScreepsAPI.GetUser(userId);
            if (userResponse != null)
            {
                Users.AddOrUpdate(userId, userResponse, (key, oldValue) => userResponse);
                return userResponse.Username;
            }
            return null;
        }
        private static async Task WriteAllUsers()
        {
            _leaderboardLogger.Information("Writing all users to database");
            foreach (var user in Users)
            {
                _leaderboardLogger.Information("Writing user {UserId} to database", user.Value.Username);
                await Task.Delay(10);
                await DBClient.WriteSingleUserData(user.Value);
            }
        }

        public static async Task OnGetAllUsersTimer(CancellationToken cancellationToken = default)
        {
            try
            {
                await GetAllUsers();
            }
            catch (Exception ex)
            {
                _leaderboardLogger.Error(ex, "Error occurred during scheduled GetAllUsers task");
            }
        }

        private static async Task OnUpdateUsersLeaderboardTimer(CancellationToken cancellationToken = default)
        {
            try
            {
                await UpdateUsersLeaderboard();
            }
            catch (Exception ex)
            {
                _leaderboardLogger.Error(ex, "Error occurred during scheduled UpdateUsersLeaderboard task");
            }
        }

        public static async Task GetAllUsers()
        {
            _leaderboardLogger.Information("Getting all users from leaderboard");
            var leaderboardsResponse = await ScreepsAPI.GetAllSeasonsLeaderboard();
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
            _leaderboardLogger.Information("Completed getting all users from leaderboard");
        }

        private static async Task UpdateUsersLeaderboard()
        {
            _leaderboardLogger.Information("Updating users leaderboard data");
            var userIdsUpdated = new HashSet<string>();

            var (gclLeaderboard, powerLeaderboard) = await ScreepsAPI.GetCurrentSeasonLeaderboard();
            _leaderboardLogger.Information("Fetched current season leaderboard data");
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
                else
                {
                    _leaderboardLogger.Warning("User {UserId} not found when updating GCL leaderboard", leaderboardSpot.UserName);
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
                else
                {
                    _leaderboardLogger.Warning("User {UserId} not found when updating GCL leaderboard", leaderboardSpot.UserName);
                }
            }
            _leaderboardLogger.Information("Updating user GCL and Power ranks");

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
            _leaderboardLogger.Information("Completed updating users leaderboard data");
        }
        private static async void OnUpdateAdminUtilsDataTimer()
        {
            _leaderboardLogger.Information("Updating admin utils data");
            var adminUtilsResponse = await ScreepsAPI.GetAdminUtilsStats();
            if (adminUtilsResponse != null)
            {
                DBClient.WriteAdminUtilsData(adminUtilsResponse);
            }
        }
    }
}
