using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserTrackerShared.Models;

namespace UserTracker.Tests.Helper
{
    public class GetObjectFromHistory
    {
        private static readonly JsonSerializer _serializer = JsonSerializer.CreateDefault();
        private static JObject ConvertToJObject<T>(T model)
        {
            var writer = new JTokenWriter();
            _serializer.Serialize(writer, model);
            return (JObject)writer.Token!;
        }
        public static JObject GetById(ScreepsRoomHistory roomHistory, string id)
        {
            var type = roomHistory.TypeMap[id];
            switch (type)
            {
                case "energy":
                    roomHistory.GroundResources.TryGetValue(id, out var objGroundResource);
                    if (objGroundResource != null)
                        return ConvertToJObject(objGroundResource);
                    break;
                case "creep":
                    roomHistory.Creeps.OwnedCreeps.TryGetValue(type, out var objOwnedCreep);
                    if (objOwnedCreep != null)
                        return ConvertToJObject(objOwnedCreep);
                    roomHistory.Creeps.OwnedCreeps.TryGetValue(type, out var objEnemyCreep);
                    if (objEnemyCreep != null)
                        return ConvertToJObject(objEnemyCreep);
                    roomHistory.Creeps.OwnedCreeps.TryGetValue(type, out var objOtherCreep);
                    if (objOtherCreep != null)
                        return ConvertToJObject(objOtherCreep);
                    break;
                case "powerCreep":
                    roomHistory.Creeps.PowerCreeps.TryGetValue(type, out var objPowerCreep);
                    if (objPowerCreep != null)
                        return ConvertToJObject(objPowerCreep);
                    break;
                case "controller":
                    if (roomHistory.Structures.Controller != null && roomHistory.Structures.Controller.Id == id)
                        return ConvertToJObject(roomHistory.Structures.Controller);
                    break;
                case "mineral":
                    if (roomHistory.Structures.Mineral != null && roomHistory.Structures.Mineral.Id == id)
                        return ConvertToJObject(roomHistory.Structures.Mineral);
                    break;
                case "deposit":
                    if (roomHistory.Structures.Deposit != null && roomHistory.Structures.Deposit.Id == id)
                        return ConvertToJObject(roomHistory.Structures.Deposit);
                    break;
                case "constructedWall":
                    roomHistory.Structures.Walls.TryGetValue(id, out var objWall);
                    if (objWall != null)
                        return ConvertToJObject(objWall);
                    break;
                case "constructionSite":
                    roomHistory.Structures.ConstructionSites.TryGetValue(id, out var objConstructionSite);
                    if (objConstructionSite != null)
                        return ConvertToJObject(objConstructionSite);
                    break;
                case "container":
                    roomHistory.Structures.Containers.TryGetValue(id, out var objContainer);
                    if (objContainer != null)
                        return ConvertToJObject(objContainer);
                    break;
                case "extension":
                    roomHistory.Structures.Extensions.TryGetValue(id, out var objExtension);
                    if (objExtension != null)
                        return ConvertToJObject(objExtension);
                    break;
                case "extractor":
                    roomHistory.Structures.Extractors.TryGetValue(id, out var objExtractor);
                    if (objExtractor != null)
                        return ConvertToJObject(objExtractor);
                    break;
                case "factory":
                    roomHistory.Structures.Factories.TryGetValue(id, out var objFactory);
                    if (objFactory != null)
                        return ConvertToJObject(objFactory);
                    break;
                case "invaderCore":
                    roomHistory.Structures.InvaderCores.TryGetValue(id, out var objInvaderCore);
                    if (objInvaderCore != null)
                        return ConvertToJObject(objInvaderCore);
                    break;
                case "keeperLair":
                    roomHistory.Structures.KeeperLairs.TryGetValue(id, out var objKeeperLair);
                    if (objKeeperLair != null)
                        return ConvertToJObject(objKeeperLair);
                    break;
                case "lab":
                    roomHistory.Structures.Labs.TryGetValue(id, out var objLab);
                    if (objLab != null)
                        return ConvertToJObject(objLab);
                    break;
                case "link":
                    roomHistory.Structures.Links.TryGetValue(id, out var objLink);
                    if (objLink != null)
                        return ConvertToJObject(objLink);
                    break;
                case "nuker":
                    roomHistory.Structures.Nukers.TryGetValue(id, out var objNuker);
                    if (objNuker != null)
                        return ConvertToJObject(objNuker);
                    break;
                case "observer":
                    roomHistory.Structures.Observers.TryGetValue(id, out var objObserver);
                    if (objObserver != null)
                        return ConvertToJObject(objObserver);
                    break;
                case "portal":
                    roomHistory.Structures.Portals.TryGetValue(id, out var objPortal);
                    if (objPortal != null)
                        return ConvertToJObject(objPortal);
                    break;
                case "powerBank":
                    roomHistory.Structures.PowerBanks.TryGetValue(id, out var objPowerBank);
                    if (objPowerBank != null)
                        return ConvertToJObject(objPowerBank);
                    break;
                case "powerSpawn":
                    roomHistory.Structures.PowerSpawns.TryGetValue(id, out var objPowerSpawn);
                    if (objPowerSpawn != null)
                        return ConvertToJObject(objPowerSpawn);
                    break;
                case "rampart":
                    roomHistory.Structures.Ramparts.TryGetValue(id, out var objRampart);
                    if (objRampart != null)
                        return ConvertToJObject(objRampart);
                    break;
                case "road":
                    roomHistory.Structures.Roads.TryGetValue(id, out var objRoad);
                    if (objRoad != null)
                        return ConvertToJObject(objRoad);
                    break;
                case "ruin":
                    roomHistory.Structures.Ruins.TryGetValue(id, out var objRuin);
                    if (objRuin != null)
                        return ConvertToJObject(objRuin);
                    break;
                case "source":
                    roomHistory.Structures.Sources.TryGetValue(id, out var objSource);
                    if (objSource != null)
                        return ConvertToJObject(objSource);
                    break;
                case "spawn":
                    roomHistory.Structures.Spawns.TryGetValue(id, out var objSpawn);
                    if (objSpawn != null)
                        return ConvertToJObject(objSpawn);
                    break;
                case "storage":
                    roomHistory.Structures.Storages.TryGetValue(id, out var objStorage);
                    if (objStorage != null)
                        return ConvertToJObject(objStorage);
                    break;
                case "terminal":
                    roomHistory.Structures.Terminals.TryGetValue(id, out var objTerminal);
                    if (objTerminal != null)
                        return ConvertToJObject(objTerminal);
                    break;
                case "tombstone":
                    roomHistory.Structures.Tombstones.TryGetValue(id, out var objTombstone);
                    if (objTombstone != null)
                        return ConvertToJObject(objTombstone);
                    break;
                case "tower":
                    roomHistory.Structures.Towers.TryGetValue(id, out var objTower);
                    if (objTower != null)
                        return ConvertToJObject(objTower);
                    break;
                case "nuke":
                    roomHistory.Structures.Nukes.TryGetValue(id, out var objNuke);
                    if (objNuke != null)
                        return ConvertToJObject(objNuke);
                    break;
                default:
                    throw new ArgumentException($"Unsupported type {type}");
            }

            return default;
        }
    }
}
