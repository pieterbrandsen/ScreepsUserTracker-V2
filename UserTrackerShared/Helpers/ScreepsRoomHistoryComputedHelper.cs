using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Timers;
using UserTrackerShared.Models;
using Timer = System.Timers.Timer;

namespace UserTrackerShared.Helpers
{
    public static class FileWriterManager
    {
        private static readonly string HistoryDirectoryPath = @"C:\Users\Pieter\source\repos\ScreepsUserTracker-V2\UserTrackerConsole\Objects\History";
        private static readonly string KeysDirectoryPath = @"C:\Users\Pieter\source\repos\ScreepsUserTracker-V2\UserTrackerConsole\Objects\Keys";

        private static readonly ConcurrentDictionary<string, JObject> HistoryCache = new();
        private static readonly ConcurrentDictionary<string, JObject> KeyCache = new();

        private static Timer? _backgroundFlushTimer;
        public static bool StopFlushing;
        public static bool IsFlushing;

        static FileWriterManager()
        {
            Directory.CreateDirectory(HistoryDirectoryPath);
            Directory.CreateDirectory(KeysDirectoryPath);

            _backgroundFlushTimer = new Timer(10 * 1000);
            _backgroundFlushTimer.Elapsed += OnBackgroundFlushTimer;
            _backgroundFlushTimer.AutoReset = true;
            _backgroundFlushTimer.Enabled = true;
        }

        public static void GenerateFiles(string tick, string type, JObject obj, Dictionary<string, object> propertiesDict)
        {
            foreach (var propertyKVP in propertiesDict)
            {
                var key = $"{type}.{propertyKVP.Key}.{propertyKVP.Value.GetType()}";
                UpdateCache(tick, key, obj);
            }
        }

        public static void GenerateHistoryFile(JObject roomData)
        {
            var room = "";
            roomData.TryGetValue("room", out JToken? jTokenRoom);
            if (jTokenRoom != null) room = jTokenRoom.Value<string>();

            long baseTick = 0;
            roomData.TryGetValue("base", out JToken? jTokenBase);
            if (jTokenBase != null) baseTick = jTokenBase.Value<long>();

            string filePath = Path.Combine(HistoryDirectoryPath, $"{baseTick}/{room}.json");
            if (File.Exists(filePath)) return;
            HistoryCache.AddOrUpdate($"{baseTick}/{room}", _ => roomData, (_, existing) =>
            {
                return existing;
            });
        }

        private static void UpdateCache(string tick, string key, JObject obj)
        {
            string filePath = Path.Combine(KeysDirectoryPath, $"{key}.json");
            if (File.Exists(filePath) || KeyCache.ContainsKey(key)) return;

            KeyCache.AddOrUpdate(key, _ => obj, (_, existing) =>
            {
                return existing;
            });
        }

        private static async void OnBackgroundFlushTimer(Object? source, ElapsedEventArgs? e)
        {
            if (IsFlushing || StopFlushing) return;
            IsFlushing = true;
            try
            {
                SemaphoreSlim semaphore = new SemaphoreSlim(10000); // Limit concurrent writes to avoid overloading the disk

                IEnumerable<string> keyKeys;
                lock (KeyCache)
                {
                    keyKeys = KeyCache.Keys.ToArray();
                }

                var keysTasks = new List<Task>();
                foreach (var key in keyKeys)
                {
                    await semaphore.WaitAsync(); // Throttle writes
                    keysTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            if (KeyCache.TryRemove(key, out JObject obj))
                            {
                                if (obj == null) return;
                                string filePath = Path.Combine(KeysDirectoryPath, $"{key}.json");

                                var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
                                try
                                {
                                    await File.WriteAllTextAsync(filePath, json);
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Screen.AddLog($"Error writing file for key {key}: {ex.Message}");
                        }
                    }));
                }
                await Task.WhenAll(keysTasks);


                IEnumerable<string> historyKeys;
                lock (HistoryCache)
                {
                    // Extract keys upfront to minimize lock time
                    historyKeys = HistoryCache.Keys.ToArray();
                }

