using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuestDB;
using QuestDB.Senders;
using System.Text;
using System.Threading.Channels;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerShared.Models.Db;
using UserTrackerShared.Models.ScreepsAPI;
using UserTrackerShared.States;
using UserTrackerShared.Utilities;

namespace UserTrackerShared.DBClients
{
    public static class QuestDBPointHelper
    {
        public static ISender UpdateHistoryPoint(ISender sender, QuestHistoryPointDataParameter parameters)
        {
            return sender.NullableColumn(parameters.Field, parameters.Value);
        }

        public static async Task InsertAdminUtilsPoint(ISender sender, QuestAdminUtilsPointDataParameter parameters)
        {
            await sender.Table(parameters.Database)
                .Symbol("field", parameters.Field)
                .Symbol("user", parameters.Username)
                .NullableColumn("value", parameters.Value)
                .AtAsync(DateTime.UtcNow);
        }
    }

    public static class QuestDBClientWriter
    {
        private static readonly JsonSerializer _serializer = JsonSerializer.CreateDefault();
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.QuestDB);
        private static bool _isInitialized = false;
        private static bool _isRunning = true;
        // Use an unbounded channel for high-performance concurrent writes.
        private static Channel<IReadOnlyList<QuestHistoryPointDataParameter>>? _historyChannel = null;
        private static Channel<QuestAdminUtilsPointDataParameter>? _adminUtilsChannel = null;

        // Counters for statistics.
        private static long _flushedPointCount = 0;
        private static long _pendingPointCount = 0;

        // Shared sender instance with proper disposal
        private static ISender? _sharedSender = null;
        private static readonly SemaphoreSlim _senderLock = new SemaphoreSlim(1, 1);
        private static ISender? _sharedHistorySender = null;
        private static readonly SemaphoreSlim _senderHistoryLock = new SemaphoreSlim(1, 1);
        private static ISender? _sharedAdminUtilsSender = null;
        private static readonly SemaphoreSlim _senderAdminUtilsLock = new SemaphoreSlim(1, 1);

