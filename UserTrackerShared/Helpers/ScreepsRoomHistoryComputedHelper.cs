using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using UserTrackerShared.Models;
using static UserTrackerShared.Models.StructureTombstone;

namespace UserTrackerShared.Helpers
{
    internal class PropertiesList
    {
        public Dictionary<string, string> StringProperties { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, long> IntegerProperties { get; set; } = new Dictionary<string, long>();
        public Dictionary<string, bool> BooleanProperties { get; set; } = new Dictionary<string, bool>();
        public List<string> NullProperties { get; set; } = new List<string>();
    }

    public static class ScreepsRoomHistoryComputedHelper
    {
        private static string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return char.ToUpper(input[0]) + input.Substring(1);
        }
        private static Dictionary<string,string> propertyNameMapping = new Dictionary<string, string>
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
        private static object NavigateToNextLevel(object obj, string property, Dictionary<object, Type> history)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            if (propertyNameMapping.TryGetValue(property, out var mappedName))
            {
                property = mappedName;
            }

            // Attempt to parse the property as a number
            if (int.TryParse(property, out int numericKey))
            {
                // Handle list or dictionary cases
                if (obj is System.Collections.IList list)
                {
                    // Ensure the list is large enough
                    while (numericKey >= list.Count)
                    {
                        list.Add(Activator.CreateInstance(list.GetType().GetGenericArguments()[0]));
                    }
                    return list[numericKey];
                }
                else if (obj.GetType().IsArray)
                {
                    // Handle arrays
                    var array = (Array)obj;
                    if (numericKey >= array.Length)
                    {
                        // Create a new larger array
                        var elementType = array.GetType().GetElementType();
                        var largerArray = Array.CreateInstance(elementType, numericKey + 1);
                        Array.Copy(array, largerArray, array.Length);
                        obj = largerArray;
                    }
                    return ((Array)obj).GetValue(numericKey);
                }
                else if (obj is System.Collections.IDictionary dict)
                {
                    // Add the key with a default value if it doesn't exist
                    if (!dict.Contains(numericKey))
                    {
                        var valueType = dict.GetType().GetGenericArguments()[1];
                        dict[numericKey] = Activator.CreateInstance(valueType);
                    }
                    return dict[numericKey];
                }
                else
                {
                    throw new ArgumentException($"Property '{property}' as a numeric key is not applicable to type {obj.GetType()}.");
                }
            }

            // Get property info
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

            // Check if the property is a dictionary
            if (typeof(System.Collections.IDictionary).IsAssignableFrom(propInfo.PropertyType))
            {
                if (childObj == null)
                {
                    childObj = Activator.CreateInstance(propInfo.PropertyType);
                    propInfo.SetValue(obj, childObj);
                }
                history[obj] = obj.GetType();
                return childObj;
            }

            // Handle other property types
            if (childObj == null)
            {
                if (typeof(System.Collections.IList).IsAssignableFrom(propInfo.PropertyType))
                {
                    childObj = Activator.CreateInstance(propInfo.PropertyType);
                }
                else if (propInfo.PropertyType.IsClass)
                {
                    childObj = Activator.CreateInstance(propInfo.PropertyType);
                }
                propInfo.SetValue(obj, childObj);
            }

            history[obj] = obj.GetType();
            return childObj;
        }

