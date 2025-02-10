using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Timers;
using UserTrackerShared.Models;
using Timer = System.Timers.Timer;

namespace UserTrackerShared.Helpers
{
    public class PropertiesList
    {
        public Dictionary<string, string> StringProperties { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, long> IntegerProperties { get; set; } = new Dictionary<string, long>();
        public Dictionary<string, bool> BooleanProperties { get; set; } = new Dictionary<string, bool>();
        public List<string> NullProperties { get; set; } = new List<string>();
    }

    public static class FileWriterManager
    {
        private static readonly string HistoryDirectoryPath = @"C:\Users\Pieter\source\repos\ScreepsUserTracker-V2\UserTrackerConsole\Objects\History";
        private static readonly string KeysDirectoryPath = @"C:\Users\Pieter\source\repos\ScreepsUserTracker-V2\UserTrackerConsole\Objects\Keys";
        private static readonly string TypesDirectoryPath = @"C:\Users\Pieter\source\repos\ScreepsUserTracker-V2\UserTrackerConsole\Objects\Types";

        private static readonly ConcurrentDictionary<string, JObject> HistoryCache = new();
        private static readonly ConcurrentDictionary<string, JObject> KeyCache = new();
        private static readonly ConcurrentDictionary<string, JObject> TypeCache = new();

        private static Timer? _backgroundFlushTimer;
        public static bool StopFlushing;
        public static bool IsFlushing;

        static FileWriterManager()
        {
            // Ensure directories exist
            Directory.CreateDirectory(KeysDirectoryPath);
            Directory.CreateDirectory(TypesDirectoryPath);

            _backgroundFlushTimer = new Timer(10 * 1000);
            _backgroundFlushTimer.Elapsed += OnBackgroundFlushTimer;
            _backgroundFlushTimer.AutoReset = true;
            _backgroundFlushTimer.Enabled = true;
        }

        public static void GenerateFiles(string tick, string type, JObject obj, PropertiesList propertiesList)
        {
            foreach (var prop in propertiesList.NullProperties)
            {
                var key = $"{type}.{prop}.null";
                UpdateCache(tick, key, obj);
            }
            foreach (var prop in propertiesList.BooleanProperties)
            {
                var key = $"{type}.{prop.Key}.bool";
                UpdateCache(tick, key, obj);
            }
            foreach (var prop in propertiesList.StringProperties)
            {
                var key = $"{type}.{prop.Key}.string";
                UpdateCache(tick, key, obj);
            }
            foreach (var prop in propertiesList.IntegerProperties)
            {
                var key = $"{type}.{prop.Key}.int";
                UpdateCache(tick, key, obj);
            }
        }

        public static void GenerateFileByType(string type, JObject obj)
        {
            //TypeCache.AddOrUpdate(type, _ =>
            //{
            //    string filePath = Path.Combine(TypesDirectoryPath, $"{type}.json");
            //    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            //    using (StreamReader reader = new StreamReader(fs))
            //    {
            //        string json = reader.ReadToEnd();
            //        JObject obj1 = JObject.Parse(json);

            //        obj1.Merge(obj, new JsonMergeSettings
            //        {
            //            MergeArrayHandling = MergeArrayHandling.Union // Prevents duplicate array values
            //        });
            //        return obj1;
            //    }
            //}, (_, existing) =>
            //{
            //    var json = JsonConvert.SerializeObject(existing);
            //    JObject obj1 = JObject.Parse(json);

            //    obj1.Merge(obj, new JsonMergeSettings
            //    {
            //        MergeArrayHandling = MergeArrayHandling.Union // Prevents duplicate array values
            //    });

            //    return obj1;
            //});
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

                IEnumerable<string> typeKeys;
                lock (TypeCache)
                {
                    typeKeys = TypeCache.Keys.ToArray();
                }

                var typeTasks = new List<Task>();
                foreach (var key in typeKeys)
                {
                    await semaphore.WaitAsync(); // Throttle writes
                    typeTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            if (TypeCache.TryRemove(key, out JObject obj))
                            {
                                if (obj == null) return;
                                string filePath = Path.Combine(TypesDirectoryPath, $"{key}.json");

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
                            Screen.AddLog($"Error writing file for type {key}: {ex.Message}");
                        }
                    }));
                }
                await Task.WhenAll(typeTasks);


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
    internal static class ConvertJObjectToHistory
    {
        private static readonly Dictionary<string, string> PropertyNameMapping = new()
        {
            { "_id", "Id" },
            { "_updated", "Updated" },
            { "effect", "EffectType" }
        };

