using Newtonsoft.Json.Linq;
using System.Text;
using UserTrackerShared.Helpers.Tests;
using UserTrackerShared.Models;
using UserTrackerShared.Utilities;

namespace UserTrackerShared.Helpers
{

    public static class ScreepsRoomHistoryHelper
    {
        public static ScreepsRoomHistory UpdateRoomHistory(string key, ScreepsRoomHistory roomHistory, Dictionary<string, object?> changes)
        {
            var type = roomHistory.TypeMap.GetValueOrDefault(key);
            var user = changes.GetValueOrDefault("user") as string;
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
                    DynamicPatcher.ApplyPatch(wall, changes);
                    break;
                case "constructionSite":
                    if (!roomHistory.Structures.ConstructionSites.TryGetValue(key, out var constructionSite))
                    {
                        constructionSite = new StructureConstructionSite();
                        roomHistory.Structures.ConstructionSites[key] = constructionSite;
                    }
                    DynamicPatcher.ApplyPatch(constructionSite, changes);
                    break;
                case "container":
                    if (!roomHistory.Structures.Containers.TryGetValue(key, out var container))
                    {
                        container = new StructureContainer();
                        roomHistory.Structures.Containers[key] = container;
                    }
                    DynamicPatcher.ApplyPatch(container, changes);
                    break;
                case "controller":
                    var controller = roomHistory.Structures.Controller ?? new StructureController();
                    DynamicPatcher.ApplyPatch(controller, changes);
                    roomHistory.Structures.Controller = controller;
                    break;
                case "creep":
                    var hasController = roomHistory.Structures.Controller != null;
                    var isOwnCreep = hasController && (roomHistory.Structures.Controller?.User == user || roomHistory.Structures.Controller?.Reservation?.User == user);
                    Creep? creep = null;
                    roomHistory.Creeps.OwnedCreeps.TryGetValue(key, out Creep? owCreep);
                    roomHistory.Creeps.EnemyCreeps.TryGetValue(key, out Creep? eCreep);
                    roomHistory.Creeps.OtherCreeps.TryGetValue(key, out Creep? otCreep);
                    if (owCreep != null) creep = owCreep;
                    else if (eCreep != null) creep = eCreep;
                    else if (otCreep != null) creep = otCreep;

                    if (creep == null)
                    {
                        if (hasController)
                        {
                            if (isOwnCreep)
                            {
                                if (!roomHistory.Creeps.OwnedCreeps.TryGetValue(key, out creep))
                                {
                                    creep = new Creep();
                                    roomHistory.Creeps.OwnedCreeps[key] = creep;
                                }
                            }
                            else
                            {
                                if (!roomHistory.Creeps.EnemyCreeps.TryGetValue(key, out creep))
                                {
                                    creep = new Creep();
                                    roomHistory.Creeps.EnemyCreeps[key] = creep;
                                }
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
                    }
                    DynamicPatcher.ApplyPatch(creep, changes);
                    break;
                case "deposit":
                    if (!roomHistory.Structures.Deposits.TryGetValue(key, out var deposit))
                    {
                        deposit = new StructureDeposit();
                        roomHistory.Structures.Deposits[key] = deposit;
                    }
                    DynamicPatcher.ApplyPatch(deposit, changes);
                    break;
                case "energy":
                    if (!roomHistory.GroundResources.TryGetValue(key, out var resource))
                    {
                        resource = new GroundResource();
                        roomHistory.GroundResources[key] = resource;
                    }
                    DynamicPatcher.ApplyPatch(resource, changes);
                    break;
                case "extension":
                    if (!roomHistory.Structures.Extensions.TryGetValue(key, out var extension))
                    {
                        extension = new StructureExtension();
                        roomHistory.Structures.Extensions[key] = extension;
                    }
                    DynamicPatcher.ApplyPatch(extension, changes);
                    break;
                case "extractor":
                    if (!roomHistory.Structures.Extractors.TryGetValue(key, out var extractor))
                    {
                        extractor = new StructureExtractor();
                        roomHistory.Structures.Extractors[key] = extractor;
                    }
                    DynamicPatcher.ApplyPatch(extractor, changes);
                    break;
                case "factory":
                    if (!roomHistory.Structures.Factories.TryGetValue(key, out var factory))
                    {
                        factory = new StructureFactory();
                        roomHistory.Structures.Factories[key] = factory;
                    }
                    DynamicPatcher.ApplyPatch(factory, changes);
                    break;
                case "invaderCore":
                    if (!roomHistory.Structures.InvaderCores.TryGetValue(key, out var invaderCore))
                    {
                        invaderCore = new StructureInvaderCore();
                        roomHistory.Structures.InvaderCores[key] = invaderCore;
                    }
                    DynamicPatcher.ApplyPatch(invaderCore, changes);
                    break;
                case "keeperLair":
                    if (!roomHistory.Structures.KeeperLairs.TryGetValue(key, out var keeperLair))
                    {
                        keeperLair = new StructureKeeperLair();
                        roomHistory.Structures.KeeperLairs[key] = keeperLair;
                    }
                    DynamicPatcher.ApplyPatch(keeperLair, changes);
                    break;
                case "lab":
                    if (!roomHistory.Structures.Labs.TryGetValue(key, out var lab))
                    {
                        lab = new StructureLab();
                        roomHistory.Structures.Labs[key] = lab;
                    }
                    DynamicPatcher.ApplyPatch(lab, changes);
                    break;
                case "link":
                    if (!roomHistory.Structures.Links.TryGetValue(key, out var link))
                    {
                        link = new StructureLink();
                        roomHistory.Structures.Links[key] = link;
                    }
                    DynamicPatcher.ApplyPatch(link, changes);
                    break;
                case "mineral":
                    var mineral = roomHistory.Structures.Mineral ?? new StructureMineral();
                    DynamicPatcher.ApplyPatch(mineral, changes);
                    roomHistory.Structures.Mineral = mineral;
                    break;
                case "nuker":
                    if (!roomHistory.Structures.Nukers.TryGetValue(key, out var nuker))
                    {
                        nuker = new StructureNuker();
                        roomHistory.Structures.Nukers[key] = nuker;
                    }
                    DynamicPatcher.ApplyPatch(nuker, changes);
                    break;
                case "observer":
                    if (!roomHistory.Structures.Observers.TryGetValue(key, out var observer))
                    {
                        observer = new StructureObserver();
                        roomHistory.Structures.Observers[key] = observer;
                    }
                    DynamicPatcher.ApplyPatch(observer, changes);
                    break;
                case "portal":
                    if (!roomHistory.Structures.Portals.TryGetValue(key, out var portal))
                    {
                        portal = new StructurePortal();
                        roomHistory.Structures.Portals[key] = portal;
                    }
                    DynamicPatcher.ApplyPatch(portal, changes);
                    break;
                case "powerBank":
                    if (!roomHistory.Structures.PowerBanks.TryGetValue(key, out var powerBank))
                    {
                        powerBank = new StructurePowerBank();
                        roomHistory.Structures.PowerBanks[key] = powerBank;
                    }
                    DynamicPatcher.ApplyPatch(powerBank, changes);
                    break;
                case "powerCreep":
                    if (!roomHistory.Creeps.PowerCreeps.TryGetValue(key, out var powerCreep))
                    {
                        powerCreep = new PowerCreep();
                        roomHistory.Creeps.PowerCreeps[key] = powerCreep;
                    }
                    DynamicPatcher.ApplyPatch(powerCreep, changes);
                    break;
                case "powerSpawn":
                    if (!roomHistory.Structures.PowerSpawns.TryGetValue(key, out var powerSpawn))
                    {
                        powerSpawn = new StructurePowerSpawn();
                        roomHistory.Structures.PowerSpawns[key] = powerSpawn;
                    }
                    DynamicPatcher.ApplyPatch(powerSpawn, changes);
                    break;
                case "rampart":
                    if (!roomHistory.Structures.Ramparts.TryGetValue(key, out var rampart))
                    {
                        rampart = new StructureRampart();
                        roomHistory.Structures.Ramparts[key] = rampart;
                    }
                    DynamicPatcher.ApplyPatch(rampart, changes);
                    break;
                case "road":
                    if (!roomHistory.Structures.Roads.TryGetValue(key, out var road))
                    {
                        road = new StructureRoad();
                        roomHistory.Structures.Roads[key] = road;
                    }
                    DynamicPatcher.ApplyPatch(road, changes);
                    break;
                case "ruin":
                    if (!roomHistory.Structures.Ruins.TryGetValue(key, out var ruin))
                    {
                        ruin = new StructureRuin();
                        roomHistory.Structures.Ruins[key] = ruin;
                    }
                    DynamicPatcher.ApplyPatch(ruin, changes);
                    break;
                case "source":
                    if (!roomHistory.Structures.Sources.TryGetValue(key, out var source))
                    {
                        source = new StructureSource();
                        roomHistory.Structures.Sources[key] = source;
                    }
                    DynamicPatcher.ApplyPatch(source, changes);
                    break;
                case "spawn":
                    if (!roomHistory.Structures.Spawns.TryGetValue(key, out var spawn))
                    {
                        spawn = new StructureSpawn();
                        roomHistory.Structures.Spawns[key] = spawn;
                    }
                    DynamicPatcher.ApplyPatch(spawn, changes);
                    break;
                case "storage":
                    if (!roomHistory.Structures.Storages.TryGetValue(key, out var storage))
                    {
                        storage = new StructureStorage();
                        roomHistory.Structures.Storages[key] = storage;
                    }
                    DynamicPatcher.ApplyPatch(storage, changes);
                    break;
                case "terminal":
                    if (!roomHistory.Structures.Terminals.TryGetValue(key, out var terminal))
                    {
                        terminal = new StructureTerminal();
                        roomHistory.Structures.Terminals[key] = terminal;
                    }
                    DynamicPatcher.ApplyPatch(terminal, changes);
                    break;
                case "tombstone":
                    if (!roomHistory.Structures.Tombstones.TryGetValue(key, out var tombstone))
                    {
                        tombstone = new StructureTombstone();
                        roomHistory.Structures.Tombstones[key] = tombstone;
                    }
                    DynamicPatcher.ApplyPatch(tombstone, changes);
                    break;
                case "tower":
                    if (!roomHistory.Structures.Towers.TryGetValue(key, out var tower))
                    {
                        tower = new StructureTower();
                        roomHistory.Structures.Towers[key] = tower;
                    }
                    DynamicPatcher.ApplyPatch(tower, changes);
                    break;
                case "nuke":
                    if (!roomHistory.Structures.Nukes.TryGetValue(key, out var nuke))
                    {
                        nuke = new StructureNuke();
                        roomHistory.Structures.Nukes[key] = nuke;
                    }
                    DynamicPatcher.ApplyPatch(nuke, changes);
                    break;
                default:
                    break;
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
                    roomHistory.Structures.Deposits.Remove(key);
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

        public static ReadOnlySpan<char> GetLastPathSegment(ReadOnlySpan<char> path)
        {
            int len = path.Length;
            for (int i = len - 1; i >= 0; i--)
            {
                if (path[i] == '.')
                    return path.Slice(i + 1);
            }
            return path;
        }

        public static ScreepsRoomHistory ComputeTick(JToken tickObject, ScreepsRoomHistory roomHistory)
        {
            var tickObjects = tickObject.Children().SelectMany(c => c.Children());
            var changesPerObjDictionary = new Dictionary<string, Dictionary<string, object?>>();

            foreach (var tickObj in tickObjects)
            {
                var id = GetLastPathSegment(tickObj.Path).ToString();

                if (tickObj.HasValues && tickObj is JObject && id != "undefined")
                {
                    var changes = new Dictionary<string, object?>();
                    JsonHelper.FlattenJson(tickObj, new StringBuilder(), changes);
                    changesPerObjDictionary.Add(id, changes);
                    if (!roomHistory.TypeMap.ContainsKey(id))
                    {
                        roomHistory.TypeMap.Add(id, changes["type"]?.ToString() ?? "unknown");
                    }

                    if (roomHistory.HistoryChangesDictionary != null)
                    {
                        roomHistory.HistoryChangesDictionary[id] = changes;
                    }
                }
                else
                {
                    roomHistory = RemoveFromRoomHistory(id, roomHistory);
                }
            }

            var controllerId = roomHistory.TypeMap.FirstOrDefault(kvp => kvp.Value == "controller").Key;
            if (controllerId != null && changesPerObjDictionary.TryGetValue(controllerId, out var controllerChanges))
            {
                changesPerObjDictionary.Remove(controllerId);
                roomHistory = UpdateRoomHistory(controllerId, roomHistory, controllerChanges);
            }

            foreach (var objChangesKvp in changesPerObjDictionary)
            {
                var objChanges = objChangesKvp.Value;
                roomHistory = UpdateRoomHistory(objChangesKvp.Key, roomHistory, objChanges);
            }

            if (ConfigSettingsState.LiveAssertRoomHistory)
            {
                AssertHistoryHelper.AssertHistory(roomHistory, tickObject);
            }

            return roomHistory;
        }
    }
}