        private static void SetNestedPropertyValue(object obj, string propertyPath, object value)
        {
            if (obj == null || string.IsNullOrEmpty(propertyPath)) throw new ArgumentException("Invalid object or property path.");

            var properties = propertyPath.Split('.');
            var history = new Dictionary<object, Type>();

            for (int i = 0; i < properties.Length - 1; i++)
            {
                string property = properties[i];

                // Navigate to the next level
                obj = NavigateToNextLevel(obj, property, history);

                // Handle dictionary keys
                if (obj is System.Collections.IDictionary dictionary)
                {
                    if (i + 1 >= properties.Length)
                    {
                        throw new InvalidOperationException("Path ended prematurely while expecting a dictionary key.");
                    }

                    string key = properties[++i]; // Move to the next part of the path for the key
                    var keyType = dictionary.GetType().GetGenericArguments()[0];
                    var valueType = dictionary.GetType().GetGenericArguments()[1];

                    // Convert the key to the appropriate type
                    object dictKey = Convert.ChangeType(key, keyType);

                    // Add a default value if the key does not exist
                    if (!dictionary.Contains(dictKey))
                    {
                        dictionary[dictKey] = Activator.CreateInstance(valueType);
                    }

                    // Navigate to the value associated with the dictionary key
                    obj = dictionary[dictKey];
                }
                else if (obj.GetType().IsArray)
                {
                    // Handle array index navigation
                    if (i + 1 >= properties.Length)
                    {
                        throw new InvalidOperationException("Path ended prematurely while expecting an array index.");
                    }

                    if (!int.TryParse(properties[++i], out int index))
                    {
                        throw new ArgumentException($"Invalid array index '{properties[i]}'.");
                    }

                    var array = (Array)obj;

                    // Resize the array if the index is out of bounds
                    if (index >= array.Length)
                    {
                        var elementType = array.GetType().GetElementType();
                        var newArray = Array.CreateInstance(elementType, index + 1);
                        Array.Copy(array, newArray, array.Length);
                        obj = newArray; // Update reference to the resized array
                    }

                    obj = array.GetValue(index);
                }
                else if (obj is System.Collections.IList list)
                {
                    // Handle list index navigation
                    if (i + 1 >= properties.Length)
                    {
                        throw new InvalidOperationException("Path ended prematurely while expecting a list index.");
                    }

                    if (!int.TryParse(properties[++i], out int index))
                    {
                        throw new ArgumentException($"Invalid list index '{properties[i]}'.");
                    }

                    // Ensure the list is large enough
                    while (index >= list.Count)
                    {
                        list.Add(Activator.CreateInstance(list.GetType().GetGenericArguments()[0]));
                    }

                    obj = list[index];
                }
                else if (i + 1 < properties.Length)
                {
                    throw new InvalidOperationException($"Unexpected object type '{obj.GetType()}' at property '{property}'.");
                }
            }

            // Handle the last property
            var lastProperty = properties[^1];
            if (propertyNameMapping.TryGetValue(lastProperty, out var mappedLastProperty))
            {
                lastProperty = mappedLastProperty;
            }

            var lastPropInfo = obj.GetType().GetProperty(lastProperty, BindingFlags.Public | BindingFlags.Instance);
            if (lastPropInfo == null)
            {
                var capitalizedLastProperty = CapitalizeFirstLetter(lastProperty);
                lastPropInfo = obj.GetType().GetProperty(capitalizedLastProperty, BindingFlags.Public | BindingFlags.Instance);
            }
            if (lastPropInfo == null)
                throw new ArgumentException($"Property '{lastProperty}' not found in {obj.GetType()}.");

            if (lastPropInfo.PropertyType.IsClass && lastPropInfo.GetValue(obj) == null &&
                !NonInstantiableTypes.Contains(lastPropInfo.PropertyType))
            {
                var newInstance = Activator.CreateInstance(lastPropInfo.PropertyType);
                lastPropInfo.SetValue(obj, newInstance);
            }
            lastPropInfo.SetValue(obj, value);

            history[obj] = obj.GetType();
        }

        private static void SetAllNestedPropertyValues(object obj, PropertiesList propertyLists)
        {
            // Assuming the propertyLists structure contains keys and values for nested properties
            foreach (var item in propertyLists.NullProperties)
            {
                try
                {
                    SetNestedPropertyValue(obj, item, null); // Set null for properties in NullProperties
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to set null property {item}: {ex.Message}");
                }
            }

            foreach (var kvp in propertyLists.StringProperties)
            {
                try
                {
                    SetNestedPropertyValue(obj, kvp.Key, kvp.Value); // Set string values
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to set string property {kvp.Key}: {ex.Message}");
                }
            }

            foreach (var kvp in propertyLists.IntegerProperties)
            {
                try
                {
                    SetNestedPropertyValue(obj, kvp.Key, kvp.Value); // Set integer values
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to set integer property {kvp.Key}: {ex.Message}");
                }
            }

            foreach (var kvp in propertyLists.BooleanProperties)
            {
                try
                {
                    SetNestedPropertyValue(obj, kvp.Key, kvp.Value); // Set boolean values
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to set boolean property {kvp.Key}: {ex.Message}");
                }
            }
        }

