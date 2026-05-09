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
        private static readonly SemaphoreSlim _leaderboardWorkSemaphore = new SemaphoreSlim(1, 1);
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

                // _ = OnUpdateAdminUtilsDataTimerAsync();
                // var onSetAdminUtilsDataTimer = new Timer(60 * 1000);
                // onSetAdminUtilsDataTimer.AutoReset = true;
                // onSetAdminUtilsDataTimer.Enabled = true;
                // onSetAdminUtilsDataTimer.Elapsed += (s, e) => _ = OnUpdateAdminUtilsDataTimerAsync();
            }
            else if (!string.IsNullOrEmpty(ConfigSettingsState.ScreepsShardName))
            {
                Shards.Add(new ShardStateManager(ConfigSettingsState.ScreepsShardName));
            }
            else
            {
                for (int i = 0; i <= 3; i++)
                {
                    Shards.Add(new ShardStateManager($"shard{i}"));
                }
            }

            if (ConfigSettingsState.GetAllUsers)
            {
                await UpdateUsersLeaderboard();
                var updateUsersLeaderboardCron = isPrivateServer ? "*/2 * * * *" : "0 */6 * * *";
                var updateLeaderboardWorker = new CronWorker(
                    "UpdateUsersLeaderboard",
                    updateUsersLeaderboardCron,
                    OnUpdateUsersLeaderboardTimer);
                _ = updateLeaderboardWorker.StartAsync(new CancellationTokenSource().Token);

                await GetAllUsers();

                var getAllUsersWorker = new CronWorker(
                    "GetAllUsers",
                    "0 0 1,11,21,31 * *",
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

        internal static async Task<int> MergeUsersBackfillingGclAsync(
            IReadOnlyDictionary<string, ScreepsUser>? usersById,
            Func<string, Task<ScreepsUser?>>? findUserById = null)
        {
            if (usersById == null || usersById.Count == 0)
            {
                return 0;
            }

            findUserById ??= ScreepsAPI.GetUser;

            var mergedUsers = 0;
            foreach (var (userId, user) in usersById)
            {
                if (string.IsNullOrWhiteSpace(userId) || user == null || string.IsNullOrWhiteSpace(user.Username))
                {
                    continue;
                }

                var userToMerge = user;
                if (user.GCL == 0)
                {
                    try
                    {
                        var foundUser = await findUserById(userId);
                        if (foundUser != null && !string.IsNullOrWhiteSpace(foundUser.Username))
                        {
                            userToMerge = foundUser;
                        }
                    }
                    catch (Exception ex)
                    {
                        _leaderboardLogger.Warning(ex, "Failed to backfill GCL for user {UserId}", userId);
                    }
                }

                if (string.IsNullOrWhiteSpace(userToMerge.Id))
                {
                    userToMerge.Id = userId;
                }

                Users.AddOrUpdate(userId, userToMerge, (key, oldValue) => userToMerge);
                mergedUsers++;
            }

            return mergedUsers;
        }

        private static void UpdateCachedUserRanks()
        {
            _leaderboardLogger.Information("Updating user GCL, Power and Score ranks from in-memory users");
            var gclSorted = Users.Values.OrderByDescending(x => x.GCL).ToList();
            var powerSorted = Users.Values.OrderByDescending(x => x.Power).ToList();
            var scoreSorted = Users.Values.OrderByDescending(x => x.Score).ToList();

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

            int scoreRank = 1;
            foreach (var group in scoreSorted.GroupBy(x => x.Score))
            {
                foreach (var user in group)
                {
                    user.ScoreRank = scoreRank;
                }
                scoreRank += group.Count();
            }
        }

        private static async Task<int> PersistUsersFromCache()
        {
            _leaderboardLogger.Information("Persisting users from in-memory cache (no user re-fetch)");
            UpdateCachedUserRanks();
            var userIds = Users.Keys.Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
            var written = await WriteUsersByIds(userIds);
            _leaderboardLogger.Information("Completed users cache persist with {UserCount} users written", written);
            return written;
        }

        internal static IReadOnlyList<ScreepsUser> GetUsersForWrite(IEnumerable<string> userIds)
        {
            var usersToWrite = new List<ScreepsUser>();
            var handledUserIds = new HashSet<string>();

            foreach (var userId in userIds)
            {
                if (string.IsNullOrWhiteSpace(userId) || !handledUserIds.Add(userId))
                {
                    continue;
                }

                if (Users.TryGetValue(userId, out var user))
                {
                    usersToWrite.Add(user);
                }
            }

            return usersToWrite;
        }

        internal static async Task<int> WriteUsersByIds(
            IEnumerable<string> userIds,
            Func<ScreepsUser, Task>? writeSingleUserData = null)
        {
            var usersToWrite = GetUsersForWrite(userIds);
            var writeUserData = writeSingleUserData ?? DBClient.WriteSingleUserData;

            _leaderboardLogger.Information("Writing {UserCount} updated users to database", usersToWrite.Count);
            foreach (var user in usersToWrite)
            {
                _leaderboardLogger.Information("Writing user {UserId} to database", user.Username);
                await Task.Delay(10);
                await writeUserData(user);
            }

            return usersToWrite.Count;
        }

        internal static async Task<bool> TryRunWithLeaderboardLock(string jobName, Func<Task> job)
        {
            if (!await _leaderboardWorkSemaphore.WaitAsync(0))
            {
                _leaderboardLogger.Warning("Skipping {JobName}: another leaderboard-heavy job is already running", jobName);
                return false;
            }

            try
            {
                await job();
                return true;
            }
            finally
            {
                _leaderboardWorkSemaphore.Release();
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
            await TryRunWithLeaderboardLock("GetAllUsers", GetAllUsersCore);
        }

        private static async Task GetAllUsersCore()
        {
            if (ConfigSettingsState.ScreepsIsPrivateServer)
            {
                _leaderboardLogger.Information("Private server detected. Skipping leaderboard users pull and reusing in-memory users.");
                await PersistUsersFromCache();
                return;
            }

            _leaderboardLogger.Information("Getting all users from leaderboard");
            var leaderboardsResponse = await ScreepsAPI.GetAllSeasonsLeaderboard();
            if (leaderboardsResponse == null || leaderboardsResponse.Count == 0)
            {
                _leaderboardLogger.Warning("Leaderboard response was empty. Falling back to in-memory users cache.");
                await PersistUsersFromCache();
                return;
            }

            if (leaderboardsResponse != null)
            {
                var seasons = leaderboardsResponse.Select(kv => kv.Key).OrderDescending().ToList();
                var currentSeason = seasons.FirstOrDefault();

                var leaderboardList = leaderboardsResponse.Where(kv => kv.Key != currentSeason).Select(kv => kv.Value).ToList();
                foreach (var (gclLeaderboard, powerLeaderboard, scoreLeaderboard) in leaderboardList)
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
                    
                    foreach (var leaderboardSpot in scoreLeaderboard)
                    {
                        var userId = Users.FirstOrDefault(kv => kv.Value.Username == leaderboardSpot.UserName).Key;
                        if (userId == null)
                        {
                            continue;
                        }
                        if (!Users.TryGetValue(userId, out ScreepsUser? value))
                        {
                            await GetUser(userId);
                            Users.TryGetValue(userId, out value); // Retry after attempting to fetch
                        }

                        if (value != null)
                        {
                            leaderboardSpot.Rank += 1;
                            leaderboardSpot.UserName = value.Username;
                            leaderboardSpot.Type = "score";
                            await DBClient.WriteHistoricalLeaderboardData(leaderboardSpot);
                        }
                    }
                }
            }
            _leaderboardLogger.Information("Completed getting all users from leaderboard");
        }

        private static async Task UpdateUsersLeaderboard()
        {
            await TryRunWithLeaderboardLock("UpdateUsersLeaderboard", UpdateUsersLeaderboardCore);
        }

        private static async Task UpdateUsersLeaderboardCore()
        {
            if (ConfigSettingsState.ScreepsIsPrivateServer)
            {
                _leaderboardLogger.Information("Private server detected. Skipping leaderboard update and reusing in-memory users.");
                await PersistUsersFromCache();
                return;
            }

            _leaderboardLogger.Information("Updating users leaderboard data");
            var userIdsUpdated = new HashSet<string>();

            var (gclLeaderboard, powerLeaderboard, scoreLeaderboard) = await ScreepsAPI.GetCurrentSeasonLeaderboard();
            _leaderboardLogger.Information("Fetched current season leaderboard data");

            if (gclLeaderboard.Count == 0 && powerLeaderboard.Count == 0 && scoreLeaderboard.Count == 0)
            {
                _leaderboardLogger.Warning("Current season leaderboard response was empty. Falling back to in-memory users cache.");
                await PersistUsersFromCache();
                return;
            }

            _leaderboardLogger.Information("Updating user data from gcl leaderboard entries");
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
                    value.Score = leaderboardSpot.Score;
                    await DBClient.WriteCurrentLeaderboardData(leaderboardSpot);
                }
                else
                {
                    _leaderboardLogger.Warning("User {UserId} not found when updating GCL leaderboard", leaderboardSpot.UserName);
                }
            }

            _leaderboardLogger.Information("Updating user data from power leaderboard entries");
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

            _leaderboardLogger.Information("Updating user data from score leaderboard entries");
            foreach (var leaderboardSpot in scoreLeaderboard)
            {
                var userId = Users.FirstOrDefault(kv => kv.Value.Username == leaderboardSpot.UserName).Key;
                if (userId == null)
                {
                    continue;
                }
                if (!Users.TryGetValue(userId, out ScreepsUser? value) || !userIdsUpdated.Contains(userId))
                {
                    await GetUser(userId);
                    Users.TryGetValue(userId, out value);
                    userIdsUpdated.Add(userId);
                }

                if (value != null)
                {
                    leaderboardSpot.Rank += 1;
                    leaderboardSpot.UserName = value.Username;
                    leaderboardSpot.Type = "score";
                    value.Score = leaderboardSpot.Score;
                    await DBClient.WriteCurrentLeaderboardData(leaderboardSpot);
                }
                else
                {
                    _leaderboardLogger.Warning("User {UserId} not found when updating Score leaderboard", leaderboardSpot.UserName);
                }
            }

            UpdateCachedUserRanks();

            await WriteUsersByIds(userIdsUpdated);

            _leaderboardLogger.Information("Completed updating users leaderboard data");
        }
        private static async Task OnUpdateAdminUtilsDataTimerAsync()
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