        public static void Init()
        {
            if (_isInitialized)
            {
                _logger.Debug("QuestDB client already initialized, skipping initialization.");
                return;
            }

            _logger.Information("Initializing QuestDB client...");
            try
            {
                _isInitialized = true;
                var message = string.Format("QuestDB client connected to host {0}", ConfigSettingsState.QuestDbHost);
                _logger.Information(message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error initializing QuestDB client.");
                throw;
            }

            _historyChannel = Channel.CreateUnbounded<IReadOnlyList<QuestHistoryPointDataParameter>>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

            _adminUtilsChannel = Channel.CreateUnbounded<QuestAdminUtilsPointDataParameter>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

            Task.Run(HistoryWorkerLoop);
            Task.Run(AdminUtilsWorkerLoop);
            Task.Run(LogStatusPeriodically);

            _logger.Information("Worker tasks started.");
        }

        private static async Task HistoryWorkerLoop()
        {
            try
            {
                _logger.Information("History worker loop started, waiting for data...");
                while (_isRunning)
                {
                    if (_historyChannel == null) continue;
                    if (!await _historyChannel.Reader.WaitToReadAsync())
                        continue;

                    var sender = await GetSenderInstanceAsync("history");
                    // A channel item is a complete row. Draining individual fields could
                    // split a producer's row into two partial rows at a batch boundary.
                    var batchRows = new List<IReadOnlyList<QuestHistoryPointDataParameter>>();
                    while (batchRows.Count < 256 && _historyChannel.Reader.TryRead(out var row))
                    {
                        batchRows.Add(row);
                        Interlocked.Add(ref _pendingPointCount, -row.Count);
                    }
                    _logger.Information("Processing batch of {RowCount} history rows", batchRows.Count);

                    foreach (var row in batchRows)
                    {
                        try
                        {
                            var firstPoint = row.FirstOrDefault();
                            if (firstPoint == null) continue;

                            sender.Table(firstPoint.Database)
                                .Symbol("shard", firstPoint.Shard)
                                .Symbol("room", firstPoint.Room)
                                .Symbol("user", firstPoint.Username)
                                .Column("tick", firstPoint.Tick);

                            foreach (var point in row)
                            {
                                sender = QuestDBPointHelper.UpdateHistoryPoint(sender, point);
                                Interlocked.Add(ref _flushedPointCount, 1);
                            }

                            await sender.AtAsync(firstPoint.Timestamp * 1_000_000);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Error writing batch to QuestDB");
                        }
                    }

                    await FlushSender(sender);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in HistoryWorkerLoop");
            }
        }

        private static async Task AdminUtilsWorkerLoop()
        {
            try
            {
                _logger.Information("AdminUtils worker loop started.");
                while (_isRunning)
                {
                    if (_adminUtilsChannel == null) continue;
                    if (!await _adminUtilsChannel.Reader.WaitToReadAsync())
                        continue;

                    var sender = await GetSenderInstanceAsync("adminUtils");
                    var batchDict = new Dictionary<string, List<QuestAdminUtilsPointDataParameter>>();
                    int batchCount = 0;

                    while (_adminUtilsChannel.Reader.TryRead(out var item))
                    {
                        if (!batchDict.ContainsKey(item.Database))
                            batchDict[item.Database] = new List<QuestAdminUtilsPointDataParameter>();

                        batchDict[item.Database].Add(item);
                        batchCount++;
                        Interlocked.Decrement(ref _pendingPointCount);
                    }

                    foreach (var kvp in batchDict)
                    {
                        try
                        {
                            foreach (var point in kvp.Value)
                            {
                                await QuestDBPointHelper.InsertAdminUtilsPoint(sender, point);
                                Interlocked.Add(ref _flushedPointCount, 1);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Error writing batch to QuestDB");
                        }
                    }

                    await FlushSender(sender);
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error starting AdminUtilsWorkerLoop");
            }
        }

        private static async Task<ISender> GetSenderInstanceAsync(string type = "default")
        {
            if (type == "history" && _sharedHistorySender != null)
                return _sharedHistorySender;
            else if (type == "adminUtils" && _sharedAdminUtilsSender != null)
                return _sharedAdminUtilsSender;
            else if (type == "default" && _sharedSender != null)
                return _sharedSender;

            if (type == "history")
                await _senderHistoryLock.WaitAsync();
            else if (type == "adminUtils")
                await _senderAdminUtilsLock.WaitAsync();
            else if (type == "default")
                await _senderLock.WaitAsync();

            try
            {
                var connectionString = $"http::addr={ConfigSettingsState.QuestDbHost}:{ConfigSettingsState.QuestDbPort};username={ConfigSettingsState.QuestDbUser};password={ConfigSettingsState.QuestDbPassword};auto_flush=off;auto_flush_rows=-1;request_timeout=30000;retry_timeout=30000";

                const int maxRetries = 3;
                const int baseDelayMs = 1000;

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        var sender = Sender.New(connectionString);

                        if (type == "history")
                            _sharedHistorySender = sender;
                        else if (type == "adminUtils")
                            _sharedAdminUtilsSender = sender;
                        else if (type == "default")
                            _sharedSender = sender;

                        _logger.Information("Successfully created QuestDB sender instance of type {Type} on attempt {Attempt}", type, attempt);
                        return sender;
                    }
                    catch (Exception ex) when (attempt < maxRetries)
                    {
                        var delay = baseDelayMs * (int)Math.Pow(2, attempt - 1); // Exponential backoff
                        _logger.Warning(ex, "Failed to create QuestDB sender instance of type {Type} on attempt {Attempt}/{MaxRetries}. Retrying in {Delay}ms", type, attempt, maxRetries, delay);
                        await Task.Delay(delay);
                    }
                }

                throw new InvalidOperationException($"Failed to create QuestDB sender instance of type {type} after {maxRetries} attempts");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating QuestDB sender instance of type {Type}", type);
                throw;
            }
            finally
            {
                if (type == "history")
                    _senderHistoryLock.Release();
                else if (type == "adminUtils")
                    _senderAdminUtilsLock.Release();
                else if (type == "default")
                    _senderLock.Release();
            }
        }

        private static async Task FlushSender(ISender sender)
        {
            const int maxRetries = 3;
            const int baseDelayMs = 500;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    await sender.SendAsync();
                    if (attempt > 1)
                    {
                        _logger.Information("Successfully flushed QuestDB sender on attempt {Attempt}", attempt);
                    }
                    return;
                }
                catch (Exception ex) when (attempt < maxRetries)
                {
                    var delay = baseDelayMs * (int)Math.Pow(2, attempt - 1); // Exponential backoff
                    _logger.Warning(ex, "Failed to flush QuestDB sender on attempt {Attempt}/{MaxRetries}. Retrying in {Delay}ms", attempt, maxRetries, delay);
                    await Task.Delay(delay);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to flush QuestDB sender after {MaxRetries} attempts", maxRetries);
                    throw;
                }
            }
        }

        private static void AddHistoryRow(IReadOnlyList<QuestHistoryPointDataParameter> row)
        {
            if (row.Count == 0) return;
            var channel = _historyChannel;
            if (channel == null)
            {
                _logger.Warning("History channel is null, cannot add row");
                return;
            }

            Interlocked.Add(ref _pendingPointCount, row.Count);
            if (!channel.Writer.TryWrite(row))
            {
                Interlocked.Add(ref _pendingPointCount, -row.Count);
                _logger.Warning("Failed to write to history channel - channel may be closed");
            }
        }

        public static void AddPoint(QuestAdminUtilsPointDataParameter pointParameters)
        {
            if (_adminUtilsChannel == null)
            {
                _logger.Warning("AdminUtils channel is null, cannot add point");
                return;
            }

            var written = _adminUtilsChannel.Writer.TryWrite(pointParameters);
            if (written)
            {
                Interlocked.Increment(ref _pendingPointCount);
            }
            else
            {
                _logger.Warning("Failed to write to adminUtils channel - channel may be full or closed");
            }
        }

        public static void UploadRoomHistoryData(string database, string shard, string room, long tick, long timestamp, string username, object obj)
            => UploadHistoryData(database, shard, room, tick, timestamp, username, obj);

        public static void UploadUserHistoryData(string database, string shard, long tick, long timestamp, string username, object obj)
            => UploadHistoryData(database, shard, "", tick, timestamp, username, obj);

        public static void UploadGlobalHistoryData(string database, string shard, long tick, long timestamp, object obj)
            => UploadHistoryData(database, shard, "", tick, timestamp, "", obj);