        private static PropertiesList GetRecursiveProperties(PropertiesList propertyLists, JObject obj, string basePath = "")
        {
            foreach (var property in obj.Properties())
            {
                var propertyKey = property.Name;
                var propertyValue = property.Value;
                var computedKey = $"{(!string.IsNullOrEmpty(basePath) ? $"{basePath}." : "")}{propertyKey}";
                switch (propertyValue.Type)
                {
                    case JTokenType.String:
                        propertyLists.StringProperties[computedKey] = propertyValue.Value<string>() ?? "";
                        break;
                    case JTokenType.Integer:
                    case JTokenType.Float:
                        propertyLists.IntegerProperties[computedKey] = propertyValue.Value<long>();
                        break;
                    case JTokenType.Boolean:
                        propertyLists.BooleanProperties[computedKey] = propertyValue.Value<bool>();
                        break;
                    case JTokenType.Null:
                        propertyLists.NullProperties.Add(computedKey);
                        break;
                    case JTokenType.Object:
                        propertyLists = GetRecursiveProperties(propertyLists, (JObject)propertyValue, computedKey);
                        break;
                    case JTokenType.Array:
                        var childArray = (JArray)propertyValue;
                        for (int i = 0; i < childArray.Count; i++)
                        {
                            var computedChildKey = $"{computedKey}.{i}";
                            var childChildItem = childArray[i];
                            if (childChildItem is JObject childChildObj)
                            {
                                propertyLists = GetRecursiveProperties(propertyLists, childChildObj, computedChildKey);
                            }
                            else
                            {
                                switch (childChildItem.Type)
                                {
                                    case JTokenType.String:
                                        propertyLists.StringProperties[computedChildKey] = childChildItem.Value<string>() ?? "";
                                        break;
                                    case JTokenType.Integer:
                                    case JTokenType.Float:
                                        propertyLists.IntegerProperties[computedChildKey] = childChildItem.Value<long>();
                                        break;
                                    case JTokenType.Boolean:
                                        propertyLists.BooleanProperties[computedChildKey] = childChildItem.Value<bool>();
                                        break;
                                    case JTokenType.Null:
                                        propertyLists.NullProperties.Add(computedChildKey);
                                        break;
                                    default:
                                        throw new Exception("Unsupported JTokenType");
                                }
                            }
                        }
                        break;
                    default:
                        throw new Exception("Unsupported JTokenType");
                }
            }
            return propertyLists;
        }

        private static void GenerateFiles(string tick, JObject obj, PropertiesList propertiesList)
        {
            foreach (var prop in propertiesList.NullProperties)
            {
                GenerateFile(tick, prop, obj);
            }
            foreach (var prop in propertiesList.BooleanProperties)
            {
                GenerateFile(tick, prop.Key, obj);
            }
            foreach (var prop in propertiesList.StringProperties)
            {
                GenerateFile(tick, prop.Key, obj);
            }
            foreach (var prop in propertiesList.IntegerProperties)
            {
                GenerateFile(tick, prop.Key, obj);
            }
        }

        private static ConcurrentDictionary<string, SemaphoreSlim> _fileWriters = new ConcurrentDictionary<string, SemaphoreSlim>();
        private static void GenerateFile(string tick, string key, JObject obj)
        {
            string directoryPath = @"C:\Users\Pieter\source\repos\ScreepsUserTracker-V2\UserTrackerConsole\Objects\Keys";
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
            string filePath = $@"{directoryPath}\{key}.json";
            if (File.Exists(filePath)) return;

            if (!obj.ContainsKey("tick")) obj.AddFirst(new JProperty("tick", tick));
            var json = obj.ToString(Formatting.Indented);
           
            var fileWriter = _fileWriters.GetOrAdd(key, key =>
            {
                return new SemaphoreSlim(1, 1);
            });
            fileWriter.Wait();
            try
            {
                File.WriteAllText(filePath, json);
            }
            finally
            {
                fileWriter.Release();
            }
        }

        private static void GenerateFileByType(JObject obj)
        {
            var objTypeToken = obj.GetValue("type");
            if (objTypeToken == null)
            {
                throw new Exception("Object type not found");
            }
            var objType = objTypeToken.Value<string>() ?? throw new Exception("Object type not found");

            var json = "{}";
            string directoryPath = @"C:\Users\Pieter\source\repos\ScreepsUserTracker-V2\UserTrackerConsole\Objects\Types";
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
            string filePath = $@"{directoryPath}\{objType}.json";
            if (!File.Exists(filePath)) File.WriteAllText(filePath, json);

            var fileWriter = _fileWriters.GetOrAdd(objType, key =>
            {
                return new SemaphoreSlim(1, 1);
            });
            fileWriter.Wait();
            json = File.ReadAllText(filePath);

            // Parse the JSON strings into JObject
            JObject obj1 = JObject.Parse(json);

            // Merge obj2 into obj1
            obj1.Merge(obj, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union // Prevents duplicate array values
            });

