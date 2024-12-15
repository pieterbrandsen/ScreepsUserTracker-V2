using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private static PropertiesList GetRecursiveProperties(PropertiesList propertyLists, JObject obj, string basePath = "")
        {
            foreach (var property in obj.Properties())
            {
                var propertyKey = property.Name;
                var propertyValue = property.Value;
                var computedKey = $"{(!string.IsNullOrEmpty(basePath) ? $"{basePath}." : "")}{propertyKey}";
                switch (propertyValue.Type.ToString())
                {
                    case "String":
                        propertyLists.StringProperties.Add(computedKey, propertyValue.Value<string>() ?? "");
                        break;
                    case "Integer":
                    case "Float":
                        propertyLists.IntegerProperties.Add(computedKey, propertyValue.Value<long>());
                        break;
                    case "Boolean":
                        propertyLists.BooleanProperties.Add(computedKey, propertyValue.Value<bool>());
                        break;
                    case "Null":
                        propertyLists.NullProperties.Add(computedKey);
                        break;
                    default:
                        if (propertyValue is JObject childObj)
                        {
                            propertyLists = GetRecursiveProperties(propertyLists, childObj, computedKey);
                        }
                        else if (propertyValue is JArray childArray)
                        {
                            for (int i = 0; i < childArray.Count; i++)
                            {
                                var computedChildKey = $"{(!string.IsNullOrEmpty(basePath) ? $"{basePath}." : "")}array{i}.{propertyKey}";
                                var childChildItem = childArray[i];
                                if (childChildItem is JObject childChildObj)
                                {
                                    propertyLists = GetRecursiveProperties(propertyLists, childChildObj, computedChildKey);
                                }
                                else
                                {
                                    switch (childChildItem.Type.ToString())
                                    {
                                        case "String":
                                            propertyLists.StringProperties.Add(computedChildKey, childChildItem.Value<string>() ?? "");
                                            break;
                                        case "Integer":
                                        case "Float":
                                            propertyLists.IntegerProperties.Add(computedChildKey, childChildItem.Value<long>());
                                            break;
                                        case "Boolean":
                                            propertyLists.BooleanProperties.Add(computedChildKey, childChildItem.Value<bool>());
                                            break;
                                        case "Null":
                                            propertyLists.NullProperties.Add(computedChildKey);
                                            break;
                                        default:
                                            throw new Exception("");
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("");
                        }
                        break;
                }
            }
            return propertyLists;
        }

        private static void GenerateFiles(string tick, JObject obj, PropertiesList propertiesList)
        {
            foreach (var prop in propertiesList.NullProperties)
            {
                GenerateFile(tick,prop, obj);
            }
            foreach (var prop in propertiesList.BooleanProperties)
            {
                GenerateFile(tick,prop.Key, obj);
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
        private static void GenerateFile(string tick, string key, JObject obj)
        {
            string directoryPath = @"C:\Users\Pieter\source\repos\ScreepsUserTracker-V2\UserTrackerConsole\Objects\Keys";
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
            string filePath = $@"{directoryPath}\{key}.json";
            if (File.Exists(filePath)) return;

            if (!obj.ContainsKey("tick")) obj.AddFirst(new JProperty("tick", tick));
            var json = obj.ToString(Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
        private static void GenerateFileByType(JObject obj)
        {
            var objTypeToken = obj.GetValue("type");
            if (objTypeToken == null)
            {
                throw new Exception();
            }
            var objType = objTypeToken.Value<string>() ?? throw new Exception("Object type not found");

            var json = "{}";
            string directoryPath = @"C:\Users\Pieter\source\repos\ScreepsUserTracker-V2\UserTrackerConsole\Objects\Types";
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
            string filePath = $@"{directoryPath}\{objType}.json";
            if (!File.Exists(filePath)) File.WriteAllText(filePath, json);

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
            File.WriteAllText(filePath, updatedJson);
        }

        private static ScreepsRoomHistory ComputeObject(string key, ScreepsRoomHistory roomHistory, PropertiesList propertyLists)
        {
            var id = propertyLists.StringProperties.GetValueOrDefault("_id");
            var type = propertyLists.StringProperties.GetValueOrDefault("type");
            if (type == null) type = roomHistory.TypeMap.GetValueOrDefault(key);
            else roomHistory.TypeMap.TryAdd(key, type);

            switch (type)
            {
                case "constructedWall":
                    StructureWall wall = roomHistory.Structures.Walls.Find(w => w.Id == id) ?? new StructureWall();
                    roomHistory.Structures.Walls.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Walls.Add(wall);
                    break;
                case "constructionSite":
                    StructureConstructionSite constructionSite = roomHistory.Structures.ConstructionSites.Find(w => w.Id == id) ?? new StructureConstructionSite();
                    roomHistory.Structures.ConstructionSites.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.ConstructionSites.Add(constructionSite);
                    break;
                case "container":
                    StructureContainer container = roomHistory.Structures.Containers.Find(w => w.Id == id) ?? new StructureContainer();
                    roomHistory.Structures.Containers.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Containers.Add(container);
                    break;
                case "controller":
                    StructureController controller = roomHistory.Structures.Controller ?? new StructureController();
                    roomHistory.Structures.Controller = controller;
                    break;
                case "creep":
                    var hasController = roomHistory.Structures.Controller != null;
                    var isOwnCreep = false;
                    if (hasController)
                    {
                        //isOwnCreep = roomHistory.Structures.Controller.
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
                    StructureDepsoit deposit = roomHistory.Structures.Deposit ?? new StructureDepsoit();
                    roomHistory.Structures.Deposit = deposit;
                    break;
                case "energy":
                    GroundResource resource = roomHistory.GroundResources.Find(w => w.Id == id) ?? new GroundResource();
                    roomHistory.GroundResources.RemoveAll(w => w.Id == id);
                    roomHistory.GroundResources.Add(resource);
                    break;
                case "extension":
                    StructureExtension extension = roomHistory.Structures.Extensions.Find(w => w.Id == id) ?? new StructureExtension();
                    roomHistory.Structures.Extensions.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Extensions.Add(extension);
                    break;
                case "extractor":
                    StructureExtractor extractor = roomHistory.Structures.Extractors.Find(w => w.Id == id) ?? new StructureExtractor();
                    roomHistory.Structures.Extractors.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Extractors.Add(extractor);
                    break;
                case "factory":
                    StructureFactory factory = roomHistory.Structures.Factories.Find(w => w.Id == id) ?? new StructureFactory();
                    roomHistory.Structures.Factories.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Factories.Add(factory);
                    break;
                case "invaderCore":
                    StructureInvaderCore invaderCore = roomHistory.Structures.InvaderCores.Find(w => w.Id == id) ?? new StructureInvaderCore();
                    roomHistory.Structures.InvaderCores.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.InvaderCores.Add(invaderCore);
                    break;
                case "keeperLair":
                    StructureKeeperLair keeperLair = roomHistory.Structures.KeeperLairs.Find(w => w.Id == id) ?? new StructureKeeperLair();
                    roomHistory.Structures.KeeperLairs.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.KeeperLairs.Add(keeperLair);
                    break;
                case "lab":
                    StructureLab lab = roomHistory.Structures.Labs.Find(w => w.Id == id) ?? new StructureLab();
                    roomHistory.Structures.Labs.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Labs.Add(lab);
                    break;
                case "link":
                    StructureLink link = roomHistory.Structures.Links.Find(w => w.Id == id) ?? new StructureLink();
                    roomHistory.Structures.Links.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Links.Add(link);
                    break;
                case "mineral":
                    StructureMineral mineral = roomHistory.Structures.Mineral ?? new StructureMineral();
                    roomHistory.Structures.Mineral = mineral;
                    break;
                case "nuker":
                    StructureNuker nuker = roomHistory.Structures.Nukers.Find(w => w.Id == id) ?? new StructureNuker();
                    roomHistory.Structures.Nukers.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Nukers.Add(nuker);
                    break;
                case "observer":
                    StructureObserver observer = roomHistory.Structures.Observers.Find(w => w.Id == id) ?? new StructureObserver();
                    roomHistory.Structures.Observers.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Observers.Add(observer);
                    break;
                case "portal":
                    StructurePortal portal = roomHistory.Structures.Portals.Find(w => w.Id == id) ?? new StructurePortal();
                    roomHistory.Structures.Portals.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Portals.Add(portal);
                    break;
                case "powerBank":
                    StructurePowerBank powerBank = roomHistory.Structures.PowerBanks.Find(w => w.Id == id) ?? new StructurePowerBank();
                    roomHistory.Structures.PowerBanks.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.PowerBanks.Add(powerBank);
                    break;
                case "powerCreep":
                    PowerCreep powerCreep = roomHistory.Creeps.PowerCreeps.Find(w => w.Id == id) ?? new PowerCreep();
                    roomHistory.Creeps.PowerCreeps.RemoveAll(w => w.Id == id);
                    roomHistory.Creeps.PowerCreeps.Add(powerCreep);
                    break;
                case "powerSpawn":
                    StructurePowerSpawn powerSpawn = roomHistory.Structures.PowerSpawns.Find(w => w.Id == id) ?? new StructurePowerSpawn();
                    roomHistory.Structures.PowerSpawns.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.PowerSpawns.Add(powerSpawn);
                    break;
                case "rampart":
                    StructureRampart rampart = roomHistory.Structures.Ramparts.Find(w => w.Id == id) ?? new StructureRampart();
                    roomHistory.Structures.Ramparts.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Ramparts.Add(rampart);
                    break;
                case "road":
                    StructureRoad road = roomHistory.Structures.Roads.Find(w => w.Id == id) ?? new StructureRoad();
                    roomHistory.Structures.Roads.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Roads.Add(road);
                    break;
                case "ruin":
                    StructureRuin ruin = roomHistory.Structures.Ruins.Find(w => w.Id == id) ?? new StructureRuin();
                    roomHistory.Structures.Ruins.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Ruins.Add(ruin);
                    break;
                case "source":
                    StructureSource source = roomHistory.Structures.Sources.Find(w => w.Id == id) ?? new StructureSource();
                    roomHistory.Structures.Sources.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Sources.Add(source);
                    break;
                case "spawn":
                    StructureSpawn spawn = roomHistory.Structures.Spawns.Find(w => w.Id == id) ?? new StructureSpawn();
                    roomHistory.Structures.Spawns.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Spawns.Add(spawn);
                    break;
                case "storage":
                    StructureStorage storage = roomHistory.Structures.Storages.Find(w => w.Id == id) ?? new StructureStorage();
                    roomHistory.Structures.Storages.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Storages.Add(storage);
                    break;
                case "terminal":
                    StructureTerminal terminal = roomHistory.Structures.Terminals.Find(w => w.Id == id) ?? new StructureTerminal();
                    roomHistory.Structures.Terminals.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Terminals.Add(terminal);
                    break;
                case "tombstone":
                    StructureTombstone tombstone = roomHistory.Structures.Tombstones.Find(w => w.Id == id) ?? new StructureTombstone();
                    roomHistory.Structures.Tombstones.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Tombstones.Add(tombstone);
                    break;
                case "tower":
                    StructureTower tower = roomHistory.Structures.Towers.Find(w => w.Id == id) ?? new StructureTower();
                    roomHistory.Structures.Towers.RemoveAll(w => w.Id == id);
                    roomHistory.Structures.Towers.Add(tower);
                    break;
                default:
                    throw new Exception("");
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
                var ticks = jTokenTicks.Values<JToken>().ToList();
                for (int i = 0; i < ticks.Count; i++)
                {
                    var tickObject = ticks[i];
                    if (tickObject == null) continue;
                    var tickNumber = tickObject.Path.Substring(tickObject.Path.LastIndexOf('.') + 1);

                    var propertiesListDictionary = new Dictionary<string, PropertiesList>();
                    var objects = tickObject.Children().Children().ToList();
                    for (int y = 0; y < objects.Count; y++)
                    {
                        JToken? item = objects[y];
                        if (item.Children().First() is JObject obj)
                        {
                            var key = obj.Path.Substring(obj.Path.LastIndexOf('.') + 1);
                            var propertiesList = GetRecursiveProperties(new PropertiesList(), obj);
                            propertiesListDictionary.Add(key, propertiesList);
                            if (i == 0) GenerateFileByType(obj);
                            GenerateFiles(tickNumber, obj, propertiesList);
                        }
                    }

                    foreach (var propertyList in propertiesListDictionary.Where(x=>x.Value.StringProperties.GetValueOrDefault("type") == "controller"))
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