        private static void UploadHistoryData(string database, string shard, string room, long tick, long timestamp, string username, object obj)
        {
            try
            {
                var flattenedData = new Dictionary<string, object?>();
                // Each call gets its own serializer; room producers may run concurrently.
                JsonHelper.FlattenJson(JToken.FromObject(obj), new StringBuilder(), flattenedData);
                var row = new List<QuestHistoryPointDataParameter>();
                foreach (var kvp in flattenedData.Where(kvp => kvp.Value is long or int or double or decimal))
                {
                    row.Add(new QuestHistoryPointDataParameter(database, shard, room, tick, timestamp,
                        username, kvp.Key.ToLowerInvariant(), Convert.ToDouble(kvp.Value)));
                }
                AddHistoryRow(row);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UploadHistoryData");
            }
        }

        public static void UploadAdminUtilsData(string database, object obj)
        {
            try
            {
                var flattenedData = new Dictionary<string, object?>();
                var writer = new JTokenWriter();
                _serializer.Serialize(writer, obj);
                JsonHelper.FlattenJson(writer.Token!, new StringBuilder(), flattenedData);

                foreach (var kvp in flattenedData.Where(kvp => kvp.Value is long || kvp.Value is int || kvp.Value is double || kvp.Value is decimal))
                {
                    var key = kvp.Key.ToLower();
                    var keyIncludesUsername = kvp.Key.Contains("users.", StringComparison.OrdinalIgnoreCase);
                    var username = keyIncludesUsername ? kvp.Key.Split('.')[1] : null;
                    if (keyIncludesUsername)
                    {
                        key = kvp.Key.Replace($"users.{username}", "users", StringComparison.OrdinalIgnoreCase);
                    }

                    var pointParameters = new QuestAdminUtilsPointDataParameter(
                        database,
                        username,
                        key,
                        Convert.ToDouble(kvp.Value));
                    AddPoint(pointParameters);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UploadAdminUtilsData");
            }
        }

        public static async Task UploadPerformanceData(string database, PerformanceClassDto performanceClassDto)
        {
            try
            {
                var sender = await GetSenderInstanceAsync();

                var command = sender.Table(database)
                        .Symbol("shard", performanceClassDto.Shard)
                        .Column("ticksBehind", performanceClassDto.TicksBehind)
                        .Column("timeTakenMs", performanceClassDto.TimeTakenMs)
                        .Column("totalRooms", performanceClassDto.TotalRooms);

                foreach (var resultCodeKvp in performanceClassDto.ResultCodes)
                {
                    command = command.Column($"resultCodes_{resultCodeKvp.Key}", resultCodeKvp.Value);
                }

                Interlocked.Increment(ref _flushedPointCount);
                await command.AtAsync(DateTime.UtcNow);
                await FlushSender(sender);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UploadPerformanceData");
            }
        }

        public static async Task UploadSingleUserData(string database, ScreepsUser user)
        {
            try
            {
                var sender = await GetSenderInstanceAsync();

                await sender.Table(database)
                    .Symbol("user", user.Username)
                    .Column("gcl", user.GCL)
                    .Column("gclRank", user.GCLRank)
                    .Column("power", user.Power)
                    .Column("powerRank", user.PowerRank)
                    .Column("score", user.Score)
                    .Column("scoreRank", user.ScoreRank)
                    .AtAsync(DateTime.UtcNow);

                Interlocked.Increment(ref _flushedPointCount);
                await FlushSender(sender);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UploadSingleUserData");
            }
        }

        public static async Task UploadLeaderboardData(string database, SeasonListItem seasonItem)
        {
            try
            {
                var sender = await GetSenderInstanceAsync();

                await sender.Table(database)
                    .Symbol("type", seasonItem.Type)
                    .Symbol("user", seasonItem.UserName)
                    .Symbol("season", seasonItem.Season)
                    .Column("score", seasonItem.Score)
                    .Column("rank", seasonItem.Rank)
                    .AtAsync(seasonItem.Timestamp);

                Interlocked.Increment(ref _flushedPointCount);
                await FlushSender(sender);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UploadLeaderboardData");
            }
        }

        private static async Task LogStatusPeriodically()
        {
            while (_isRunning)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));