        private static readonly HashSet<Type> NonInstantiableTypes = new()
        {
            typeof(string),
            typeof(Uri),
            typeof(DateTime),
            typeof(Guid),
            typeof(TimeSpan)
        };
        private static string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return char.ToUpper(input[0]) + input.Substring(1);
        }
        private static string DeCapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return char.ToLower(input[0]) + input.Substring(1);
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
            else if (obj is System.Collections.IList list)
            {
                // Ensure the list is large enough
                while (numericKey >= list.Count)
                {
                    list.Add(GetDefaultValue(list.GetType().GetGenericArguments()[0]));
                }
            }
            else if (obj is System.Collections.IDictionary dict)
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
                else if (obj is System.Collections.IList list)
                {
                    return list[numericKey];
                }
                else if (obj is System.Collections.IDictionary dict)
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

            if (childObj == null && !NonInstantiableTypes.Contains(propInfo.PropertyType))
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
                                      ?? objType.GetProperty(CapitalizeFirstLetter(property), BindingFlags.Public | BindingFlags.Instance)
                                      ?? objType.GetProperty(DeCapitalizeFirstLetter(property), BindingFlags.Public | BindingFlags.Instance);

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
                    !NonInstantiableTypes.Contains(propInfo.PropertyType))
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
                                      ?? objType.GetProperty(CapitalizeFirstLetter(property), BindingFlags.Public | BindingFlags.Instance)
                                      ?? objType.GetProperty(DeCapitalizeFirstLetter(property), BindingFlags.Public | BindingFlags.Instance);

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
                    !NonInstantiableTypes.Contains(propInfo.PropertyType))
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

        private static void SetAllNestedPropertyValues(object obj, PropertiesList propertyLists)
        {
            foreach (var item in propertyLists.NullProperties)
            {
                try
                {
                    SetNestedPropertyValue(ref obj, item, null); // Set null for properties in NullProperties
                }
                catch (Exception ex)
                {
                    //FileWriterManager.StopFlushing = true;
                    //while (FileWriterManager.IsFlushing)
                    //{
                    //    Thread.Sleep(5000);
                    //}
                    throw new Exception($"Failed to set null property {item}: {ex.Message}");
                }
            }

            foreach (var kvp in propertyLists.StringProperties)
            {
                try
                {
                    SetNestedPropertyValue(ref obj, kvp.Key, kvp.Value); // Set string values
                }
                catch (Exception ex)
                {
                    //FileWriterManager.StopFlushing = true;
                    //while (FileWriterManager.IsFlushing)
                    //{
                    //    Thread.Sleep(5000);
                    //}
                    throw new Exception($"Failed to set string property {kvp.Key}: {ex.Message}");
                }
            }

            foreach (var kvp in propertyLists.IntegerProperties)
            {
                try
                {
                    SetNestedPropertyValue(ref obj, kvp.Key, kvp.Value); // Set integer values
                }
                catch (Exception ex)
                {
                    //FileWriterManager.StopFlushing = true;
                    //while (FileWriterManager.IsFlushing)
                    //{
                    //    Thread.Sleep(5000);
                    //}
                    throw new Exception($"Failed to set integer property {kvp.Key}: {ex.Message}");
                }
            }

            foreach (var kvp in propertyLists.BooleanProperties)
            {
                try
                {
                    SetNestedPropertyValue(ref obj, kvp.Key, kvp.Value); // Set boolean values
                }
                catch (Exception ex)
                {
                    //FileWriterManager.StopFlushing = true;
                    //while (FileWriterManager.IsFlushing)
                    //{
                    //    Thread.Sleep(5000);
                    //}
                    throw new Exception($"Failed to set boolean property {kvp.Key}: {ex.Message}");
                }
            }
        }

        public static ScreepsRoomHistory UpdateRoomHistory(string key, ScreepsRoomHistory roomHistory, PropertiesList propertyLists)
        {
            var type = propertyLists.StringProperties.GetValueOrDefault("type");
            if (type == null) type = roomHistory.TypeMap.GetValueOrDefault(key);
            else roomHistory.TypeMap.TryAdd(key, type);
            var user = propertyLists.StringProperties.GetValueOrDefault("user");
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
                    SetAllNestedPropertyValues(wall, propertyLists);
                    break;
                case "constructionSite":
                    if (!roomHistory.Structures.ConstructionSites.TryGetValue(key, out var constructionSite))
                    {
                        constructionSite = new StructureConstructionSite();
                        roomHistory.Structures.ConstructionSites[key] = constructionSite;
                    }
                    SetAllNestedPropertyValues(constructionSite, propertyLists);
                    break;
                case "container":
                    if (!roomHistory.Structures.Containers.TryGetValue(key, out var container))
                    {
                        container = new StructureContainer();
                        roomHistory.Structures.Containers[key] = container;
                    }
                    SetAllNestedPropertyValues(container, propertyLists);
                    break;
                case "controller":
                    var controller = roomHistory.Structures.Controller ?? new StructureController();
                    SetAllNestedPropertyValues(controller, propertyLists);
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
                    SetAllNestedPropertyValues(creep, propertyLists);
                    break;
                case "deposit":
                    var deposit = roomHistory.Structures.Deposit ?? new StructureDepsoit();
                    SetAllNestedPropertyValues(deposit, propertyLists);
                    roomHistory.Structures.Deposit = deposit;
                    break;
                case "energy":
                    if (!roomHistory.GroundResources.TryGetValue(key, out var resource))
                    {
                        resource = new GroundResource();
                        roomHistory.GroundResources[key] = resource;
                    }
                    SetAllNestedPropertyValues(resource, propertyLists);
                    break;
                case "extension":
                    if (!roomHistory.Structures.Extensions.TryGetValue(key, out var extension))
                    {
                        extension = new StructureExtension();
                        roomHistory.Structures.Extensions[key] = extension;
                    }
                    SetAllNestedPropertyValues(extension, propertyLists);
                    break;
                case "extractor":
                    if (!roomHistory.Structures.Extractors.TryGetValue(key, out var extractor))
                    {
                        extractor = new StructureExtractor();
                        roomHistory.Structures.Extractors[key] = extractor;
                    }
                    SetAllNestedPropertyValues(extractor, propertyLists);
                    break;
                case "factory":
                    if (!roomHistory.Structures.Factories.TryGetValue(key, out var factory))
                    {
                        factory = new StructureFactory();
                        roomHistory.Structures.Factories[key] = factory;
                    }
                    SetAllNestedPropertyValues(factory, propertyLists);
                    break;
                case "invaderCore":
                    if (!roomHistory.Structures.InvaderCores.TryGetValue(key, out var invaderCore))
                    {
                        invaderCore = new StructureInvaderCore();
                        roomHistory.Structures.InvaderCores[key] = invaderCore;
                    }
                    SetAllNestedPropertyValues(invaderCore, propertyLists);
                    break;
                case "keeperLair":
                    if (!roomHistory.Structures.KeeperLairs.TryGetValue(key, out var keeperLair))
                    {
                        keeperLair = new StructureKeeperLair();
                        roomHistory.Structures.KeeperLairs[key] = keeperLair;
                    }
                    SetAllNestedPropertyValues(keeperLair, propertyLists);
                    break;
                case "lab":
                    if (!roomHistory.Structures.Labs.TryGetValue(key, out var lab))
                    {
                        lab = new StructureLab();
                        roomHistory.Structures.Labs[key] = lab;
                    }
                    SetAllNestedPropertyValues(lab, propertyLists);
                    break;
                case "link":
                    if (!roomHistory.Structures.Links.TryGetValue(key, out var link))
                    {
                        link = new StructureLink();
                        roomHistory.Structures.Links[key] = link;
                    }
                    SetAllNestedPropertyValues(link, propertyLists);
                    break;
                case "mineral":
                    var mineral = roomHistory.Structures.Mineral ?? new StructureMineral();
                    SetAllNestedPropertyValues(mineral, propertyLists);
                    roomHistory.Structures.Mineral = mineral;
                    break;
                case "nuker":
                    if (!roomHistory.Structures.Nukers.TryGetValue(key, out var nuker))
                    {
                        nuker = new StructureNuker();
                        roomHistory.Structures.Nukers[key] = nuker;
                    }
                    SetAllNestedPropertyValues(nuker, propertyLists);
                    break;
                case "observer":
                    if (!roomHistory.Structures.Observers.TryGetValue(key, out var observer))
                    {
                        observer = new StructureObserver();
                        roomHistory.Structures.Observers[key] = observer;
                    }
                    SetAllNestedPropertyValues(observer, propertyLists);
                    break;
                case "portal":
                    if (!roomHistory.Structures.Portals.TryGetValue(key, out var portal))
                    {
                        portal = new StructurePortal();
                        roomHistory.Structures.Portals[key] = portal;
                    }
                    SetAllNestedPropertyValues(portal, propertyLists);
                    break;
                case "powerBank":
                    if (!roomHistory.Structures.PowerBanks.TryGetValue(key, out var powerBank))
                    {
                        powerBank = new StructurePowerBank();
                        roomHistory.Structures.PowerBanks[key] = powerBank;
                    }
                    SetAllNestedPropertyValues(powerBank, propertyLists);
                    break;
                case "powerCreep":
                    if (!roomHistory.Creeps.PowerCreeps.TryGetValue(key, out var powerCreep))
                    {
                        powerCreep = new PowerCreep();
                        roomHistory.Creeps.PowerCreeps[key] = powerCreep;
                    }
                    SetAllNestedPropertyValues(powerCreep, propertyLists);
                    break;
                case "powerSpawn":
                    if (!roomHistory.Structures.PowerSpawns.TryGetValue(key, out var powerSpawn))
                    {
                        powerSpawn = new StructurePowerSpawn();
                        roomHistory.Structures.PowerSpawns[key] = powerSpawn;
                    }
                    SetAllNestedPropertyValues(powerSpawn, propertyLists);
                    break;
                case "rampart":
                    if (!roomHistory.Structures.Ramparts.TryGetValue(key, out var rampart))
                    {
                        rampart = new StructureRampart();
                        roomHistory.Structures.Ramparts[key] = rampart;
                    }
                    SetAllNestedPropertyValues(rampart, propertyLists);
                    break;
                case "road":
                    if (!roomHistory.Structures.Roads.TryGetValue(key, out var road))
                    {
                        road = new StructureRoad();
                        roomHistory.Structures.Roads[key] = road;
                    }
                    SetAllNestedPropertyValues(road, propertyLists);
                    break;
                case "ruin":
                    if (!roomHistory.Structures.Ruins.TryGetValue(key, out var ruin))
                    {
                        ruin = new StructureRuin();
                        roomHistory.Structures.Ruins[key] = ruin;
                    }
                    SetAllNestedPropertyValues(ruin, propertyLists);
                    break;
                case "source":
                    if (!roomHistory.Structures.Sources.TryGetValue(key, out var source))
                    {
                        source = new StructureSource();
                        roomHistory.Structures.Sources[key] = source;
                    }
                    SetAllNestedPropertyValues(source, propertyLists);
                    break;
                case "spawn":
                    if (!roomHistory.Structures.Spawns.TryGetValue(key, out var spawn))
                    {
                        spawn = new StructureSpawn();
                        roomHistory.Structures.Spawns[key] = spawn;
                    }
                    SetAllNestedPropertyValues(spawn, propertyLists);
                    break;
                case "storage":
                    if (!roomHistory.Structures.Storages.TryGetValue(key, out var storage))
                    {
                        storage = new StructureStorage();
                        roomHistory.Structures.Storages[key] = storage;
                    }
                    SetAllNestedPropertyValues(storage, propertyLists);
                    break;
                case "terminal":
                    if (!roomHistory.Structures.Terminals.TryGetValue(key, out var terminal))
                    {
                        terminal = new StructureTerminal();
                        roomHistory.Structures.Terminals[key] = terminal;
                    }
                    SetAllNestedPropertyValues(terminal, propertyLists);
                    break;
                case "tombstone":
                    if (!roomHistory.Structures.Tombstones.TryGetValue(key, out var tombstone))
                    {
                        tombstone = new StructureTombstone();
                        roomHistory.Structures.Tombstones[key] = tombstone;
                    }
                    SetAllNestedPropertyValues(tombstone, propertyLists);
                    break;
                case "tower":
                    if (!roomHistory.Structures.Towers.TryGetValue(key, out var tower))
                    {
                        tower = new StructureTower();
                        roomHistory.Structures.Towers[key] = tower;
                    }
                    SetAllNestedPropertyValues(tower, propertyLists);
                    break;
                case "nuke":
                    if (!roomHistory.Structures.Nukes.TryGetValue(key, out var nuke))
                    {
                        nuke = new StructureNuke();
                        roomHistory.Structures.Nukes[key] = nuke;
                    }
                    SetAllNestedPropertyValues(nuke, propertyLists);
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

            return roomHistory;
        }
    }

    public static class ScreepsRoomHistoryComputedHelper
    {
        private static PropertiesList UpdateRecursiveProperties(PropertiesList propertyLists, JObject obj, string basePath = "")
        {
            foreach (var property in obj.Properties())
            {
                var propertyTypeSet = "";
                var propertyKey = property.Name;
                var propertyValue = property.Value;
                var computedKey = $"{(!string.IsNullOrEmpty(basePath) ? $"{basePath}." : "")}{propertyKey}";
                switch (propertyValue.Type)
                {
                    case JTokenType.String:
                        propertyLists.StringProperties[computedKey] = propertyValue.Value<string>() ?? "";
                        propertyTypeSet = "string";
                        break;
                    case JTokenType.Integer:
                    case JTokenType.Float:
                        if (propertyKey != "_id")
                        {
                            propertyLists.IntegerProperties[computedKey] = propertyValue.Value<long>();
                            propertyTypeSet = "integer";
                        }
                        else
                        {
                            propertyLists.StringProperties[computedKey] = propertyValue.Value<string>() ?? "";
                            propertyTypeSet = "string";
                        }
                        break;
                    case JTokenType.Boolean:
                        propertyLists.BooleanProperties[computedKey] = propertyValue.Value<bool>();
                        propertyTypeSet = "boolean";
                        break;
                    case JTokenType.Null:
                        propertyLists.NullProperties.Remove(computedKey);
                        propertyLists.NullProperties.Add(computedKey);
                        propertyTypeSet = "null";
                        break;
                    case JTokenType.Object:
                        propertyLists = UpdateRecursiveProperties(propertyLists, (JObject)propertyValue, computedKey);
                        break;
                    case JTokenType.Array:
                        var childArray = (JArray)propertyValue;
                        for (int i = 0; i < childArray.Count; i++)
                        {
                            var childPropertyTypeSet = "";
                            var computedChildKey = $"{computedKey}.{i}";
                            var childChildItem = childArray[i];
                            if (childChildItem is JObject childChildObj)
                            {
                                propertyLists = UpdateRecursiveProperties(propertyLists, childChildObj, computedChildKey);
                            }
                            else
                            {
                                switch (childChildItem.Type)
                                {
                                    case JTokenType.String:
                                        propertyLists.StringProperties[computedChildKey] = childChildItem.Value<string>() ?? "";
                                        childPropertyTypeSet = "string";
                                        break;
                                    case JTokenType.Integer:
                                    case JTokenType.Float:
                                        if (propertyKey != "_id")
                                        {
                                            propertyLists.IntegerProperties[computedChildKey] = childChildItem.Value<long>();
                                            childPropertyTypeSet = "integer";
                                        }
                                        else
                                        {
                                            propertyLists.StringProperties[computedChildKey] = childChildItem.Value<string>() ?? "";
                                            childPropertyTypeSet = "string";
                                        }
                                        break;
                                    case JTokenType.Boolean:
                                        propertyLists.BooleanProperties[computedChildKey] = childChildItem.Value<bool>();
                                        childPropertyTypeSet = "boolean";
                                        break;
                                    case JTokenType.Null:
                                        propertyLists.NullProperties.Remove(computedChildKey);
                                        propertyLists.NullProperties.Add(computedChildKey);
                                        childPropertyTypeSet = "null";
                                        break;
                                    default:
                                        throw new Exception("Unsupported JTokenType");
                                }
                                propertyLists = RemoveOtherTypePropertyListings(propertyLists, propertyTypeSet, computedKey);
                            }
                        }
                        break;
                    default:
                        throw new Exception("Unsupported JTokenType");
                }
                propertyLists = RemoveOtherTypePropertyListings(propertyLists, propertyTypeSet, computedKey);
            }


            return propertyLists;
        }

        private static PropertiesList RemoveOtherTypePropertyListings(PropertiesList propertiesList, string type, string key)
        {
            if (string.IsNullOrEmpty(type)) return propertiesList;
            if (type != "string") propertiesList.StringProperties.Remove(key);
            if (type != "integer") propertiesList.IntegerProperties.Remove(key);
            if (type != "boolean") propertiesList.BooleanProperties.Remove(key);
            if (type != "null") propertiesList.NullProperties.Remove(key);
            return propertiesList;
        }

        public static ScreepsRoomHistory ComputeTick(JToken tickObject, ScreepsRoomHistory roomHistory)
        {
            foreach (var item in tickObject.Children().Children())
            {
                var key = item.Path.Substring(item.Path.LastIndexOf('.') + 1);
                if (item.Children().Count() > 0 && item is JObject obj)
                {
                    var propertiesList = UpdateRecursiveProperties(roomHistory.PropertiesListDictionary.ContainsKey(key) ? roomHistory.PropertiesListDictionary[key] : new PropertiesList(), obj);
                    roomHistory.PropertiesListDictionary[key] = propertiesList;

                    var type = propertiesList.StringProperties.GetValueOrDefault("type");
                    type ??= roomHistory.TypeMap.GetValueOrDefault(key);

                    if (ConfigSettingsState.WriteHistoryProperties) FileWriterManager.GenerateFiles(roomHistory.Tick.ToString(), type, obj, propertiesList);
                    if (ConfigSettingsState.WriteHistoryTypeProperties && roomHistory.Base == roomHistory.Tick)
                    {
                        FileWriterManager.GenerateFileByType(type, obj);
                    }
                }
                else
                {
                    roomHistory = ConvertJObjectToHistory.RemoveFromRoomHistory(key, roomHistory);
                }
            }

            if (roomHistory.Base == roomHistory.Tick)
            {
                foreach (var propertyList in roomHistory.PropertiesListDictionary.Where(x => x.Value.StringProperties.GetValueOrDefault("type") == "controller"))
                {
                    var key = propertyList.Key;
                    if (string.IsNullOrEmpty(key) || key == "undefined") continue;
                    var propertyLists = propertyList.Value;
                    roomHistory = ConvertJObjectToHistory.UpdateRoomHistory(key, roomHistory, propertyLists);
                }
            }

            foreach (var propertyList in roomHistory.PropertiesListDictionary)
            {
                var key = propertyList.Key;
                if (string.IsNullOrEmpty(key) || key == "undefined") continue;
                var propertyLists = propertyList.Value;
                roomHistory = ConvertJObjectToHistory.UpdateRoomHistory(key, roomHistory, propertyLists);
            }

            return roomHistory;
        }
    }
}
