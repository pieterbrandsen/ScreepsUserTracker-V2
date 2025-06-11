using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserTrackerShared.Models;

namespace UserTracker.Tests.Helper
{
    public class GetObjectFromHistory
    {
        protected GetObjectFromHistory()
        {
        }

        public static object? GetById(ScreepsRoomHistory roomHistory, string id)
        {
            var type = roomHistory.TypeMap[id];
            switch (type)
            {
                case "energy":
                    roomHistory.GroundResources.TryGetValue(id, out var objGroundResource);
                    if (objGroundResource != null)
                        return objGroundResource;
                    break;
                case "creep":
                    roomHistory.Creeps.OwnedCreeps.TryGetValue(id, out var objOwnedCreep);
                    if (objOwnedCreep != null)
                        return objOwnedCreep;
                    roomHistory.Creeps.EnemyCreeps.TryGetValue(id, out var objEnemyCreep);
                    if (objEnemyCreep != null)
                        return objEnemyCreep;
                    roomHistory.Creeps.OtherCreeps.TryGetValue(id, out var objOtherCreep);
                    if (objOtherCreep != null)
                        return objOtherCreep;
                    break;
                case "powerCreep":
                    roomHistory.Creeps.PowerCreeps.TryGetValue(id, out var objPowerCreep);
                    if (objPowerCreep != null)
                        return objPowerCreep;
                    break;
                case "controller":
                    if (roomHistory.Structures.Controller != null && roomHistory.Structures.Controller.Id == id)
                        return roomHistory.Structures.Controller;
                    break;
                case "mineral":
                    if (roomHistory.Structures.Mineral != null && roomHistory.Structures.Mineral.Id == id)
                        return roomHistory.Structures.Mineral;
                    break;
                case "deposit":
                    if (roomHistory.Structures.Deposits.TryGetValue(id, out var objDeposit))
                        return objDeposit;
                    break;
                case "constructedWall":
                    roomHistory.Structures.Walls.TryGetValue(id, out var objWall);
                    if (objWall != null)
                        return objWall;
                    break;
                case "constructionSite":
                    roomHistory.Structures.ConstructionSites.TryGetValue(id, out var objConstructionSite);
                    if (objConstructionSite != null)
                        return objConstructionSite;
                    break;
                case "container":
                    roomHistory.Structures.Containers.TryGetValue(id, out var objContainer);
                    if (objContainer != null)
                        return objContainer;
                    break;
                case "extension":
                    roomHistory.Structures.Extensions.TryGetValue(id, out var objExtension);
                    if (objExtension != null)
                        return objExtension;
                    break;
                case "extractor":
                    roomHistory.Structures.Extractors.TryGetValue(id, out var objExtractor);
                    if (objExtractor != null)
                        return objExtractor;
                    break;
                case "factory":
                    roomHistory.Structures.Factories.TryGetValue(id, out var objFactory);
                    if (objFactory != null)
                        return objFactory;
                    break;
                case "invaderCore":
                    roomHistory.Structures.InvaderCores.TryGetValue(id, out var objInvaderCore);
                    if (objInvaderCore != null)
                        return objInvaderCore;
                    break;
                case "keeperLair":
                    roomHistory.Structures.KeeperLairs.TryGetValue(id, out var objKeeperLair);
                    if (objKeeperLair != null)
                        return objKeeperLair;
                    break;
                case "lab":
                    roomHistory.Structures.Labs.TryGetValue(id, out var objLab);
                    if (objLab != null)
                        return objLab;
                    break;
                case "link":
                    roomHistory.Structures.Links.TryGetValue(id, out var objLink);
                    if (objLink != null)
                        return objLink;
                    break;
                case "nuker":
                    roomHistory.Structures.Nukers.TryGetValue(id, out var objNuker);
                    if (objNuker != null)
                        return objNuker;
                    break;
                case "observer":
                    roomHistory.Structures.Observers.TryGetValue(id, out var objObserver);
                    if (objObserver != null)
                        return objObserver;
                    break;
                case "portal":
                    roomHistory.Structures.Portals.TryGetValue(id, out var objPortal);
                    if (objPortal != null)
                        return objPortal;
                    break;
                case "powerBank":
                    roomHistory.Structures.PowerBanks.TryGetValue(id, out var objPowerBank);
                    if (objPowerBank != null)
                        return objPowerBank;
                    break;
                case "powerSpawn":
                    roomHistory.Structures.PowerSpawns.TryGetValue(id, out var objPowerSpawn);
                    if (objPowerSpawn != null)
                        return objPowerSpawn;
                    break;
                case "rampart":
                    roomHistory.Structures.Ramparts.TryGetValue(id, out var objRampart);
                    if (objRampart != null)
                        return objRampart;
                    break;
                case "road":
                    roomHistory.Structures.Roads.TryGetValue(id, out var objRoad);
                    if (objRoad != null)
                        return objRoad;
                    break;
                case "ruin":
                    roomHistory.Structures.Ruins.TryGetValue(id, out var objRuin);
                    if (objRuin != null)
                        return objRuin;
                    break;
                case "source":
                    roomHistory.Structures.Sources.TryGetValue(id, out var objSource);
                    if (objSource != null)
                        return objSource;
                    break;
                case "spawn":
                    roomHistory.Structures.Spawns.TryGetValue(id, out var objSpawn);
                    if (objSpawn != null)
                        return objSpawn;
                    break;
                case "storage":
                    roomHistory.Structures.Storages.TryGetValue(id, out var objStorage);
                    if (objStorage != null)
                        return objStorage;
                    break;
                case "terminal":
                    roomHistory.Structures.Terminals.TryGetValue(id, out var objTerminal);
                    if (objTerminal != null)
                        return objTerminal;
                    break;
                case "tombstone":
                    roomHistory.Structures.Tombstones.TryGetValue(id, out var objTombstone);
                    if (objTombstone != null)
                        return objTombstone;
                    break;
                case "tower":
                    roomHistory.Structures.Towers.TryGetValue(id, out var objTower);
                    if (objTower != null)
                        return objTower;
                    break;
                case "nuke":
                    roomHistory.Structures.Nukes.TryGetValue(id, out var objNuke);
                    if (objNuke != null)
                        return objNuke;
                    break;
                default:
                    throw new ArgumentException($"Unsupported type {type}");
            }

            return default;
        }
    }
}
