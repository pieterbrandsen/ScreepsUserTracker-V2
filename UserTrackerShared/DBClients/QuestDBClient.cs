using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuestDB;
using QuestDB.Senders;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using UserTrackerShared.Helpers;
using UserTrackerShared.Models;
using UserTrackerShared.Models.Db;
using UserTrackerShared.Models.ScreepsAPI;
using UserTrackerShared.States;

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
        private static Channel<QuestHistoryPointDataParameter>? _historyChannel = null;
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

            _historyChannel = Channel.CreateUnbounded<QuestHistoryPointDataParameter>(new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            });

            _adminUtilsChannel = Channel.CreateUnbounded<QuestAdminUtilsPointDataParameter>(new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            });

            Task.Run(HistoryWorkerLoop);
            Task.Run(AdminUtilsWorkerLoop);

            // Start a background task to log status.
            Task.Run(LogStatusPeriodically);

            _logger.Information("Worker tasks started.");
        }

        private static string GetBatchKey(QuestHistoryPointDataParameter point)
        {
            return $"{point.Database}|{point.Shard}|{point.Room}|{point.Username}|{point.Tick}";
        }

        private static async Task HistoryWorkerLoop()
        {
            while (_isRunning)
            {
                if (_historyChannel == null) continue;
                // Create a temporary dictionary to group points per bucket.
                if (!await _historyChannel.Reader.WaitToReadAsync())
                    continue;

                var sender = await GetSenderInstanceAsync("history");
                var batchDict = new Dictionary<string, List<QuestHistoryPointDataParameter>>();
                int batchCount = 0;

                // Read items from the channel up to the batch size.
                while (_historyChannel.Reader.TryRead(out var item))
                {
                    var batchKey = GetBatchKey(item);
                    if (!batchDict.ContainsKey(batchKey))
                        batchDict[batchKey] = new List<QuestHistoryPointDataParameter>();

                    batchDict[batchKey].Add(item);
                    batchCount++;
                    Interlocked.Decrement(ref _pendingPointCount);
                }
                _logger.Information("Processing batch of {BatchCount} rows", batchCount);

                // Process each bucket's batch.
                foreach (var kvp in batchDict)
                {
                    try
                    {
                        var firstPoint = kvp.Value.FirstOrDefault();
                        if (firstPoint == null) continue;

                        sender.Table(firstPoint.Database)
                            .Symbol("shard", firstPoint.Shard)
                            .Symbol("room", firstPoint.Room)
                            .Symbol("user", firstPoint.Username)
                            .Column("tick", firstPoint.Tick);

                        foreach (var point in kvp.Value)
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
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private static async Task AdminUtilsWorkerLoop()
        {
            while (_isRunning)
            {
                if (_adminUtilsChannel == null) continue;
                // Create a temporary dictionary to group points per bucket.
                if (!await _adminUtilsChannel.Reader.WaitToReadAsync())
                    continue;

                var sender = await GetSenderInstanceAsync("adminUtils");
                var batchDict = new Dictionary<string, List<QuestAdminUtilsPointDataParameter>>();
                int batchCount = 0;

                // Read items from the channel up to the batch size.
                while (_adminUtilsChannel.Reader.TryRead(out var item))
                {
                    if (!batchDict.ContainsKey(item.Database))
                        batchDict[item.Database] = new List<QuestAdminUtilsPointDataParameter>();

                    batchDict[item.Database].Add(item);
                    batchCount++;
                    Interlocked.Decrement(ref _pendingPointCount);
                }

                // Process each bucket's batch.
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
                var sender = Sender.New(connectionString);

                if (type == "history")
                    _sharedHistorySender = sender;
                else if (type == "adminUtils")
                    _sharedAdminUtilsSender = sender;
                else if (type == "default")
                    _sharedSender = sender;

                return sender;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating QuestDB sender instance");
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
            try
            {
                await sender.SendAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error flushing QuestDB sender.");
            }
        }

        public static void AddPoint(QuestHistoryPointDataParameter pointParameters)
        {
            _historyChannel?.Writer.TryWrite(pointParameters);
            Interlocked.Increment(ref _pendingPointCount);
        }

        public static void AddPoint(QuestAdminUtilsPointDataParameter pointParameters)
        {
            _adminUtilsChannel?.Writer.TryWrite(pointParameters);
            Interlocked.Increment(ref _pendingPointCount);
        }

        public static void UploadRoomHistoryData(string database, string shard, string room, long tick, long timestamp, string username, object obj)
        {
            try
            {
                var flattenedData = new Dictionary<string, object?>();
                var writer = new JTokenWriter();
                _serializer.Serialize(writer, obj);
                JsonHelper.FlattenJson(writer.Token!, new StringBuilder(), flattenedData);

                foreach (var kvp in flattenedData.Where(kvp => kvp.Value is long || kvp.Value is int || kvp.Value is double || kvp.Value is decimal))
                {
                    var pointParameters = new QuestHistoryPointDataParameter(
                        database,
                        shard,
                        room,
                        tick,
                        timestamp,
                        username,
                        kvp.Key.ToLower(),
                        Convert.ToDouble(kvp.Value));
                    AddPoint(pointParameters);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UploadHistoryData");
            }
        }

        public static void UploadUserHistoryData(string database, string shard, long tick, long timestamp, string username, object obj)
        {
            try
            {
                var flattenedData = new Dictionary<string, object?>();
                var writer = new JTokenWriter();
                _serializer.Serialize(writer, obj);
                JsonHelper.FlattenJson(writer.Token!, new StringBuilder(), flattenedData);

                foreach (var kvp in flattenedData.Where(kvp => kvp.Value is long || kvp.Value is int || kvp.Value is double || kvp.Value is decimal))
                {
                    var pointParameters = new QuestHistoryPointDataParameter(
                        database,
                        shard,
                        "",
                        tick,
                        timestamp,
                        username,
                        kvp.Key.ToLower(),
                        Convert.ToDouble(kvp.Value));
                    AddPoint(pointParameters);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UploadUserHistoryData");
            }
        }

        public static void UploadGlobalHistoryData(string database, string shard, long tick, long timestamp, object obj)
        {
            try
            {
                var flattenedData = new Dictionary<string, object?>();
                var writer = new JTokenWriter();
                _serializer.Serialize(writer, obj);
                JsonHelper.FlattenJson(writer.Token!, new StringBuilder(), flattenedData);

                foreach (var kvp in flattenedData.Where(kvp => kvp.Value is long || kvp.Value is int || kvp.Value is double || kvp.Value is decimal))
                {
                    var pointParameters = new QuestHistoryPointDataParameter(
                        database,
                        shard,
                        "",
                        tick,
                        timestamp,
                        "",
                        kvp.Key.ToLower(),
                        Convert.ToDouble(kvp.Value));
                    AddPoint(pointParameters);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UploadGlobalHistoryData");
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
                    var keyIncludesUsername = kvp.Key.Contains("users.", StringComparison.OrdinalIgnoreCase);
                    var username = keyIncludesUsername ? kvp.Key.Split('.')[1] : null;
                    var pointParameters = new QuestAdminUtilsPointDataParameter(
                        database,
                        username,
                        kvp.Key.ToLower(),
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
        public static (int, int, int, Dictionary<string, int>) GetStructureCounts(ScreepsRoomHistoryDto history)
        {
            var structureCounts = new Dictionary<string, int>();
            int structureCount = 0;
            int ownedStructureCount = 0;
            int neutralStructureCount = 0;

            if (history.Structures.Controller != null)
            {
                structureCounts["controller"] = 1;
                structureCount = 1;
                ownedStructureCount = 1;
            }

            if (history.Structures.Mineral != null)
            {
                structureCounts["mineral"] = 1;
            }
            if (history.Structures.Deposit != null)
            {
                structureCounts["deposit"] = 1;
            }
            if (history.Structures.Wall != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.Wall.Count));
                structureCounts["wall"] = count;
                structureCount += count;
                ownedStructureCount += count;
            }
            if (history.Structures.ConstructionSite != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.ConstructionSite.Count));
                structureCounts["constructionsite"] = count;
                structureCount += count;
                ownedStructureCount += count;
            }
            if (history.Structures.Container != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.Container.Count));
                structureCounts["container"] = count;
                structureCount += count;
                neutralStructureCount += count;
            }
            if (history.Structures.Extension != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.Extension.Count));
                structureCounts["extension"] = count;
                structureCount += count;
                ownedStructureCount += count;
            }
            if (history.Structures.Extractor != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.Extractor.Count));
                structureCounts["extractor"] = count;
                structureCount += count;
                ownedStructureCount += count;
            }
            if (history.Structures.Factory != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.Factory.Count));
                structureCounts["factory"] = count;
                structureCount += count;
                ownedStructureCount += count;
            }
            if (history.Structures.InvaderCore != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.InvaderCore.Count));
                structureCounts["invadercore"] = count;

            }
            if (history.Structures.KeeperLair != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.KeeperLair.Count));
                structureCounts["keeperlair"] = count;
            }
            if (history.Structures.Lab != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.Lab.Count));
                structureCounts["lab"] = count;
                structureCount += count;
                ownedStructureCount += count;
            }
            if (history.Structures.Link != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.Link.Count));
                structureCounts["link"] = count;
                structureCount += count;
                ownedStructureCount += count;
            }
            if (history.Structures.Observer != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.Observer.Count));
                structureCounts["observer"] = count;
                structureCount += count;
                ownedStructureCount += count;
            }
            if (history.Structures.Portal != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.Portal.Count));
                structureCounts["portal"] = count;
                structureCount += count;
                ownedStructureCount += count;
            }
            if (history.Structures.PowerBank != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.PowerBank.Count));
                structureCounts["powerbank"] = count;
                structureCount += count;
                ownedStructureCount += count;
            }
            if (history.Structures.PowerSpawn != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.PowerSpawn.Count));
                structureCounts["powerspawn"] = count;
                structureCount += count;
                ownedStructureCount += count;
            }
            if (history.Structures.Rampart != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.Rampart.Count));
                structureCounts["rampart"] = count;
                structureCount += count;
                ownedStructureCount += count;
            }
            if (history.Structures.Road != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.Road.Count));
                structureCounts["road"] = count;
                structureCount += count;
                neutralStructureCount += count;
            }
            if (history.Structures.Ruin != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.Ruin.Count));
                structureCounts["ruin"] = count;
                structureCount += count;
                neutralStructureCount += count;
            }
            if (history.Structures.Source != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.Source.Count));
                structureCounts["source"] = count;
                structureCount += count;
                neutralStructureCount += count;
            }
            if (history.Structures.Spawn != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.Spawn.Count));
                structureCounts["spawn"] = count;
                structureCount += count;
                ownedStructureCount += count;
            }
            if (history.Structures.Storage != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.Storage.Count));
                structureCounts["storage"] = count;
                structureCount += count;
                ownedStructureCount += count;
            }
            if (history.Structures.Terminal != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.Terminal.Count));
                structureCounts["terminal"] = count;
                structureCount += count;
                ownedStructureCount += count;
            }
            if (history.Structures.Tombstone != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.Tombstone.Count));
                structureCounts["tombstone"] = count;
                structureCount += count;
                neutralStructureCount += count;
            }
            if (history.Structures.Tower != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.Tower.Count));
                structureCounts["tower"] = count;
                structureCount += count;
                ownedStructureCount += count;
            }
            if (history.Structures.Nuker != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.Nuker.Count));
                structureCounts["nuker"] = count;
                structureCount += count;
                ownedStructureCount += count;
            }
            if (history.Structures.Nuke != null)
            {
                var count = Convert.ToInt32(Math.Floor(history.Structures.Nuke.Count));
                structureCounts["nuke"] = count;
            }

            return (structureCount, ownedStructureCount, neutralStructureCount, structureCounts);
        }

        public static (int, int, int, int) GetCreepCounts(ScreepsRoomHistoryDto history)
        {
            int ownedCreepCount = 0;
            int enemyCreepCount = 0;
            int otherCreepCount = 0;
            int powerCreepCount = 0;

            if (history.Creeps.OwnedCreeps != null)
            {
                ownedCreepCount += Convert.ToInt32(history.Creeps.OwnedCreeps.Count);
            }
            if (history.Creeps.EnemyCreeps != null)
            {
                enemyCreepCount += Convert.ToInt32(history.Creeps.EnemyCreeps.Count);
            }
            if (history.Creeps.OtherCreeps != null)
            {
                otherCreepCount += Convert.ToInt32(history.Creeps.OtherCreeps.Count);
            }
            if (history.Creeps.PowerCreeps != null)
            {
                powerCreepCount = Convert.ToInt32(history.Creeps.PowerCreeps.Count);
            }

            return (ownedCreepCount, enemyCreepCount, otherCreepCount, powerCreepCount);
        }
        private static (int, Dictionary<string, int>) GetCreepPartsCounts(CountByPartDto countByPart)
        {
            var creepPartsCounts = new Dictionary<string, int>();
            int creepPartsCount = 0;

            creepPartsCount += Convert.ToInt32(countByPart.Attack);
            creepPartsCounts["attack"] = Convert.ToInt32(countByPart.Attack);

            creepPartsCount += Convert.ToInt32(countByPart.Carry);
            creepPartsCounts["carry"] = Convert.ToInt32(countByPart.Carry);

            creepPartsCount += Convert.ToInt32(countByPart.Heal);
            creepPartsCounts["heal"] = Convert.ToInt32(countByPart.Heal);

            creepPartsCount += Convert.ToInt32(countByPart.Move);
            creepPartsCounts["move"] = Convert.ToInt32(countByPart.Move);

            creepPartsCount += Convert.ToInt32(countByPart.RangedAttack);
            creepPartsCounts["ranged_attack"] = Convert.ToInt32(countByPart.RangedAttack);

            creepPartsCount += Convert.ToInt32(countByPart.Tough);
            creepPartsCounts["tough"] = Convert.ToInt32(countByPart.Tough);

            creepPartsCount += Convert.ToInt32(countByPart.Work);
            creepPartsCounts["work"] = Convert.ToInt32(countByPart.Work);

            creepPartsCount += Convert.ToInt32(countByPart.Claim);
            creepPartsCounts["claim"] = Convert.ToInt32(countByPart.Claim);


            return (creepPartsCount, creepPartsCounts);
        }
        public static (int, Dictionary<string, int>) GetCreepPartsCounts(ScreepsRoomHistoryDto history)
        {
            var creepPartsCounts = new Dictionary<string, int>();
            int creepPartsCount = 0;

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
        private static (int, Dictionary<string, int>) GetCreepIntentsCounts(ActionLogDto actionLog)
        {
            var creepIntentsCounts = new Dictionary<string, int>();
            int creepIntentsCount = 0;

            creepIntentsCount += Convert.ToInt32(actionLog.Attack.Count);
            creepIntentsCounts["attack"] = Convert.ToInt32(actionLog.Attack.Count);

            creepIntentsCount += Convert.ToInt32(actionLog.Attacked.Count);
            creepIntentsCounts["attacked"] = Convert.ToInt32(actionLog.Attacked.Count);

            creepIntentsCount += Convert.ToInt32(actionLog.RangedAttack.Count);
            creepIntentsCounts["ranged_attack"] = Convert.ToInt32(actionLog.RangedAttack.Count);

            creepIntentsCount += Convert.ToInt32(actionLog.RangedMassAttack.Count);
            creepIntentsCounts["ranged_mass_attacked"] = Convert.ToInt32(actionLog.RangedMassAttack.Count);

            creepIntentsCount += Convert.ToInt32(actionLog.RangedHeal.Count);
            creepIntentsCounts["ranged_heal"] = Convert.ToInt32(actionLog.RangedHeal.Count);

            creepIntentsCount += Convert.ToInt32(actionLog.Heal.Count);
            creepIntentsCounts["heal"] = Convert.ToInt32(actionLog.Heal.Count);

            creepIntentsCount += Convert.ToInt32(actionLog.Healed.Count);
            creepIntentsCounts["healed"] = Convert.ToInt32(actionLog.Healed.Count);

            creepIntentsCount += Convert.ToInt32(actionLog.Harvest.Count);
            creepIntentsCounts["harvest"] = Convert.ToInt32(actionLog.Harvest.Count);

            creepIntentsCount += Convert.ToInt32(actionLog.Repair.Count);
            creepIntentsCounts["repair"] = Convert.ToInt32(actionLog.Repair.Count);

            creepIntentsCount += Convert.ToInt32(actionLog.Build.Count);
            creepIntentsCounts["build"] = Convert.ToInt32(actionLog.Build.Count);

            creepIntentsCount += Convert.ToInt32(actionLog.UpgradeController.Count);
            creepIntentsCounts["upgrade_controller"] = Convert.ToInt32(actionLog.UpgradeController.Count);

            creepIntentsCount += Convert.ToInt32(actionLog.Move.Count);
            creepIntentsCounts["move"] = Convert.ToInt32(actionLog.Move.Count);

            creepIntentsCount += Convert.ToInt32(actionLog.Say.Count);
            creepIntentsCounts["say"] = Convert.ToInt32(actionLog.Say.Count);

            creepIntentsCount += Convert.ToInt32(actionLog.ReserveController.Count);
            creepIntentsCounts["reserve_controller"] = Convert.ToInt32(actionLog.ReserveController.Count);

            creepIntentsCount += Convert.ToInt32(actionLog.AttackController.Count);
            creepIntentsCounts["attack_controller"] = Convert.ToInt32(actionLog.AttackController.Count);

            creepIntentsCount += Convert.ToInt32(actionLog.Produce.Count);
            creepIntentsCounts["produce"] = Convert.ToInt32(actionLog.Produce.Count);

            creepIntentsCount += Convert.ToInt32(actionLog.TransferEnergy.Count);
            creepIntentsCounts["transfer_energy"] = Convert.ToInt32(actionLog.TransferEnergy.Count);

            creepIntentsCount += Convert.ToInt32(actionLog.RunReaction.Count);
            creepIntentsCounts["run_reaction"] = Convert.ToInt32(actionLog.RunReaction.Count);

            creepIntentsCount += Convert.ToInt32(actionLog.ReverseReaction.Count);
            creepIntentsCounts["reverse_reaction"] = Convert.ToInt32(actionLog.ReverseReaction.Count);

            creepIntentsCount += Convert.ToInt32(actionLog.Spawned.Count);
            creepIntentsCounts["spawned"] = Convert.ToInt32(actionLog.Spawned.Count);

            creepIntentsCount += Convert.ToInt32(actionLog.Power.Count);
            creepIntentsCounts["power"] = Convert.ToInt32(actionLog.Power.Count);


            return (creepIntentsCount, creepIntentsCounts);
        }
        public static (int, Dictionary<string, int>) GetCreepIntentsCounts(ScreepsRoomHistoryDto history)
        {
            var creepIntentsCounts = new Dictionary<string, int>();
            int creepIntentsCount = 0;

            if (history.Creeps.OwnedCreeps != null)
            {
                var actionLog = history.Creeps.OwnedCreeps.ActionLog;
                var (count, counts) = GetCreepIntentsCounts(actionLog);
                creepIntentsCount += count;
                foreach (var kvp in counts)
                {
                    if (creepIntentsCounts.ContainsKey(kvp.Key))
                        creepIntentsCounts[kvp.Key] += kvp.Value;
                    else
                        creepIntentsCounts[kvp.Key] = kvp.Value;
                }
            }

            return (creepIntentsCount, creepIntentsCounts);
        }
        public static (int, int, int) GetRoomCounts(ScreepsRoomHistoryDto history)
        {
            int ownedRoomCount = 0;
            int reservedRoomCount = 0;
            int otherRoomCount = 0;

            if (history.Structures.Controller == null)
            {
                otherRoomCount = 1;
            }
            else
            {
                if (history.Structures.Controller.ReservationUserId != null)
                {
                    reservedRoomCount = 1;
                }
                else
                {
                    ownedRoomCount = 1;
                }
            }

            return (ownedRoomCount, reservedRoomCount, otherRoomCount);
        }
        public static (int, Dictionary<string, int>) GetStructureStoreCounts(Store store)
        {
            var storeTotals = new Dictionary<string, int>();
            int storeTotal = 0;

            void Add(string key, decimal? value)
            {
                if (value.HasValue)
                {
                    storeTotals[key] = (int)value.Value;
                    storeTotal += (int)value.Value;
                }
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
        public static (int, Dictionary<string, int>) GetStoreCounts(ScreepsRoomHistoryDto history)
        {
            var storeTotals = new Dictionary<string, int>();
            int storeTotal = 0;

            if (history.Structures.Storage != null)
            {
                var (total, totals) = GetStructureStoreCounts(history.Structures.Storage.Store);
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
                var (total, totals) = GetStructureStoreCounts(history.Structures.Terminal.Store);
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
                var (total, totals) = GetStructureStoreCounts(history.Structures.Container.Store);
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
                storeTotal += Convert.ToInt32(energyTotal);
                if (storeTotals.ContainsKey("energy"))
                {
                    storeTotals["energy"] += Convert.ToInt32(energyTotal);
                }
                else
                {
                    storeTotals["energy"] = Convert.ToInt32(energyTotal);
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

        private static QuestDBHistoryDTO GetQuestDBDto(ScreepsRoomHistoryDto screepsRoomHistory)
        {
            var (structureCount, ownedStructureCount, neutralStructureCount, structureCounts) = QuestDBDtoHelper.GetStructureCounts(screepsRoomHistory);
            var (ownedCreepCount, enemyCreepCount, otherCreepCount, powerCreepCount) = QuestDBDtoHelper.GetCreepCounts(screepsRoomHistory);
            var (ownedCreepPartsCount, ownedCreepPartsCounts) = QuestDBDtoHelper.GetCreepPartsCounts(screepsRoomHistory);
            var (creepIntentCount, creepIntentCounts) = QuestDBDtoHelper.GetCreepIntentsCounts(screepsRoomHistory);
            var (ownedRoomCount, reservedRoomCount, otherRoomCount) = QuestDBDtoHelper.GetRoomCounts(screepsRoomHistory);
            var (storeTotal, storeTotals) = QuestDBDtoHelper.GetStoreCounts(screepsRoomHistory);

            var questDBHistoryDTO = new QuestDBHistoryDTO()
            {
                StructureCount = structureCount,
                StructureCounts = structureCounts,
                OwnedStructureCount = ownedStructureCount,
                NeutralStructureCount = neutralStructureCount,

                OwnedCreepCount = ownedCreepCount,
                EnemyCreepCount = enemyCreepCount,
                OtherCreepCount = otherCreepCount,
                PowerCreepCount = powerCreepCount,
                OwnedCreepPartsCount = ownedCreepPartsCount,
                OwnedCreepPartsCounts = ownedCreepPartsCounts,
                CreepIntentCount = creepIntentCount,
                CreepIntentCounts = creepIntentCounts,

                OwnedRoomCount = ownedRoomCount,
                ReservedRoomCount = reservedRoomCount,
                OtherRoomCount = otherRoomCount,

                ControllerLevel = Convert.ToInt32(screepsRoomHistory.Structures.Controller?.Level ?? null),
                ControllerProgress = Convert.ToInt32(screepsRoomHistory.Structures.Controller?.Progress ?? null),
                ControllerProgressTotal = Convert.ToInt32(screepsRoomHistory.Structures.Controller?.ProgressTotal ?? null),
                ControllerPointsPerTick = Convert.ToInt32(screepsRoomHistory.Structures.Controller?.Upgraded ?? null),

                StoreTotal = Convert.ToInt32(storeTotal),
                StoreTotals = storeTotals
            };

            return questDBHistoryDTO;
        }

        public static async void WriteScreepsRoomHistory(string shard, string room, long tick, long timestamp, ScreepsRoomHistoryDto screepsRoomHistory)
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
                    var apiUser = await ScreepsApi.GetUser(userId);
                    if (apiUser != null)
                    {
                        GameState.Users.AddOrUpdate(userId, apiUser, (key, oldValue) => apiUser);
                        username = apiUser?.Username ?? "";
                    }
                }
                var database = $"{ConfigSettingsState.ServerName}_room_history";
                var questDBHistoryDTO = GetQuestDBDto(screepsRoomHistory);

                _logger.Information("Uploading room history data for {Shard}/{Room} at tick {Tick} (user: {Username})", shard, room, tick, username);
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
                seasonItem.Timestamp = DateTime.SpecifyKind(DateTime.ParseExact(seasonItem.Season, "yyyy-MM", null), DateTimeKind.Utc);
                var database = $"{ConfigSettingsState.ServerName}_historicalLeaderboard";
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
                var database = $"{ConfigSettingsState.ServerName}_currentLeaderboard";
                await QuestDBClientWriter.UploadLeaderboardData(database, seasonItem);
                _logger.Information("Current leaderboard data for user {User} in season {Season} uploaded successfully.", seasonItem.UserName, seasonItem.Season);
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