            // Serialize the merged JObject back to a JSON string
            var updatedJson = obj1.ToString(Formatting.Indented);
            try
            {
                File.WriteAllText(filePath, updatedJson);
            }
            finally
            {
                fileWriter.Release();
            }
        }

        private static ScreepsRoomHistory ComputeObject(string key, ScreepsRoomHistory roomHistory, PropertiesList propertyLists)
        {
            var id = propertyLists.StringProperties.GetValueOrDefault("_id");
            var type = propertyLists.StringProperties.GetValueOrDefault("type");
            if (type == null) type = roomHistory.TypeMap.GetValueOrDefault(key);
            else roomHistory.TypeMap.TryAdd(key, type);
            var user = propertyLists.StringProperties.GetValueOrDefault("user");
            if (user == null) user = roomHistory.UserMap.GetValueOrDefault(key);
            else roomHistory.UserMap.TryAdd(key, user);

            switch (type)
            {
                case "constructedWall":
                    var wall = roomHistory.Structures.Walls.Find(w => w.Id == id) ?? new StructureWall();
                    SetAllNestedPropertyValues(wall, propertyLists);
                    roomHistory.Structures.Walls.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Walls.Add(wall);
                    break;
                case "constructionSite":
                    var constructionSite = roomHistory.Structures.ConstructionSites.Find(w => w.Id == id) ?? new StructureConstructionSite();
                    SetAllNestedPropertyValues(constructionSite, propertyLists);
                    roomHistory.Structures.ConstructionSites.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.ConstructionSites.Add(constructionSite);
                    break;
                case "container":
                    var container = roomHistory.Structures.Containers.Find(w => w.Id == id) ?? new StructureContainer();
                    SetAllNestedPropertyValues(container, propertyLists);
                    roomHistory.Structures.Containers.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Containers.Add(container);
                    break;
                case "controller":
                    var controller = roomHistory.Structures.Controller ?? new StructureController();
                    SetAllNestedPropertyValues(controller, propertyLists);
                    roomHistory.Structures.Controller = controller;
                    break;
                case "creep":
                    var hasController = roomHistory.Structures.Controller != null;
                    var isOwnCreep = false;
                    if (hasController)
                    {
                        isOwnCreep = roomHistory.Structures.Controller.User == user;
                    }
                    Creep creep = null;
                    if (hasController && isOwnCreep)
                    {
                        creep = roomHistory.Creeps.OwnedCreeps.Find(w => w.Id == id) ?? new Creep();
                    }
                    else if (hasController && !isOwnCreep)
                    {
                        creep = roomHistory.Creeps.EnemyCreeps.Find(w => w.Id == id) ?? new Creep();
                    }
                    else
                    {
                        creep = roomHistory.Creeps.OtherCreeps.Find(w => w.Id == id) ?? new Creep();
                    }

                    SetAllNestedPropertyValues(creep, propertyLists);
                    if (hasController && isOwnCreep)
                    {
                        roomHistory.Creeps.OwnedCreeps.RemoveAll(w => w.Id == id);
                        roomHistory.Creeps.OwnedCreeps.Add(creep);
                    }
                    else if (hasController && !isOwnCreep)
                    {
                        roomHistory.Creeps.EnemyCreeps.RemoveAll(w => w.Id == id);
                        roomHistory.Creeps.EnemyCreeps.Add(creep);
                    }
                    else
                    {
                        roomHistory.Creeps.OtherCreeps.RemoveAll(w => w.Id == id);
                        roomHistory.Creeps.OtherCreeps.Add(creep);
                    }
                    break;
                case "deposit":
                    var deposit = roomHistory.Structures.Deposit ?? new StructureDepsoit();
                    SetAllNestedPropertyValues(deposit, propertyLists);
                    roomHistory.Structures.Deposit = deposit;
                    break;
                case "energy":
                    var resource = roomHistory.GroundResources.Find(w => w.Id == id) ?? new GroundResource();
                    SetAllNestedPropertyValues(resource, propertyLists);
                    roomHistory.GroundResources.RemoveAll(w => w.Id == id);
                    roomHistory.GroundResources.Add(resource);
                    break;
                case "extension":
                    var extension = roomHistory.Structures.Extensions.Find(w => w.Id == id) ?? new StructureExtension();
                    SetAllNestedPropertyValues(extension, propertyLists);
                    roomHistory.Structures.Extensions.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Extensions.Add(extension);
                    break;
                case "extractor":
                    var extractor = roomHistory.Structures.Extractors.Find(w => w.Id == id) ?? new StructureExtractor();
                    SetAllNestedPropertyValues(extractor, propertyLists);
                    roomHistory.Structures.Extractors.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Extractors.Add(extractor);
                    break;
                case "factory":
                    var factory = roomHistory.Structures.Factories.Find(w => w.Id == id) ?? new StructureFactory();
                    SetAllNestedPropertyValues(factory, propertyLists);
                    roomHistory.Structures.Factories.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Factories.Add(factory);
                    break;
                case "invaderCore":
                    var invaderCore = roomHistory.Structures.InvaderCores.Find(w => w.Id == id) ?? new StructureInvaderCore();
                    SetAllNestedPropertyValues(invaderCore, propertyLists);
                    roomHistory.Structures.InvaderCores.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.InvaderCores.Add(invaderCore);
                    break;
                case "keeperLair":
                    var keeperLair = roomHistory.Structures.KeeperLairs.Find(w => w.Id == id) ?? new StructureKeeperLair();
                    SetAllNestedPropertyValues(keeperLair, propertyLists);
                    roomHistory.Structures.KeeperLairs.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.KeeperLairs.Add(keeperLair);
                    break;
                case "lab":
                    var lab = roomHistory.Structures.Labs.Find(w => w.Id == id) ?? new StructureLab();
                    SetAllNestedPropertyValues(lab, propertyLists);
                    roomHistory.Structures.Labs.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Labs.Add(lab);
                    break;
                case "link":
                    var link = roomHistory.Structures.Links.Find(w => w.Id == id) ?? new StructureLink();
                    SetAllNestedPropertyValues(link, propertyLists);
                    roomHistory.Structures.Links.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Links.Add(link);
                    break;
                case "mineral":
                    var mineral = roomHistory.Structures.Mineral ?? new StructureMineral();
                    SetAllNestedPropertyValues(mineral, propertyLists);
                    roomHistory.Structures.Mineral = mineral;
                    break;
                case "nuker":
                    var nuker = roomHistory.Structures.Nukers.Find(w => w.Id == id) ?? new StructureNuker();
                    SetAllNestedPropertyValues(nuker, propertyLists);
                    roomHistory.Structures.Nukers.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Nukers.Add(nuker);
                    break;
                case "observer":
                    var observer = roomHistory.Structures.Observers.Find(w => w.Id == id) ?? new StructureObserver();
                    SetAllNestedPropertyValues(observer, propertyLists);
                    roomHistory.Structures.Observers.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Observers.Add(observer);
                    break;
                case "portal":
                    var portal = roomHistory.Structures.Portals.Find(w => w.Id == id) ?? new StructurePortal();
                    SetAllNestedPropertyValues(portal, propertyLists);
                    roomHistory.Structures.Portals.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Portals.Add(portal);
                    break;
                case "powerBank":
                    var powerBank = roomHistory.Structures.PowerBanks.Find(w => w.Id == id) ?? new StructurePowerBank();
                    SetAllNestedPropertyValues(powerBank, propertyLists);
                    roomHistory.Structures.PowerBanks.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.PowerBanks.Add(powerBank);
                    break;
                case "powerCreep":
                    var powerCreep = roomHistory.Creeps.PowerCreeps.Find(w => w.Id == id) ?? new PowerCreep();
                    SetAllNestedPropertyValues(powerCreep, propertyLists);
                    roomHistory.Creeps.PowerCreeps.RemoveAll(w => w.Id == id);
                    roomHistory.Creeps.PowerCreeps.Add(powerCreep);
                    break;
                case "powerSpawn":
                    var powerSpawn = roomHistory.Structures.PowerSpawns.Find(w => w.Id == id) ?? new StructurePowerSpawn();
                    SetAllNestedPropertyValues(powerSpawn, propertyLists);
                    roomHistory.Structures.PowerSpawns.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.PowerSpawns.Add(powerSpawn);
                    break;
                case "rampart":
                    var rampart = roomHistory.Structures.Ramparts.Find(w => w.Id == id) ?? new StructureRampart();
                    SetAllNestedPropertyValues(rampart, propertyLists);
                    roomHistory.Structures.Ramparts.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Ramparts.Add(rampart);
                    break;
                case "road":
                    var road = roomHistory.Structures.Roads.Find(w => w.Id == id) ?? new StructureRoad();
                    SetAllNestedPropertyValues(road, propertyLists);
                    roomHistory.Structures.Roads.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Roads.Add(road);
                    break;
                case "ruin":
                    var ruin = roomHistory.Structures.Ruins.Find(w => w.Id == id) ?? new StructureRuin();
                    SetAllNestedPropertyValues(ruin, propertyLists);
                    roomHistory.Structures.Ruins.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Ruins.Add(ruin);
                    break;
                case "source":
                    var source = roomHistory.Structures.Sources.Find(w => w.Id == id) ?? new StructureSource();
                    SetAllNestedPropertyValues(source, propertyLists);
                    roomHistory.Structures.Sources.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Sources.Add(source);
                    break;
                case "spawn":
                    var spawn = roomHistory.Structures.Spawns.Find(w => w.Id == id) ?? new StructureSpawn();
                    SetAllNestedPropertyValues(spawn, propertyLists);
                    roomHistory.Structures.Spawns.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Spawns.Add(spawn);
                    break;
                case "storage":
                    var storage = roomHistory.Structures.Storages.Find(w => w.Id == id) ?? new StructureStorage();
                    SetAllNestedPropertyValues(storage, propertyLists);
                    roomHistory.Structures.Storages.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Storages.Add(storage);
                    break;
                case "terminal":
                    var terminal = roomHistory.Structures.Terminals.Find(w => w.Id == id) ?? new StructureTerminal();
                    SetAllNestedPropertyValues(terminal, propertyLists);
                    roomHistory.Structures.Terminals.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Terminals.Add(terminal);
                    break;
                case "tombstone":
                    var tombstone = roomHistory.Structures.Tombstones.Find(w => w.Id == id) ?? new StructureTombstone();
                    SetAllNestedPropertyValues(tombstone, propertyLists);
                    roomHistory.Structures.Tombstones.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Tombstones.Add(tombstone);
                    break;
                case "tower":
                    var tower = roomHistory.Structures.Towers.Find(w => w.Id == id) ?? new StructureTower();
                    SetAllNestedPropertyValues(tower, propertyLists);
                    roomHistory.Structures.Towers.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Towers.Add(tower);
                    break;
                case "nuke":
                    var nuke = roomHistory.Structures.Nukes.Find(w => w.Id == id) ?? new StructureNuke();
                    SetAllNestedPropertyValues(nuke, propertyLists);
                    roomHistory.Structures.Nukes.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Nukes.Add(nuke);
                    break;
                default:
                    Debug.WriteLine(type);
                    throw new Exception("Unknown type");
            }

            return roomHistory;
        }

        public static ScreepsRoomHistory Compute(JObject roomData)
        {
            var roomHistory = new ScreepsRoomHistory();
            roomData.TryGetValue("timestamp", out JToken? jTokenTime);
            if (jTokenTime != null) roomHistory.TimeStamp = jTokenTime.Value<long>();

            roomData.TryGetValue("ticks", out JToken? jTokenTicks);
            if (jTokenTicks != null)
            {
                // for
                var jTokenTicksValues = jTokenTicks.Values<JToken>();
                for (int i = 0; i < jTokenTicksValues.Count(); i++)
                {
                    var tickObject = jTokenTicksValues.ElementAt(i);
                    if (tickObject == null) continue;
                    var tickNumber = tickObject.Path.Substring(tickObject.Path.LastIndexOf('.') + 1);


                    var propertiesListDictionary = new Dictionary<string, PropertiesList>();
                    foreach (var item in tickObject.Children().Children())
                    {
                        if (item.Children().First() is JObject obj)
                        {
                            var key = obj.Path.Substring(obj.Path.LastIndexOf('.') + 1);
                            var propertiesList = GetRecursiveProperties(new PropertiesList(), obj);
                            propertiesListDictionary[key] = propertiesList;

                            //if (i == 0)
                            //{
                            //    GenerateFiles(tickNumber, obj, propertiesList);
                            //    GenerateFileByType(obj);
                            //}
                        }
                    }

                    foreach (var propertyList in propertiesListDictionary.Where(x => x.Value.StringProperties.GetValueOrDefault("type") == "controller"))
                    {
                        var key = propertyList.Key;
                        var propertyLists = propertyList.Value;
                        roomHistory = ComputeObject(key, roomHistory, propertyLists);
                    }

                    foreach (var propertyList in propertiesListDictionary)
                    {
                        var key = propertyList.Key;
                        var propertyLists = propertyList.Value;
                        roomHistory = ComputeObject(key, roomHistory, propertyLists);
                    }
                }
            }

            return roomHistory;
        }
    }
}