                var historyTasks = new List<Task>();
                foreach (var key in historyKeys)
                {
                    await semaphore.WaitAsync(); // Throttle writes
                    historyTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            if (HistoryCache.TryRemove(key, out JObject obj))
                            {
                                if (obj == null) return;
                                string[] parts = key.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

                                string tickDir = Path.Combine(HistoryDirectoryPath, parts[0]);
                                if (!Directory.Exists(tickDir))
                                {
                                    Directory.CreateDirectory(tickDir);
                                }
                                string filePath = Path.Combine(HistoryDirectoryPath, $"{key}.json");

                                var json = JsonConvert.SerializeObject(obj, Formatting.Indented);

                                try
                                {
                                    await File.WriteAllTextAsync(filePath, json);
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Screen.AddLog($"Error writing file for key {key}: {ex.Message}");
                        }
                    }));
                }
                await Task.WhenAll(historyTasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in BackgroundFlush: {ex.Message}");
            }
            IsFlushing = false;
        }
    }
    public static class ScreepsClassUpdater
    {
        private static readonly Dictionary<string, string> PropertyNameMapping = new()
        {
            { "_id", "Id" },
            { "_updated", "Updated" },
            { "effect", "EffectType" }
        };
        private static string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return char.ToUpper(input[0]) + input.Substring(1);
        }
        private static object GetDefaultValue(Type type)
        {
            if (type == typeof(string))
            {
                return ""; // Default to empty string for strings
            }

            return Activator.CreateInstance(type);
        }

        private static void SetNestedPropertyValue(ref object rootObj, string propertyPath, object value)
        {
            if (rootObj == null || string.IsNullOrEmpty(propertyPath))
                throw new ArgumentException("Invalid object or property path.");

            var properties = propertyPath.Split('.');
            rootObj = SetPropertyRecursively(rootObj, properties, 0, value);
        }
        private static object SetPropertyRecursively(object currentObj, string[] properties, int index, object value)
        {
            if (currentObj == null)
                throw new ArgumentNullException(nameof(currentObj), "Cannot navigate through a null object.");

            if (index >= properties.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "Property index out of range.");

            string property = properties[index];
            int numericKey = -1;

            // Check if property is numeric (for list/array indices)
            bool isNumericKey = int.TryParse(property, out numericKey);

            if (index == properties.Length - 1)
            {
                if (isNumericKey) CreateNewIndexItemIfNeeded(ref currentObj, numericKey);

                // Base case: Set the final property value
                if (isNumericKey && currentObj is IList list)
                {
                    list[numericKey] = value;
                }
                else if (currentObj is IDictionary dictionary)
                {
                    dictionary[property] = value;
                }
                else
                {
                    SetPropertyValue(currentObj, property, value);
                }
                return currentObj;
            }
            else
            {
                // Recursive case: Navigate deeper
                object nextObj = NavigateToNextLevel(ref currentObj, property);

                // Recursively set the value on the child
                var updatedChild = SetPropertyRecursively(nextObj, properties, index + 1, value);

                // Update the parent reference with the modified child
                if (isNumericKey && currentObj is IList parentList)
                {
                    parentList[numericKey] = updatedChild;
                }
                else if (currentObj is IDictionary parentDictionary)
                {
                    parentDictionary[property] = updatedChild;
                }
                else
                {
                    SetPropertyValue(currentObj, property, updatedChild);
                }

                return currentObj;
            }
        }
        private static void CreateNewIndexItemIfNeeded(ref object obj, int numericKey)
        {
            if (obj.GetType().IsArray)
            {
                // Handle array navigation
                var array = (Array)obj;
                if (numericKey >= array.Length)
                {
                    var elementType = array.GetType().GetElementType();
                    var newArray = Array.CreateInstance(elementType, numericKey + 1);
                    Array.Copy(array, newArray, array.Length);

                    for (int i = array.Length; i < numericKey + 1; i++)
                    {
                        newArray.SetValue(GetDefaultValue(elementType), i);
                    }
                    obj = newArray;
                }
            }
            else if (obj is IList list)
            {
                // Ensure the list is large enough
                while (numericKey >= list.Count)
                {
                    list.Add(GetDefaultValue(list.GetType().GetGenericArguments()[0]));
                }
            }
            else if (obj is IDictionary dict)
            {
                string stringKey = numericKey.ToString();
                if (!dict.Contains(stringKey))
                {
                    var valueType = dict.GetType().GetGenericArguments()[1];
                    dict[stringKey] = GetDefaultValue(valueType);
                }
            }
        }
        private static object NavigateToNextLevel(ref object obj, string property)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            // Check for numeric keys for lists/dictionaries
            if (int.TryParse(property, out int numericKey))
            {
                CreateNewIndexItemIfNeeded(ref obj, numericKey);
                if (obj.GetType().IsArray)
                {
                    return ((Array)obj).GetValue(numericKey);
                }
                else if (obj is IList list)
                {
                    return list[numericKey];
                }
                else if (obj is IDictionary dict)
                {
                    string stringKey = numericKey.ToString();
                    return dict[stringKey];
                }
                else
                {
                    throw new ArgumentException($"Property '{property}' as a numeric key is not applicable to type {obj.GetType()}.");
                }
            }

            // Navigate through regular object properties
            PropertyInfo propInfo = obj.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance);
            if (propInfo == null)
            {
                var capitalizedProperty = CapitalizeFirstLetter(property);
                propInfo = obj.GetType().GetProperty(capitalizedProperty, BindingFlags.Public | BindingFlags.Instance);
            }

            if (propInfo == null)
            {
                throw new ArgumentException($"Property '{property}' not found in {obj.GetType()}.");
            }

            var childObj = propInfo.GetValue(obj);

            if (childObj == null && propInfo.PropertyType != typeof(string))
            {
                childObj = GetDefaultValue(propInfo.PropertyType);
                propInfo.SetValue(obj, childObj);
            }

            return childObj;
        }
        private static void SetPropertyValue(object obj, string property, object value)
        {
            try
            {
                if (obj == null) throw new ArgumentNullException(nameof(obj));

                // Handle property name mapping
                property = PropertyNameMapping.TryGetValue(property, out var mappedName) ? mappedName : property;

                var objType = obj.GetType();
                PropertyInfo propInfo = objType.GetProperty(property, BindingFlags.Public | BindingFlags.Instance)
                                      ?? objType.GetProperty(CapitalizeFirstLetter(property), BindingFlags.Public | BindingFlags.Instance);

                if (propInfo == null)
                {
                    if (obj is string)
                    {
                        obj = (string)value;
                        return;
                    }
                    throw new ArgumentException($"Property '{property}' not found in {objType}.");
                }

                if (propInfo.PropertyType.IsClass && propInfo.GetValue(obj) == null &&
                    propInfo.PropertyType != typeof(string))
                {
                    propInfo.SetValue(obj, GetDefaultValue(propInfo.PropertyType));
                }

                // Handle null assignment for value types
                if (value == null)
                {
                    if (Nullable.GetUnderlyingType(propInfo.PropertyType) != null)
                    {
                        propInfo.SetValue(obj, null);
                    }
                    else if (propInfo.PropertyType.IsValueType)
                    {
                        propInfo.SetValue(obj, Activator.CreateInstance(propInfo.PropertyType));
                    }
                    else
                    {
                        propInfo.SetValue(obj, null);
                    }
                }
                else
                {
                    propInfo.SetValue(obj, Convert.ChangeType(value, Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType));
                }
            }
            catch (Exception)
            {
                if (obj == null) throw new ArgumentNullException(nameof(obj));

                // Handle property name mapping
                property = PropertyNameMapping.TryGetValue(property, out var mappedName) ? mappedName : property;

                var objType = obj.GetType();
                var a = string.Join(", ", objType.GetProperties().Select(p => p.Name));

                PropertyInfo propInfo = objType.GetProperty(property, BindingFlags.Public | BindingFlags.Instance)
                                      ?? objType.GetProperty(CapitalizeFirstLetter(property), BindingFlags.Public | BindingFlags.Instance);

                if (propInfo == null)
                {
                    if (obj is string)
                    {
                        obj = (string)value;
                        return;
                    }
                    throw new ArgumentException($"Property '{property}' not found in {objType}.");
                }

                if (propInfo.PropertyType.IsClass && propInfo.GetValue(obj) == null &&
                    propInfo.PropertyType != typeof(string))
                {
                    propInfo.SetValue(obj, GetDefaultValue(propInfo.PropertyType));
                }

                // Handle null assignment for value types
                if (value == null)
                {
                    if (Nullable.GetUnderlyingType(propInfo.PropertyType) != null)
                    {
                        propInfo.SetValue(obj, null);
                    }
                    else if (propInfo.PropertyType.IsValueType)
                    {
                        propInfo.SetValue(obj, Activator.CreateInstance(propInfo.PropertyType));
                    }
                    else
                    {
                        propInfo.SetValue(obj, null);
                    }
                }
                else
                {
                    propInfo.SetValue(obj, Convert.ChangeType(value, Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType));
                }
            }
        }
        public static void SetAllNestedPropertyValues(object obj, Dictionary<string, object> propertyLists)
        {
            foreach (var itemKVP in propertyLists)
            {
                try
                {
                    if (itemKVP.Key.StartsWith("effects")) continue;
                    SetNestedPropertyValue(ref obj, itemKVP.Key, itemKVP.Value);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to set property {itemKVP.Key} with {itemKVP.Value}: {ex.Message}");
                }
            }
        }
    }
    public static class ScreespRoomHistoryHelper
    {
        public static ScreepsRoomHistory UpdateRoomHistory(string key, ScreepsRoomHistory roomHistory, Dictionary<string, object> propertiesDict)
        {
            var type = propertiesDict.GetValueOrDefault("type") as string;
            if (type == null) type = roomHistory.TypeMap.GetValueOrDefault(key);
            else roomHistory.TypeMap.TryAdd(key, type);

            var user = propertiesDict.GetValueOrDefault("user") as string;
            if (user == null) user = roomHistory.UserMap.GetValueOrDefault(key);
            else roomHistory.UserMap.TryAdd(key, user);

            switch (type)
            {
                case "constructedWall":
                    if (!roomHistory.Structures.Walls.TryGetValue(key, out var wall))
                    {
                        wall = new StructureWall();
                        roomHistory.Structures.Walls[key] = wall;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(wall, propertiesDict);
                    break;
                case "constructionSite":
                    if (!roomHistory.Structures.ConstructionSites.TryGetValue(key, out var constructionSite))
                    {
                        constructionSite = new StructureConstructionSite();
                        roomHistory.Structures.ConstructionSites[key] = constructionSite;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(constructionSite, propertiesDict);
                    break;
                case "container":
                    if (!roomHistory.Structures.Containers.TryGetValue(key, out var container))
                    {
                        container = new StructureContainer();
                        roomHistory.Structures.Containers[key] = container;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(container, propertiesDict);
                    break;
                case "controller":
                    var controller = roomHistory.Structures.Controller ?? new StructureController();
                    ScreepsClassUpdater.SetAllNestedPropertyValues(controller, propertiesDict);
                    roomHistory.Structures.Controller = controller;
                    break;
                case "creep":
                    var hasController = roomHistory.Structures.Controller != null;
                    var isOwnCreep = hasController && roomHistory.Structures.Controller.User == user;
                    Creep creep;
                    if (hasController && isOwnCreep)
                    {
                        if (!roomHistory.Creeps.OwnedCreeps.TryGetValue(key, out creep))
                        {
                            creep = new Creep();
                            roomHistory.Creeps.OwnedCreeps[key] = creep;
                        }
                    }
                    else if (hasController && !isOwnCreep)
                    {
                        if (!roomHistory.Creeps.EnemyCreeps.TryGetValue(key, out creep))
                        {
                            creep = new Creep();
                            roomHistory.Creeps.EnemyCreeps[key] = creep;
                        }
                    }
                    else
                    {
                        if (!roomHistory.Creeps.OtherCreeps.TryGetValue(key, out creep))
                        {
                            creep = new Creep();
                            roomHistory.Creeps.OtherCreeps[key] = creep;
                        }
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(creep, propertiesDict);
                    break;
                case "deposit":
                    var deposit = roomHistory.Structures.Deposit ?? new StructureDepsoit();
                    ScreepsClassUpdater.SetAllNestedPropertyValues(deposit, propertiesDict);
                    roomHistory.Structures.Deposit = deposit;
                    break;
                case "energy":
                    if (!roomHistory.GroundResources.TryGetValue(key, out var resource))
                    {
                        resource = new GroundResource();
                        roomHistory.GroundResources[key] = resource;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(resource, propertiesDict);
                    break;
                case "extension":
                    if (!roomHistory.Structures.Extensions.TryGetValue(key, out var extension))
                    {
                        extension = new StructureExtension();
                        roomHistory.Structures.Extensions[key] = extension;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(extension, propertiesDict);
                    break;
                case "extractor":
                    if (!roomHistory.Structures.Extractors.TryGetValue(key, out var extractor))
                    {
                        extractor = new StructureExtractor();
                        roomHistory.Structures.Extractors[key] = extractor;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(extractor, propertiesDict);
                    break;
                case "factory":
                    if (!roomHistory.Structures.Factories.TryGetValue(key, out var factory))
                    {
                        factory = new StructureFactory();
                        roomHistory.Structures.Factories[key] = factory;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(factory, propertiesDict);
                    break;
                case "invaderCore":
                    if (!roomHistory.Structures.InvaderCores.TryGetValue(key, out var invaderCore))
                    {
                        invaderCore = new StructureInvaderCore();
                        roomHistory.Structures.InvaderCores[key] = invaderCore;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(invaderCore, propertiesDict);
                    break;
                case "keeperLair":
                    if (!roomHistory.Structures.KeeperLairs.TryGetValue(key, out var keeperLair))
                    {
                        keeperLair = new StructureKeeperLair();
                        roomHistory.Structures.KeeperLairs[key] = keeperLair;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(keeperLair, propertiesDict);
                    break;
                case "lab":
                    if (!roomHistory.Structures.Labs.TryGetValue(key, out var lab))
                    {
                        lab = new StructureLab();
                        roomHistory.Structures.Labs[key] = lab;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(lab, propertiesDict);
                    break;
                case "link":
                    if (!roomHistory.Structures.Links.TryGetValue(key, out var link))
                    {
                        link = new StructureLink();
                        roomHistory.Structures.Links[key] = link;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(link, propertiesDict);
                    break;
                case "mineral":
                    var mineral = roomHistory.Structures.Mineral ?? new StructureMineral();
                    ScreepsClassUpdater.SetAllNestedPropertyValues(mineral, propertiesDict);
                    roomHistory.Structures.Mineral = mineral;
                    break;
                case "nuker":
                    if (!roomHistory.Structures.Nukers.TryGetValue(key, out var nuker))
                    {
                        nuker = new StructureNuker();
                        roomHistory.Structures.Nukers[key] = nuker;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(nuker, propertiesDict);
                    break;
                case "observer":
                    if (!roomHistory.Structures.Observers.TryGetValue(key, out var observer))
                    {
                        observer = new StructureObserver();
                        roomHistory.Structures.Observers[key] = observer;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(observer, propertiesDict);
                    break;
                case "portal":
                    if (!roomHistory.Structures.Portals.TryGetValue(key, out var portal))
                    {
                        portal = new StructurePortal();
                        roomHistory.Structures.Portals[key] = portal;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(portal, propertiesDict);
                    break;
                case "powerBank":
                    if (!roomHistory.Structures.PowerBanks.TryGetValue(key, out var powerBank))
                    {
                        powerBank = new StructurePowerBank();
                        roomHistory.Structures.PowerBanks[key] = powerBank;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(powerBank, propertiesDict);
                    break;
                case "powerCreep":
                    if (!roomHistory.Creeps.PowerCreeps.TryGetValue(key, out var powerCreep))
                    {
                        powerCreep = new PowerCreep();
                        roomHistory.Creeps.PowerCreeps[key] = powerCreep;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(powerCreep, propertiesDict);
                    break;
                case "powerSpawn":
                    if (!roomHistory.Structures.PowerSpawns.TryGetValue(key, out var powerSpawn))
                    {
                        powerSpawn = new StructurePowerSpawn();
                        roomHistory.Structures.PowerSpawns[key] = powerSpawn;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(powerSpawn, propertiesDict);
                    break;
                case "rampart":
                    if (!roomHistory.Structures.Ramparts.TryGetValue(key, out var rampart))
                    {
                        rampart = new StructureRampart();
                        roomHistory.Structures.Ramparts[key] = rampart;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(rampart, propertiesDict);
                    break;
                case "road":
                    if (!roomHistory.Structures.Roads.TryGetValue(key, out var road))
                    {
                        road = new StructureRoad();
                        roomHistory.Structures.Roads[key] = road;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(road, propertiesDict);
                    break;
                case "ruin":
                    if (!roomHistory.Structures.Ruins.TryGetValue(key, out var ruin))
                    {
                        ruin = new StructureRuin();
                        roomHistory.Structures.Ruins[key] = ruin;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(ruin, propertiesDict);
                    break;
                case "source":
                    if (!roomHistory.Structures.Sources.TryGetValue(key, out var source))
                    {
                        source = new StructureSource();
                        roomHistory.Structures.Sources[key] = source;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(source, propertiesDict);
                    break;
                case "spawn":
                    if (!roomHistory.Structures.Spawns.TryGetValue(key, out var spawn))
                    {
                        spawn = new StructureSpawn();
                        roomHistory.Structures.Spawns[key] = spawn;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(spawn, propertiesDict);
                    break;
                case "storage":
                    if (!roomHistory.Structures.Storages.TryGetValue(key, out var storage))
                    {
                        storage = new StructureStorage();
                        roomHistory.Structures.Storages[key] = storage;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(storage, propertiesDict);
                    break;
                case "terminal":
                    if (!roomHistory.Structures.Terminals.TryGetValue(key, out var terminal))
                    {
                        terminal = new StructureTerminal();
                        roomHistory.Structures.Terminals[key] = terminal;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(terminal, propertiesDict);
                    break;
                case "tombstone":
                    if (!roomHistory.Structures.Tombstones.TryGetValue(key, out var tombstone))
                    {
                        tombstone = new StructureTombstone();
                        roomHistory.Structures.Tombstones[key] = tombstone;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(tombstone, propertiesDict);
                    break;
                case "tower":
                    if (!roomHistory.Structures.Towers.TryGetValue(key, out var tower))
                    {
                        tower = new StructureTower();
                        roomHistory.Structures.Towers[key] = tower;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(tower, propertiesDict);
                    break;
                case "nuke":
                    if (!roomHistory.Structures.Nukes.TryGetValue(key, out var nuke))
                    {
                        nuke = new StructureNuke();
                        roomHistory.Structures.Nukes[key] = nuke;
                    }
                    ScreepsClassUpdater.SetAllNestedPropertyValues(nuke, propertiesDict);
                    break;
                default:
                    Debug.WriteLine(type);
                    throw new Exception("Unknown type");
            }

            return roomHistory;
        }

        public static ScreepsRoomHistory UpdateRoomHistory(string key, ScreepsRoomHistory roomHistory, JObject obj)
        {
            obj.TryGetValue("type", out JToken? typeToken);
            var type = typeToken?.Value<string>() ?? "";

            obj.TryGetValue("user", out JToken? userToken);
            var user = userToken?.Value<string>();

            roomHistory.TypeMap.TryAdd(key, type);
            if (user != null) roomHistory.UserMap.TryAdd(key, user);

            switch (type)
            {
                case "constructedWall":
                    var wall = obj.ToObject<StructureWall>();
                    roomHistory.Structures.Walls[key] = wall;
                    break;
                case "constructionSite":
                    var constructionSite = obj.ToObject<StructureConstructionSite>();
                    roomHistory.Structures.ConstructionSites[key] = constructionSite;
                    break;
                case "container":
                    var container = obj.ToObject<StructureContainer>();
                    roomHistory.Structures.Containers[key] = container;
                    break;
                case "controller":
                    var controller = obj.ToObject<StructureController>();
                    roomHistory.Structures.Controller = controller;
                    break;
                case "creep":
                    var hasController = roomHistory.Structures.Controller != null;
                    var isOwnCreep = hasController && roomHistory.Structures.Controller?.User == user;
                    Creep creep;
                    if (hasController && isOwnCreep)
                    {
                        creep = obj.ToObject<Creep>();
                        roomHistory.Creeps.OwnedCreeps[key] = creep;
                    }
                    else if (hasController && !isOwnCreep)
                    {
                        creep = obj.ToObject<Creep>();
                        roomHistory.Creeps.EnemyCreeps[key] = creep;
                    }
                    else
                    {
                        creep = obj.ToObject<Creep>();
                        roomHistory.Creeps.OtherCreeps[key] = creep;
                    }
                    break;
                case "deposit":
                    var deposit = obj.ToObject<StructureDepsoit>();
                    roomHistory.Structures.Deposit = deposit;
                    break;
                case "energy":
                    var resource = obj.ToObject<GroundResource>();
                    roomHistory.GroundResources[key] = resource;
                    break;
                case "extension":
                    var extension = obj.ToObject<StructureExtension>();
                    roomHistory.Structures.Extensions[key] = extension;
                    break;
                case "extractor":
                    var extractor = obj.ToObject<StructureExtractor>();
                    roomHistory.Structures.Extractors[key] = extractor;
                    break;
                case "factory":
                    var factory = obj.ToObject<StructureFactory>();
                    roomHistory.Structures.Factories[key] = factory;
                    break;
                case "invaderCore":
                    var invaderCore = obj.ToObject<StructureInvaderCore>();
                    roomHistory.Structures.InvaderCores[key] = invaderCore;
                    break;
                case "keeperLair":
                    var keeperLair = obj.ToObject<StructureKeeperLair>();
                    roomHistory.Structures.KeeperLairs[key] = keeperLair;
                    break;
                case "lab":
                    var lab = obj.ToObject<StructureLab>();
                    roomHistory.Structures.Labs[key] = lab;
                    break;
                case "link":
                    var link = obj.ToObject<StructureLink>();
                    roomHistory.Structures.Links[key] = link;
                    break;
                case "mineral":
                    var mineral = obj.ToObject<StructureMineral>();
                    roomHistory.Structures.Mineral = mineral;
                    break;
                case "nuker":
                    var nuker = obj.ToObject<StructureNuker>();
                    roomHistory.Structures.Nukers[key] = nuker;
                    break;
                case "observer":
                    var observer = obj.ToObject<StructureObserver>();
                    roomHistory.Structures.Observers[key] = observer;
                    break;
                case "portal":
                    var portal = obj.ToObject<StructurePortal>();
                    roomHistory.Structures.Portals[key] = portal;
                    break;
                case "powerBank":
                    var powerBank = obj.ToObject<StructurePowerBank>();
                    roomHistory.Structures.PowerBanks[key] = powerBank;
                    break;
                case "powerCreep":
                    var powerCreep = obj.ToObject<PowerCreep>();
                    roomHistory.Creeps.PowerCreeps[key] = powerCreep;
                    break;
                case "powerSpawn":
                    var powerSpawn = obj.ToObject<StructurePowerSpawn>();
                    roomHistory.Structures.PowerSpawns[key] = powerSpawn;
                    break;
                case "rampart":
                    var rampart = obj.ToObject<StructureRampart>();
                    roomHistory.Structures.Ramparts[key] = rampart;
                    break;
                case "road":
                    var road = obj.ToObject<StructureRoad>();
                    roomHistory.Structures.Roads[key] = road;
                    break;
                case "ruin":
                    var ruin = obj.ToObject<StructureRuin>();
                    roomHistory.Structures.Ruins[key] = ruin;
                    break;
                case "source":
                    var source = obj.ToObject<StructureSource>();
                    roomHistory.Structures.Sources[key] = source;
                    break;
                case "spawn":
                    var spawn = obj.ToObject<StructureSpawn>();
                    roomHistory.Structures.Spawns[key] = spawn;
                    break;
                case "storage":
                    var storage = obj.ToObject<StructureStorage>();
                    roomHistory.Structures.Storages[key] = storage;
                    break;
                case "terminal":
                    var terminal = obj.ToObject<StructureTerminal>();
                    roomHistory.Structures.Terminals[key] = terminal;
                    break;
                case "tombstone":
                    var tombstone = obj.ToObject<StructureTombstone>();
                    roomHistory.Structures.Tombstones[key] = tombstone;
                    break;
                case "tower":
                    var tower = obj.ToObject<StructureTower>();
                    roomHistory.Structures.Towers[key] = tower;
                    break;
                case "nuke":
                    var nuke = obj.ToObject<StructureNuke>();
                    roomHistory.Structures.Nukes[key] = nuke;
                    break;
                default:
                    Debug.WriteLine(type);
                    throw new Exception("Unknown type");
            }

            return roomHistory;
        }

        public static ScreepsRoomHistory RemoveFromRoomHistory(string key, ScreepsRoomHistory roomHistory)
        {
            var type = roomHistory.TypeMap.GetValueOrDefault(key);
            switch (type)
            {
                case "constructedWall":
                    roomHistory.Structures.Walls.Remove(key);
                    break;
                case "constructionSite":
                    roomHistory.Structures.ConstructionSites.Remove(key);
                    break;
                case "container":
                    roomHistory.Structures.Containers.Remove(key);
                    break;
                case "controller":
                    if (roomHistory.Structures.Controller != null && roomHistory.Structures.Controller.Id == key)
                        roomHistory.Structures.Controller = null;
                    break;
                case "creep":
                    roomHistory.Creeps.OwnedCreeps.Remove(key);
                    roomHistory.Creeps.EnemyCreeps.Remove(key);
                    roomHistory.Creeps.OtherCreeps.Remove(key);
                    break;
                case "deposit":
                    if (roomHistory.Structures.Deposit != null && roomHistory.Structures.Deposit.Id == key)
                        roomHistory.Structures.Deposit = null;
                    break;
                case "energy":
                    roomHistory.GroundResources.Remove(key);
                    break;
                case "extension":
                    roomHistory.Structures.Extensions.Remove(key);
                    break;
                case "extractor":
                    roomHistory.Structures.Extractors.Remove(key);
                    break;
                case "factory":
                    roomHistory.Structures.Factories.Remove(key);
                    break;
                case "invaderCore":
                    roomHistory.Structures.InvaderCores.Remove(key);
                    break;
                case "keeperLair":
                    roomHistory.Structures.KeeperLairs.Remove(key);
                    break;
                case "lab":
                    roomHistory.Structures.Labs.Remove(key);
                    break;
                case "link":
                    roomHistory.Structures.Links.Remove(key);
                    break;
                case "mineral":
                    if (roomHistory.Structures.Mineral != null && roomHistory.Structures.Mineral.Id == key)
                        roomHistory.Structures.Mineral = null;
                    break;
                case "nuker":
                    roomHistory.Structures.Nukers.Remove(key);
                    break;
                case "observer":
                    roomHistory.Structures.Observers.Remove(key);
                    break;
                case "portal":
                    roomHistory.Structures.Portals.Remove(key);
                    break;
                case "powerBank":
                    roomHistory.Structures.PowerBanks.Remove(key);
                    break;
                case "powerCreep":
                    roomHistory.Creeps.PowerCreeps.Remove(key);
                    break;
                case "powerSpawn":
                    roomHistory.Structures.PowerSpawns.Remove(key);
                    break;
                case "rampart":
                    roomHistory.Structures.Ramparts.Remove(key);
                    break;
                case "road":
                    roomHistory.Structures.Roads.Remove(key);
                    break;
                case "ruin":
                    roomHistory.Structures.Ruins.Remove(key);
                    break;
                case "source":
                    roomHistory.Structures.Sources.Remove(key);
                    break;
                case "spawn":
                    roomHistory.Structures.Spawns.Remove(key);
                    break;
                case "storage":
                    roomHistory.Structures.Storages.Remove(key);
                    break;
                case "terminal":
                    roomHistory.Structures.Terminals.Remove(key);
                    break;
                case "tombstone":
                    roomHistory.Structures.Tombstones.Remove(key);
                    break;
                case "tower":
                    roomHistory.Structures.Towers.Remove(key);
                    break;
                case "nuke":
                    roomHistory.Structures.Nukes.Remove(key);
                    break;
                default:
                    break;
            }

            roomHistory.UserMap.Remove(key);
            roomHistory.TypeMap.Remove(key);
            return roomHistory;
        }
        private static void FlattenJson(JToken token, StringBuilder currentPath, IDictionary<string, object> dict)
        {
            switch (token)
            {
                case JObject obj:
                    foreach (var prop in obj.Properties())
                    {
                        int initialLen = currentPath.Length;
                        currentPath.Append($"{prop.Name}.");
                        FlattenJson(prop.Value, currentPath, dict);
                        currentPath.Length = initialLen; // Reset path
                    }
                    break;

                case JArray array:
                    for (int i = 0; i < array.Count; i++)
                    {
                        int initialLen = currentPath.Length;
                        currentPath.Append($"{i}.");
                        FlattenJson(array[i], currentPath, dict);
                        currentPath.Length = initialLen; // Reset path
                    }
                    break;

                case JValue jValue:
                    if (currentPath.Length > 0)
                        currentPath.Length--; // Remove trailing "."
                    dict[currentPath.ToString()] = jValue.Value; // Directly use Value
                    break;
            }
        }
        public static ReadOnlySpan<char> GetLastPathSegment(ReadOnlySpan<char> path)
        {
            int len = path.Length;
            for (int i = len - 1; i >= 0; i--)
            {
                if (path[i] == '.')
                    return path.Slice(i + 1); // No allocation, just a slice reference
            }
            return path; // If no dot is found, return the full string
        }
        public static ScreepsRoomHistory ComputeTick(JToken tickObject, ScreepsRoomHistory roomHistory)
        {
            var objDictionary = new Dictionary<string, JObject>();
            var propertiesControllerDict = new Dictionary<string, object>();
            roomHistory.PropertiesListDictionary = new Dictionary<string, Dictionary<string, object>>();

            // Avoid unnecessary ToList() allocation
            var tickObjects = tickObject.Children().SelectMany(c => c.Children());

            foreach (var tickObj in tickObjects)
            {
                var id = GetLastPathSegment(tickObj.Path).ToString();
                if (tickObj.HasValues && tickObj is JObject obj)
                {
                    if (roomHistory.TypeMap.TryGetValue(id, out var type))
                    {
                        type ??= "unknown";
                        if (!roomHistory.PropertiesListDictionary.TryGetValue(id, out var propertiesDict))
                        {
                            propertiesDict = new Dictionary<string, object>();
                            roomHistory.PropertiesListDictionary[id] = propertiesDict;
                        }
                        FlattenJson(obj, new StringBuilder(), propertiesDict);

                        if (ConfigSettingsState.WriteHistoryProperties)
                        {
                            FileWriterManager.GenerateFiles(roomHistory.Tick.ToString(), type, obj, propertiesDict);
                        }
                    }
                    else
                    {
                        type = obj.GetValue("type")?.Value<string>() ?? "unknown";
                        objDictionary[id] = obj;

                        if (type == "controller" && roomHistory.Structures.Controller == null)
                        {
                            FlattenJson(obj, new StringBuilder(), propertiesControllerDict);
                        }
                        else if (ConfigSettingsState.WriteHistoryProperties || ConfigSettingsState.RunningHistoryTested)
                        {
                            var propertiesDict = new Dictionary<string, object>();
                            FlattenJson(obj, new StringBuilder(), propertiesDict);
                            if (ConfigSettingsState.WriteHistoryProperties)
                                FileWriterManager.GenerateFiles(roomHistory.Tick.ToString(), type, obj, propertiesDict);
                        }
                    }
                }
                else
                {
                    roomHistory = RemoveFromRoomHistory(id, roomHistory);
                }
            }

            if (propertiesControllerDict.Count > 0)
            {
                roomHistory = UpdateRoomHistory(propertiesControllerDict["_id"].ToString(), roomHistory, propertiesControllerDict);
            }

            foreach (var (key, propertyLists) in roomHistory.PropertiesListDictionary)
            {
                if (!string.IsNullOrEmpty(key) && key != "undefined")
                {
                    roomHistory = UpdateRoomHistory(key, roomHistory, propertyLists);
                }
            }
            foreach (var (key, obj) in objDictionary)
            {
                roomHistory = UpdateRoomHistory(key, roomHistory, obj);
            }

            return roomHistory;
        }
    }
}