                var flushed = Interlocked.Exchange(ref _flushedPointCount, 0);
                var pending = Interlocked.Read(ref _pendingPointCount);
                _logger.Information("Flushed {Flushed} points in the last 10 seconds. Pending points: {Pending}", flushed, pending);
            }
        }

        public static void Stop()
        {
            _isRunning = false;

            // Clean up the shared sender
            if (_sharedSender != null)
            {
                try
                {
                    _sharedSender.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Error disposing QuestDB sender");
                }
                finally
                {
                    _sharedSender = null;
                }
            }

            _logger.Information("Worker tasks stopped.");
        }
    }

    public static class QuestDBDtoHelper
    {
        private static readonly System.Reflection.PropertyInfo[] StoreResourceProperties = typeof(Store)
            .GetProperties().Where(p => p.PropertyType == typeof(decimal?)).ToArray();

        public static (decimal, decimal, Dictionary<string, decimal>) GetStructureCounts(ScreepsRoomHistoryDto history)
        {
            var structureCounts = new Dictionary<string, decimal>();
            decimal structureCount = 0;
            decimal placedStructureCounts = 0;

            if (history.Structures.Controller != null)
            {
                var count = history.Structures.Controller.Count;
                structureCounts["controller"] = count;

                structureCount += count;
            }
            if (history.Structures.Mineral != null)
            {
                var count = history.Structures.Mineral.Count;
                structureCounts["mineral"] = count;

                structureCount += count;
            }
            if (history.Structures.Deposit != null)
            {
                var count = history.Structures.Deposit.Count;
                structureCounts["deposit"] = count;

                structureCount += count;
            }
            if (history.Structures.Wall != null)
            {
                var count = history.Structures.Wall.Count;
                structureCounts["wall"] = count;

                structureCount += count;
                placedStructureCounts += count;
            }
            if (history.Structures.ConstructionSite != null)
            {
                var count = history.Structures.ConstructionSite.Count;
                structureCounts["constructionsite"] = count;
            }
            if (history.Structures.Container != null)
            {
                var count = history.Structures.Container.Count;
                structureCounts["container"] = count;

                structureCount += count;
                placedStructureCounts += count;
            }
            if (history.Structures.Extension != null)
            {
                var count = history.Structures.Extension.Count;
                structureCounts["extension"] = count;

                structureCount += count;
                placedStructureCounts += count;
            }
            if (history.Structures.Extractor != null)
            {
                var count = history.Structures.Extractor.Count;
                structureCounts["extractor"] = count;

                structureCount += count;
                placedStructureCounts += count;
            }
            if (history.Structures.Factory != null)
            {
                var count = history.Structures.Factory.Count;
                structureCounts["factory"] = count;

                structureCount += count;
                placedStructureCounts += count;
            }
            if (history.Structures.InvaderCore != null)
            {
                var count = history.Structures.InvaderCore.Count;
                structureCounts["invadercore"] = count;
            }
            if (history.Structures.KeeperLair != null)
            {
                var count = history.Structures.KeeperLair.Count;
                structureCounts["keeperlair"] = count;

                structureCount += count;
            }
            if (history.Structures.Lab != null)
            {
                var count = history.Structures.Lab.Count;
                structureCounts["lab"] = count;

                structureCount += count;
                placedStructureCounts += count;
            }
            if (history.Structures.Link != null)
            {
                var count = history.Structures.Link.Count;
                structureCounts["link"] = count;

                structureCount += count;
                placedStructureCounts += count;
            }
            if (history.Structures.Observer != null)
            {
                var count = history.Structures.Observer.Count;
                structureCounts["observer"] = count;

                structureCount += count;
                placedStructureCounts += count;
            }
            if (history.Structures.Portal != null)
            {
                var count = history.Structures.Portal.Count;
                structureCounts["portal"] = count;

                structureCount += count;
            }
            if (history.Structures.PowerBank != null)
            {
                var count = history.Structures.PowerBank.Count;
                structureCounts["powerbank"] = count;

                structureCount += count;
            }
            if (history.Structures.PowerSpawn != null)
            {
                var count = history.Structures.PowerSpawn.Count;
                structureCounts["powerspawn"] = count;

                structureCount += count;
                placedStructureCounts += count;
            }
            if (history.Structures.Rampart != null)
            {
                var count = history.Structures.Rampart.Count;
                structureCounts["rampart"] = count;

                structureCount += count;
                placedStructureCounts += count;
            }
            if (history.Structures.Road != null)
            {
                var count = history.Structures.Road.Count;
                structureCounts["road"] = count;

                structureCount += count;
                placedStructureCounts += count;
            }
            if (history.Structures.Ruin != null)
            {
                var count = history.Structures.Ruin.Count;
                structureCounts["ruin"] = count;

                structureCount += count;
            }
            if (history.Structures.Source != null)
            {
                var count = history.Structures.Source.Count;
                structureCounts["source"] = count;

                structureCount += count;
            }
            if (history.Structures.Spawn != null)
            {
                var count = history.Structures.Spawn.Count;
                structureCounts["spawn"] = count;

                structureCount += count;
                placedStructureCounts += count;
            }
            if (history.Structures.Storage != null)
            {
                var count = history.Structures.Storage.Count;
                structureCounts["storage"] = count;

                structureCount += count;
                placedStructureCounts += count;
            }
            if (history.Structures.Terminal != null)
            {
                var count = history.Structures.Terminal.Count;
                structureCounts["terminal"] = count;

                structureCount += count;
                placedStructureCounts += count;
            }
            if (history.Structures.Tombstone != null)
            {
                var count = history.Structures.Tombstone.Count;
                structureCounts["tombstone"] = count;

                structureCount += count;
            }
            if (history.Structures.Tower != null)
            {
                var count = history.Structures.Tower.Count;
                structureCounts["tower"] = count;

                structureCount += count;
                placedStructureCounts += count;
            }
            if (history.Structures.Nuker != null)
            {
                var count = history.Structures.Nuker.Count;
                structureCounts["nuker"] = count;

                structureCount += count;
                placedStructureCounts += count;
            }
            if (history.Structures.Nuke != null)
            {
                var count = history.Structures.Nuke.Count;
                structureCounts["nuke"] = count;

                structureCount += count;
            }

            return (structureCount, placedStructureCounts, structureCounts);
        }

        public static (decimal, decimal, decimal, decimal, decimal) GetCreepCounts(ScreepsRoomHistoryDto history)
        {
            decimal ownedCreepCount = 0;
            decimal enemyCreepCount = 0;
            decimal otherCreepCount = 0;
            decimal powerCreepCount = 0;

            if (history.Creeps.OwnedCreeps != null)
            {
                ownedCreepCount += history.Creeps.OwnedCreeps.Count;
            }
            if (history.Creeps.EnemyCreeps != null)
            {
                enemyCreepCount += history.Creeps.EnemyCreeps.Count;
            }
            if (history.Creeps.OtherCreeps != null)
            {
                otherCreepCount += history.Creeps.OtherCreeps.Count;
            }
            if (history.Creeps.PowerCreeps != null)
            {
                powerCreepCount = history.Creeps.PowerCreeps.Count;
            }

            decimal creepCount = ownedCreepCount + enemyCreepCount + otherCreepCount + powerCreepCount;
            return (creepCount, ownedCreepCount, enemyCreepCount, otherCreepCount, powerCreepCount);
        }
        private static (decimal, Dictionary<string, decimal>) GetCreepPartsCounts(CountByPartDto countByPart)
        {
            decimal creepPartsCount = 0;
            var creepPartsCounts = new Dictionary<string, decimal>()
            {
                {"attack", 0},
                {"carry", 0},
                {"heal", 0},
                {"move", 0},
                {"ranged_attack", 0},
                {"tough", 0},
                {"work", 0},
                {"claim", 0 }
            };


            creepPartsCount += countByPart.Attack;
            creepPartsCounts["attack"] = countByPart.Attack;

            creepPartsCount += countByPart.Carry;
            creepPartsCounts["carry"] = countByPart.Carry;

            creepPartsCount += countByPart.Heal;
            creepPartsCounts["heal"] = countByPart.Heal;

            creepPartsCount += countByPart.Move;
            creepPartsCounts["move"] = countByPart.Move;

            creepPartsCount += countByPart.RangedAttack;
            creepPartsCounts["ranged_attack"] = countByPart.RangedAttack;

            creepPartsCount += countByPart.Tough;
            creepPartsCounts["tough"] = countByPart.Tough;

            creepPartsCount += countByPart.Work;
            creepPartsCounts["work"] = countByPart.Work;

            creepPartsCount += countByPart.Claim;
            creepPartsCounts["claim"] = countByPart.Claim;


            return (creepPartsCount, creepPartsCounts);
        }
        public static (decimal, Dictionary<string, decimal>) GetCreepPartsCounts(ScreepsRoomHistoryDto history)
        {
            var creepPartsCounts = new Dictionary<string, decimal>();
            decimal creepPartsCount = 0;

            if (history.Creeps.OwnedCreeps != null)
            {
                var bodyParts = history.Creeps.OwnedCreeps.BodyParts;
                var (count, counts) = GetCreepPartsCounts(bodyParts);
                creepPartsCount += count;
                foreach (var kvp in counts)
                {
                    if (creepPartsCounts.ContainsKey(kvp.Key))
                        creepPartsCounts[kvp.Key] += kvp.Value;
                    else
                        creepPartsCounts[kvp.Key] = kvp.Value;
                }
            }

            return (creepPartsCount, creepPartsCounts);
        }
        private static (decimal, Dictionary<string, decimal>, decimal, decimal) GetCreepIntentsCounts(ActionLogDto actionLog)
        {
            decimal energyInflow = 0;
            decimal energyOutflow = 0;
            decimal creepIntentsCount = 0;
            var creepIntentsCounts = new Dictionary<string, decimal>()
            {
                {"attack", 0},
                {"attacked", 0},
                {"ranged_attack", 0},
                {"ranged_mass_attacked", 0},
                {"ranged_heal", 0},
                {"heal", 0},
                {"healed", 0},
                {"harvest", 0},
                {"repair", 0},
                {"build", 0},
                {"upgrade_controller", 0},
                {"move", 0},
                {"say", 0},
                {"reserve_controller", 0},
                {"attack_controller", 0},
                {"produce", 0},
                {"transfer_energy", 0},
                {"run_reaction", 0},
                {"reverse_reaction", 0},
                {"spawned", 0},
                {"power", 0}
            };

            creepIntentsCount += actionLog.Attack.Count;
            creepIntentsCounts["attack"] = actionLog.Attack.Count;

            creepIntentsCount += actionLog.Attacked.Count;
            creepIntentsCounts["attacked"] = actionLog.Attacked.Count;

            creepIntentsCount += actionLog.RangedAttack.Count;
            creepIntentsCounts["ranged_attack"] = actionLog.RangedAttack.Count;

            creepIntentsCount += actionLog.RangedMassAttack.Count;
            creepIntentsCounts["ranged_mass_attacked"] = actionLog.RangedMassAttack.Count;

            creepIntentsCount += actionLog.RangedHeal.Count;
            creepIntentsCounts["ranged_heal"] = actionLog.RangedHeal.Count;

            creepIntentsCount += actionLog.Heal.Count;
            creepIntentsCounts["heal"] = actionLog.Heal.Count;

            creepIntentsCount += actionLog.Healed.Count;
            creepIntentsCounts["healed"] = actionLog.Healed.Count;

            creepIntentsCount += actionLog.Harvest.Count;
            creepIntentsCounts["harvest"] = actionLog.Harvest.Count;
            energyInflow += actionLog.Harvest.Inflow;

            creepIntentsCount += actionLog.Repair.Count;
            creepIntentsCounts["repair"] = actionLog.Repair.Count;
            energyOutflow += actionLog.Repair.Outflow;

            creepIntentsCount += actionLog.Build.Count;
            creepIntentsCounts["build"] = actionLog.Build.Count;
            energyOutflow += actionLog.Build.Outflow;

            creepIntentsCount += actionLog.UpgradeController.Count;
            creepIntentsCounts["upgrade_controller"] = actionLog.UpgradeController.Count;
            energyOutflow += actionLog.UpgradeController.Outflow;

            creepIntentsCount += actionLog.Move.Count;
            creepIntentsCounts["move"] = actionLog.Move.Count;

            creepIntentsCount += actionLog.Say.Count;
            creepIntentsCounts["say"] = actionLog.Say.Count;

            creepIntentsCount += actionLog.ReserveController.Count;
            creepIntentsCounts["reserve_controller"] = actionLog.ReserveController.Count;

            creepIntentsCount += actionLog.AttackController.Count;
            creepIntentsCounts["attack_controller"] = actionLog.AttackController.Count;

            creepIntentsCount += actionLog.Produce.Count;
            creepIntentsCounts["produce"] = actionLog.Produce.Count;

            creepIntentsCount += actionLog.TransferEnergy.Count;
            creepIntentsCounts["transfer_energy"] = actionLog.TransferEnergy.Count;

            creepIntentsCount += actionLog.RunReaction.Count;
            creepIntentsCounts["run_reaction"] = actionLog.RunReaction.Count;

            creepIntentsCount += actionLog.ReverseReaction.Count;
            creepIntentsCounts["reverse_reaction"] = actionLog.ReverseReaction.Count;

            creepIntentsCount += actionLog.Spawned.Count;
            creepIntentsCounts["spawned"] = actionLog.Spawned.Count;

            creepIntentsCount += actionLog.Power.Count;
            creepIntentsCounts["power"] = actionLog.Power.Count;


            return (creepIntentsCount, creepIntentsCounts, energyInflow, energyOutflow);
        }
        public static (decimal, Dictionary<string, decimal>, decimal, decimal) GetCreepIntentsCounts(ScreepsRoomHistoryDto history)
        {
            decimal creepEnergyInflow = 0;
            decimal creepEnergyOutflow = 0;
            var creepIntentsCounts = new Dictionary<string, decimal>();
            decimal creepIntentsCount = 0;

            if (history.Creeps.OwnedCreeps != null)
            {
                var actionLog = history.Creeps.OwnedCreeps.ActionLog;
                var (count, counts, energyInflow, energyOutflow) = GetCreepIntentsCounts(actionLog);
                creepIntentsCount += count;
                creepEnergyInflow += energyInflow;
                creepEnergyOutflow += energyOutflow;
                foreach (var kvp in counts)
                {
                    if (creepIntentsCounts.ContainsKey(kvp.Key))
                        creepIntentsCounts[kvp.Key] += kvp.Value;
                    else
                        creepIntentsCounts[kvp.Key] = kvp.Value;
                }
            }

            return (creepIntentsCount, creepIntentsCounts, creepEnergyInflow, creepEnergyOutflow);
        }

        public static (decimal, Dictionary<string, decimal>) GetStructureStoreCounts(Store store, bool detailed = false)
        {
            var storeTotals = new Dictionary<string, decimal>();
            decimal storeTotal = 0;

            void Add(string key, decimal? value)
            {
                if (value.HasValue)
                {
                    storeTotals[key] = value.Value;
                    storeTotal += value.Value;
                }
            }

            if (detailed)
            {
                // Include zero stock, too, so a depleted resource is not a missing metric.
                foreach (var property in StoreResourceProperties)
                    Add(property.Name.ToLowerInvariant(), (decimal?)property.GetValue(store) ?? 0m);
                return (storeTotal, storeTotals);
            }

            Add(nameof(store.energy), store.energy);
            Add("battery", store.battery);

            Add("h", store.H);
            Add("o", store.O);
            Add("u", store.U);
            Add("l", store.L);
            Add("k", store.K);
            Add("z", store.Z);
            Add("x", store.X);
            Add("g", store.G);

            Add(nameof(store.power), store.power);
            Add("ops", store.ops);

            return (storeTotal, storeTotals);
        }
        public static (decimal, Dictionary<string, decimal>) GetStoreCounts(ScreepsRoomHistoryDto history, bool detailed = false)
        {
            var storeTotals = new Dictionary<string, decimal>();
            decimal storeTotal = 0;

            if (history.Structures.Storage != null)
            {
                var (total, totals) = GetStructureStoreCounts(history.Structures.Storage.Store, detailed);
                storeTotal += total;
                foreach (var kvp in totals)
                {
                    if (storeTotals.ContainsKey(kvp.Key))
                        storeTotals[kvp.Key] += kvp.Value;
                    else
                        storeTotals[kvp.Key] = kvp.Value;
                }
            }
            if (history.Structures.Terminal != null)
            {
                var (total, totals) = GetStructureStoreCounts(history.Structures.Terminal.Store, detailed);
                storeTotal += total;
                foreach (var kvp in totals)
                {
                    if (storeTotals.ContainsKey(kvp.Key))
                        storeTotals[kvp.Key] += kvp.Value;
                    else
                        storeTotals[kvp.Key] = kvp.Value;
                }
            }
            if (history.Structures.Container != null)
            {
                var (total, totals) = GetStructureStoreCounts(history.Structures.Container.Store, detailed);
                storeTotal += total;
                foreach (var kvp in totals)
                {
                    if (storeTotals.ContainsKey(kvp.Key))
                        storeTotals[kvp.Key] += kvp.Value;
                    else
                        storeTotals[kvp.Key] = kvp.Value;
                }
            }
            if (history.Structures.Link != null)
            {
                var energyTotal = history.Structures.Link.Energy;
                storeTotal += energyTotal;
                if (storeTotals.ContainsKey("energy"))
                {
                    storeTotals["energy"] += energyTotal;
                }
                else
                {
                    storeTotals["energy"] = energyTotal;
                }
            }

            return (storeTotal, storeTotals);
        }
    }

    public static class QuestDBClientState
    {
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.QuestDB);
        private static readonly HttpClient _httpClient = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var handler = new SocketsHttpHandler()
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(15),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
                MaxConnectionsPerServer = 100,
                EnableMultipleHttp2Connections = true
            };

            return new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        private static async Task<bool> ContainsHistoricalLeaderboardData(string database, SeasonListItem seasonItem)
        {
            try
            {
                var sql = $"SELECT 1 FROM {database} WHERE season = '{seasonItem.Season}' and user = '{seasonItem.UserName}' LIMIT 1";
                var response = await _httpClient.GetAsync($"http://{ConfigSettingsState.QuestDbHost}:9000/exec?query=" + Uri.EscapeDataString(sql));
                var content = await response.Content.ReadAsStringAsync();

                var lines = content.Split('\n');
                bool exists = lines.Length > 2;
                return exists;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching historical leaderboard data for season {Season}", seasonItem.Season);
                return false;
            }
        }

        public static QuestDBHistoryDTO GetQuestDBDto(ScreepsRoomHistoryDto screepsRoomHistory)
        {
            var (structureCount, placedStructureCounts, structureCounts) = QuestDBDtoHelper.GetStructureCounts(screepsRoomHistory);
            var (creepCount, ownedCreepCount, enemyCreepCount, otherCreepCount, powerCreepCount) = QuestDBDtoHelper.GetCreepCounts(screepsRoomHistory);
            var (ownedCreepPartsCount, ownedCreepPartsCounts) = QuestDBDtoHelper.GetCreepPartsCounts(screepsRoomHistory);
            var (creepIntentCount, creepIntentCounts, creepEnergyInflow, creepEnergyOutflow) = QuestDBDtoHelper.GetCreepIntentsCounts(screepsRoomHistory);
            var (ownedRoomCount, reservedRoomCount) = (screepsRoomHistory.Structures.Controller.OwnedUserIdCount, screepsRoomHistory.Structures.Controller.ReservationUserIdCount);
            var (storeTotal, storeTotals) = QuestDBDtoHelper.GetStoreCounts(screepsRoomHistory, ConfigSettingsState.QuestDbDetailedEnabled);
            var controller = screepsRoomHistory.Structures.Controller;

            var questDBHistoryDTO = new QuestDBHistoryDTO()
            {
                StructureCount = structureCount,
                StructureCounts = structureCounts,
                PlacedStructureCount = placedStructureCounts,

                CreepCount = creepCount,
                OwnedCreepCount = ownedCreepCount,
                EnemyCreepCount = enemyCreepCount,
                OtherCreepCount = otherCreepCount,
                PowerCreepCount = powerCreepCount,
                OwnedCreepPartsCount = ownedCreepPartsCount,
                OwnedCreepPartsCounts = ownedCreepPartsCounts,
                CreepIntentCount = creepIntentCount,
                CreepIntentCounts = creepIntentCounts,
                CreepEnergyInflow = creepEnergyInflow,
                CreepEnergyOutflow = creepEnergyOutflow,

                OwnedRoomCount = ownedRoomCount,
                ReservedRoomCount = reservedRoomCount,

                ControllerLevel = controller?.Level,
                ControllerProgress = controller?.Progress,
                ControllerProgressTotal = controller?.ProgressTotal,
                ControllerPointsPerTick = controller?.Upgraded,
                ControllerScorePerTick = controller?.ScorePerTick,

                StoreTotal = storeTotal,
                StoreTotals = storeTotals,
                Structures = ConfigSettingsState.QuestDbDetailedEnabled ? screepsRoomHistory.Structures : null,
                Creeps = ConfigSettingsState.QuestDbDetailedEnabled ? screepsRoomHistory.Creeps : null,
                GroundResources = ConfigSettingsState.QuestDbDetailedEnabled ? screepsRoomHistory.GroundResources : null
            };

            return questDBHistoryDTO;
        }

        public static async Task WriteScreepsRoomHistory(string shard, string room, long tick, long timestamp, ScreepsRoomHistoryDto screepsRoomHistory)
        {
            try
            {
                var userId = screepsRoomHistory.UserId;
                var username = "";

                if (!string.IsNullOrEmpty(userId) && GameState.Users.TryGetValue(userId, out var user))
                {
                    username = user.Username;
                }
                else if (!string.IsNullOrEmpty(userId))
                {
                    var apiUser = await ScreepsAPI.GetUser(userId);
                    if (apiUser != null)
                    {
                        GameState.Users.AddOrUpdate(userId, apiUser, (key, oldValue) => apiUser);
                        username = apiUser?.Username ?? "";
                    }
                }
                var database = $"{ConfigSettingsState.ServerName}_room_history";
                var questDBHistoryDTO = GetQuestDBDto(screepsRoomHistory);

                _logger.Information("Uploading room history data for {Shard}/{Room} at tick {Tick} (user: {Username}/{UserId})", shard, room, tick, username, userId);
                QuestDBClientWriter.UploadRoomHistoryData(database, shard, room, tick, timestamp, username, questDBHistoryDTO);
            }
            catch (Exception e)
            {
                var message = string.Format("Error uploading roomhistory {0}/{1}/{2}", shard, room, tick);
                _logger.Error(e, message);
            }
        }

        public static void WriteScreepsUserHistory(string shard, string username, long tick, long timestamp, ScreepsRoomHistoryDto screepsRoomHistory)
        {
            try
            {
                var database = $"{ConfigSettingsState.ServerName}_user_history";
                var questDBHistoryDTO = GetQuestDBDto(screepsRoomHistory);

                _logger.Information("Uploading user history data for {Shard} at tick {Tick} (user: {Username})", shard, tick, username);
                QuestDBClientWriter.UploadUserHistoryData(database, shard, tick, timestamp, username, questDBHistoryDTO);
            }
            catch (Exception e)
            {
                var message = string.Format("Error uploading userhistory {0}/{1}/{2}", shard, username, tick);
                _logger.Error(e, message);
            }
        }

        public static void WriteScreepsGlobalHistory(string shard, long tick, long timestamp, ScreepsRoomHistoryDto screepsRoomHistory)
        {
            try
            {
                var database = $"{ConfigSettingsState.ServerName}_global_history";
                var questDBHistoryDTO = GetQuestDBDto(screepsRoomHistory);

                _logger.Information("Uploading user history data for {Shard} at tick {Tick}", shard, tick);
                QuestDBClientWriter.UploadGlobalHistoryData(database, shard, tick, timestamp, questDBHistoryDTO);
            }
            catch (Exception e)
            {
                var message = string.Format("Error uploading globalhistory {0}/{1}", shard, tick);
                _logger.Error(e, message);
            }
        }

        public static async Task WritePerformanceData(PerformanceClassDto performanceClassDto)
        {
            try
            {
                var database = $"{ConfigSettingsState.ServerName}_performance";
                await QuestDBClientWriter.UploadPerformanceData(database, performanceClassDto);
                _logger.Information("Performance data for shard {Shard} uploaded successfully.", performanceClassDto.Shard);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error uploading performance data");
            }
        }

        public static async Task WriteHistoricalLeaderboardData(SeasonListItem seasonItem)
        {
            try
            {
                var seasonDate = DateTime.ParseExact(seasonItem.Season, "yyyy-MM", null);
                var endOfMonth = new DateTime(seasonDate.Year, seasonDate.Month, DateTime.DaysInMonth(seasonDate.Year, seasonDate.Month), 23, 59, 59);
                seasonItem.Timestamp = DateTime.SpecifyKind(endOfMonth, DateTimeKind.Utc);

                var database = $"{ConfigSettingsState.ServerName}_historical_leaderboard";
                if (!await ContainsHistoricalLeaderboardData(database, seasonItem))
                {
                    await QuestDBClientWriter.UploadLeaderboardData(database, seasonItem);
                    _logger.Information("Historical leaderboard data for user {User} in season {Season} uploaded successfully.", seasonItem.UserName, seasonItem.Season);
                }
                else
                {
                    _logger.Information("Historical leaderboard data for user {User} in season {Season} already exists, skipping upload.", seasonItem.UserName, seasonItem.Season);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error uploading historical leaderboard data");
            }
        }

        public static async Task WriteCurrentLeaderboardData(SeasonListItem seasonItem)
        {
            try
            {
                seasonItem.Timestamp = DateTime.UtcNow;
                var database = $"{ConfigSettingsState.ServerName}_current_leaderboard";
                await QuestDBClientWriter.UploadLeaderboardData(database, seasonItem);
                _logger.Information("Current leaderboard data for user {User}/{UserId} in season {Season} uploaded successfully.", seasonItem.UserName, seasonItem.UserId, seasonItem.Season);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error uploading current leaderboard data");
            }
        }

        public static void WriteAdminUtilsData(AdminUtilsDto adminUtilsDto)
        {
            try
            {
                var database = $"{ConfigSettingsState.ServerName}_adminutils";
                QuestDBClientWriter.UploadAdminUtilsData(database, adminUtilsDto);
                _logger.Information("Admin utils data uploaded successfully.");
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error uploading admin utils data");
            }
        }

        public static async Task WriteSingleUserData(ScreepsUser user)
        {
            try
            {
                var database = $"{ConfigSettingsState.ServerName}_users";
                await QuestDBClientWriter.UploadSingleUserData(database, user);
                _logger.Information("User data for {Username} uploaded successfully.", user.Username);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error uploading user data");
            }
        }
    }
}
