using UserTrackerShared.Models;
using UserTrackerShared.States;

namespace UserTrackerShared.Helpers
{
    public static class ScreepsRoomHistoryDtoHelper
    {
        private static void ProcessStore(Store currentStore, Store store)
        {
            if (store == null) return;
            
            var storeType = typeof(Store);
            var properties = storeType.GetProperties();
            
            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(decimal?) && prop.CanRead && prop.CanWrite)
                {
                    var currentValueRaw = prop.GetValue(currentStore);
                    var storeValueRaw = prop.GetValue(store);
                    
                    decimal currentValue = 0;
                    decimal storeValue = 0;
                    
                    if (currentValueRaw != null)
                    {
                        if (currentValueRaw is decimal d1)
                            currentValue = d1;
                        else if (currentValueRaw is int i1)
                            currentValue = i1;
                        else if (currentValueRaw is long l1)
                            currentValue = l1;
                        else if (currentValueRaw is double db1)
                            currentValue = (decimal)db1;
                        else if (currentValueRaw is float f1)
                            currentValue = (decimal)f1;
                    }
                    
                    if (storeValueRaw != null)
                    {
                        if (storeValueRaw is decimal d2)
                            storeValue = d2;
                        else if (storeValueRaw is int i2)
                            storeValue = i2;
                        else if (storeValueRaw is long l2)
                            storeValue = l2;
                        else if (storeValueRaw is double db2)
                            storeValue = (decimal)db2;
                        else if (storeValueRaw is float f2)
                            storeValue = (decimal)f2;
                    }
                    
                    prop.SetValue(currentStore, currentValue + (storeValue / ConfigSettingsState.TicksInObject));
                 }
            }
        }
        private static void CombineStore(Store currentStore, Store store)
        {
            if (store == null) return;

            var storeType = typeof(Store);
            var properties = storeType.GetProperties();

            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(decimal?) && prop.CanRead && prop.CanWrite)
                {
                    var currentValueRaw = prop.GetValue(currentStore);
                    var storeValueRaw = prop.GetValue(store);

                    decimal currentValue = 0;
                    decimal storeValue = 0;

                    if (currentValueRaw != null)
                    {
                        if (currentValueRaw is decimal d1)
                            currentValue = d1;
                        else if (currentValueRaw is int i1)
                            currentValue = i1;
                        else if (currentValueRaw is long l1)
                            currentValue = l1;
                        else if (currentValueRaw is double db1)
                            currentValue = (decimal)db1;
                        else if (currentValueRaw is float f1)
                            currentValue = (decimal)f1;
                    }

                    if (storeValueRaw != null)
                    {
                        if (storeValueRaw is decimal d2)
                            storeValue = d2;
                        else if (storeValueRaw is int i2)
                            storeValue = i2;
                        else if (storeValueRaw is long l2)
                            storeValue = l2;
                        else if (storeValueRaw is double db2)
                            storeValue = (decimal)db2;
                        else if (storeValueRaw is float f2)
                            storeValue = (decimal)f2;
                    }

                    prop.SetValue(currentStore, currentValue + storeValue);
                }
            }
        }
        public static StructuresDto ConvertStructures(Structures structures, StructuresDto structuresDto)
        {
            if (structures.Controller != null)
            {
                structuresDto.Controller.Count += 1m / ConfigSettingsState.TicksInObject;
                structuresDto.Controller.Level += structures.Controller.Level / ConfigSettingsState.TicksInObject;
                structuresDto.Controller.Progress += structures.Controller.Progress / ConfigSettingsState.TicksInObject;
                structuresDto.Controller.ProgressTotal += structures.Controller.ProgressTotal / ConfigSettingsState.TicksInObject;
                structuresDto.Controller.UserId = structures.Controller.User ?? "";
                structuresDto.Controller.ReservationUserId = structures.Controller.Reservation?.User;
                structuresDto.Controller.Upgraded = structures.Controller._upgraded / ConfigSettingsState.TicksInObject;
                
                structuresDto.Controller.OwnedUserIdCount += (structures.Controller.User != null) ? (1m / ConfigSettingsState.TicksInObject) : 0;
                structuresDto.Controller.ReservationUserIdCount += (structures.Controller.Reservation?.User != null) ? (1m / ConfigSettingsState.TicksInObject) : 0;
            }
            if (structures.Mineral != null)
            {
                structuresDto.Mineral.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            if (structures.Deposits != null)
            {
                foreach (var deposit in structures.Deposits.Select(x => x.Value))
                {
                    structuresDto.Deposit.Count += 1m / ConfigSettingsState.TicksInObject;
                }
            }
            foreach (var wall in structures.Walls)
            {
                structuresDto.Wall.Count += 1m / ConfigSettingsState.TicksInObject;
                structuresDto.Wall.Hits += wall.Value.Hits / ConfigSettingsState.TicksInObject;
            }
            foreach (var constructionSite in structures.ConstructionSites.Select(x => x.Value))
            {
                structuresDto.ConstructionSite.Count += 1m / ConfigSettingsState.TicksInObject;
                structuresDto.ConstructionSite.Progress += constructionSite.Progress / ConfigSettingsState.TicksInObject;
                structuresDto.ConstructionSite.ProgressTotal += constructionSite.ProgressTotal / ConfigSettingsState.TicksInObject;

                var typeBeingBuild = constructionSite.StructureType;
                if (typeBeingBuild != null)
                {
                    if (!structuresDto.ConstructionSite.TypesBuilding.TryGetValue(typeBeingBuild, out var current))
                    {
                        current = 0;
                    }
                    structuresDto.ConstructionSite.TypesBuilding[typeBeingBuild] = current + 1 / ConfigSettingsState.TicksInObject;
                }
            }
            foreach (var container in structures.Containers.Select(x => x.Value))
            {
                structuresDto.Container.Count += 1m / ConfigSettingsState.TicksInObject;

                if (container.Store != null)
                {
                    ProcessStore(structuresDto.Container.Store, container.Store);
                }
            }
            foreach (var extension in structures.Extensions.Select(x => x.Value))
            {
                structuresDto.Extension.Count += 1m / ConfigSettingsState.TicksInObject;
                structuresDto.Extension.Energy += extension.Store?.energy / ConfigSettingsState.TicksInObject ?? 0;
                structuresDto.Extension.EnergyCapacity += extension.StoreCapacityResource?.energy / ConfigSettingsState.TicksInObject ?? 0;
            }
            foreach (var extractor in structures.Extractors.Select(x => x.Value))
            {
                structuresDto.Extractor.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            foreach (var factory in structures.Factories.Select(x => x.Value))
            {
                structuresDto.Factory.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            foreach (var invderCore in structures.InvaderCores.Select(x => x.Value))
            {
                structuresDto.InvaderCore.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            foreach (var invderCore in structures.InvaderCores.Select(x => x.Value))
            {
                structuresDto.InvaderCore.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            foreach (var keeperLair in structures.KeeperLairs.Select(x => x.Value))
            {
                structuresDto.KeeperLair.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            foreach (var lab in structures.Labs.Select(x => x.Value))
            {
                structuresDto.Lab.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            foreach (var link in structures.Links.Select(x => x.Value))
            {
                structuresDto.Link.Count += 1m / ConfigSettingsState.TicksInObject;
                structuresDto.Link.Energy += link.Store?.energy / ConfigSettingsState.TicksInObject ?? 0;
                structuresDto.Link.EnergyCapacity += link.StoreCapacityResource?.energy / ConfigSettingsState.TicksInObject ?? 0;
            }
            foreach (var observer in structures.Observers.Select(x => x.Value))
            {
                structuresDto.Observer.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            foreach (var portal in structures.Portals.Select(x => x.Value))
            {
                structuresDto.Portal.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            foreach (var powerBank in structures.PowerBanks.Select(x => x.Value))
            {
                structuresDto.PowerBank.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            foreach (var powerSpawns in structures.PowerSpawns.Select(x => x.Value))
            {
                structuresDto.PowerSpawn.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            foreach (var rampart in structures.Ramparts.Select(x => x.Value))
            {
                structuresDto.Rampart.Count += 1m / ConfigSettingsState.TicksInObject;
                structuresDto.Rampart.Hits += rampart.Hits / ConfigSettingsState.TicksInObject;
            }
            foreach (var road in structures.Roads.Select(x => x.Value))
            {
                structuresDto.Road.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            foreach (var ruin in structures.Ruins.Select(x => x.Value))
            {
                structuresDto.Ruin.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            foreach (var rampart in structures.Ramparts.Select(x => x.Value))
            {
                structuresDto.Rampart.Count += 1m / ConfigSettingsState.TicksInObject;
                structuresDto.Rampart.Hits += rampart.Hits / ConfigSettingsState.TicksInObject;
            }
            foreach (var source in structures.Sources.Select(x => x.Value))
            {
                structuresDto.Source.Count += 1m / ConfigSettingsState.TicksInObject;
                structuresDto.Source.Energy += source.Energy / ConfigSettingsState.TicksInObject;
                structuresDto.Source.EnergyCapacity += source.EnergyCapacity / ConfigSettingsState.TicksInObject;
            }
            foreach (var rampart in structures.Ramparts.Select(x => x.Value))
            {
                structuresDto.Rampart.Count += 1m / ConfigSettingsState.TicksInObject;
                structuresDto.Rampart.Hits += rampart.Hits / ConfigSettingsState.TicksInObject;
            }
            foreach (var spawn in structures.Spawns.Select(x => x.Value))
            {
                structuresDto.Spawn.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            foreach (var storage in structures.Storages.Select(x => x.Value))
            {
                structuresDto.Storage.Count += 1m / ConfigSettingsState.TicksInObject;

                if (storage.Store != null)
                {
                    ProcessStore(structuresDto.Storage.Store, storage.Store);
                }
            }
            foreach (var terminal in structures.Terminals.Select(x => x.Value))
            {
                structuresDto.Terminal.Count += 1m / ConfigSettingsState.TicksInObject;

                if (terminal.Store != null)
                {
                    ProcessStore(structuresDto.Terminal.Store, terminal.Store);
                }
            }
            foreach (var tombstone in structures.Tombstones.Select(x => x.Value))
            {
                structuresDto.Tombstone.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            foreach (var tower in structures.Towers.Select(x => x.Value))
            {
                structuresDto.Tower.Count += 1m / ConfigSettingsState.TicksInObject;
                structuresDto.Tower.Energy += tower.Store?.energy / ConfigSettingsState.TicksInObject ?? 0;
            }
            foreach (var nuker in structures.Nukers.Select(x => x.Value))
            {
                structuresDto.Nuker.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            foreach (var nuke in structures.Nukes.Select(x => x.Value))
            {
                structuresDto.Nuke.Count += 1m / ConfigSettingsState.TicksInObject;
            }

            return structuresDto;
        }
        public static StructuresDto CombineStructures(StructuresDto structuresDtoA, StructuresDto structuresDtoB)
        {
            // Controller
            structuresDtoA.Controller.Count += structuresDtoB.Controller.Count;
            structuresDtoA.Controller.Level += structuresDtoB.Controller.Level;
            structuresDtoA.Controller.Progress += structuresDtoB.Controller.Progress;
            structuresDtoA.Controller.ProgressTotal += structuresDtoB.Controller.ProgressTotal;
            structuresDtoA.Controller.Upgraded += structuresDtoB.Controller.Upgraded;
            structuresDtoA.Controller.ReservationUserIdCount += structuresDtoB.Controller.ReservationUserIdCount;
            structuresDtoA.Controller.OwnedUserIdCount += structuresDtoB.Controller.OwnedUserIdCount;

            // Mineral, Deposit, Wall
            structuresDtoA.Mineral.Count += structuresDtoB.Mineral.Count;
            structuresDtoA.Deposit.Count += structuresDtoB.Deposit.Count;
            structuresDtoA.Wall.Count += structuresDtoB.Wall.Count;
            structuresDtoA.Wall.Hits += structuresDtoB.Wall.Hits;

            // ConstructionSite
            structuresDtoA.ConstructionSite.Count += structuresDtoB.ConstructionSite.Count;
            structuresDtoA.ConstructionSite.Progress += structuresDtoB.ConstructionSite.Progress;
            structuresDtoA.ConstructionSite.ProgressTotal += structuresDtoB.ConstructionSite.ProgressTotal;
            foreach (var kvp in structuresDtoB.ConstructionSite.TypesBuilding)
            {
                if (structuresDtoA.ConstructionSite.TypesBuilding.TryGetValue(kvp.Key, out var existingValue))
                    structuresDtoA.ConstructionSite.TypesBuilding[kvp.Key] = existingValue + kvp.Value;
                else
                    structuresDtoA.ConstructionSite.TypesBuilding[kvp.Key] = kvp.Value;
            }

            // Container
            structuresDtoA.Container.Count += structuresDtoB.Container.Count;
            CombineStore(structuresDtoA.Container.Store, structuresDtoB.Container.Store);

            // Extension
            structuresDtoA.Extension.Count += structuresDtoB.Extension.Count;
            structuresDtoA.Extension.Energy += structuresDtoB.Extension.Energy;
            structuresDtoA.Extension.EnergyCapacity += structuresDtoB.Extension.EnergyCapacity;

            // Extractor, Factory, InvaderCore, KeeperLair, Lab
            structuresDtoA.Extractor.Count += structuresDtoB.Extractor.Count;
            structuresDtoA.Factory.Count += structuresDtoB.Factory.Count;
            structuresDtoA.InvaderCore.Count += structuresDtoB.InvaderCore.Count;
            structuresDtoA.KeeperLair.Count += structuresDtoB.KeeperLair.Count;
            structuresDtoA.Lab.Count += structuresDtoB.Lab.Count;

            // Link
            structuresDtoA.Link.Count += structuresDtoB.Link.Count;
            structuresDtoA.Link.Energy += structuresDtoB.Link.Energy;
            structuresDtoA.Link.EnergyCapacity += structuresDtoB.Link.EnergyCapacity;

            // Observer, Portal, PowerBank, PowerSpawn
            structuresDtoA.Observer.Count += structuresDtoB.Observer.Count;
            structuresDtoA.Portal.Count += structuresDtoB.Portal.Count;
            structuresDtoA.PowerBank.Count += structuresDtoB.PowerBank.Count;
            structuresDtoA.PowerSpawn.Count += structuresDtoB.PowerSpawn.Count;

            // Rampart
            structuresDtoA.Rampart.Count += structuresDtoB.Rampart.Count;
            structuresDtoA.Rampart.Hits += structuresDtoB.Rampart.Hits;

            // Road, Ruin
            structuresDtoA.Road.Count += structuresDtoB.Road.Count;
            structuresDtoA.Ruin.Count += structuresDtoB.Ruin.Count;

            // Source
            structuresDtoA.Source.Count += structuresDtoB.Source.Count;
            structuresDtoA.Source.Energy += structuresDtoB.Source.Energy;
            structuresDtoA.Source.EnergyCapacity += structuresDtoB.Source.EnergyCapacity;

            // Spawn
            structuresDtoA.Spawn.Count += structuresDtoB.Spawn.Count;

            // Storage
            structuresDtoA.Storage.Count += structuresDtoB.Storage.Count;
            CombineStore(structuresDtoA.Storage.Store, structuresDtoB.Storage.Store);

            // Terminal
            structuresDtoA.Terminal.Count += structuresDtoB.Terminal.Count;
            CombineStore(structuresDtoA.Terminal.Store, structuresDtoB.Terminal.Store);

            // Tombstone, Tower, Nuker, Nuke
            structuresDtoA.Tombstone.Count += structuresDtoB.Tombstone.Count;
            structuresDtoA.Tower.Count += structuresDtoB.Tower.Count;
            structuresDtoA.Tower.Energy += structuresDtoB.Tower.Energy;
            structuresDtoA.Nuker.Count += structuresDtoB.Nuker.Count;
            structuresDtoA.Nuke.Count += structuresDtoB.Nuke.Count;

            return structuresDtoA;
        }
        public class IntentMapDto
        {
            public decimal Harvest { get; set; } = 0;
            public decimal Build { get; set; } = 0;
            public decimal Repair { get; set; } = 0;
            public decimal Dismantle { get; set; } = 0;
            public decimal UpgradeController { get; set; } = 0;
            public decimal Attack { get; set; } = 0;
            public decimal RangedAttack { get; set; } = 0;
            public decimal RangedMassAttack { get; set; } = 0;
            public decimal Heal { get; set; } = 0;
            public decimal RangedHeal { get; set; } = 0;
            public void Clear()
            {
                Harvest = 0;
                Build = 0;
                Repair = 0;
                Dismantle = 0;
                UpgradeController = 0;
                Attack = 0;
                RangedAttack = 0;
                RangedMassAttack = 0;
                Heal = 0;
                RangedHeal = 0;
            }
        }
        public static CreepDto ConvertCreeps(List<BaseCreep> creeps, CreepDto creepsDto)
        {
            var intentMap = new IntentMapDto();
            var creepsDtoBodyPart = new CountByPartDto();
            foreach (var creep in creeps)
            {
                creepsDto.Count += 1m / ConfigSettingsState.TicksInObject;
                ConvertBody(creep.Body, creepsDtoBodyPart);
                ComputeExtraIntentPower(creep.Body, creepsDtoBodyPart, intentMap);
                ConvertActiongLog(creep.ActionLog ?? new ActionLog(), creepsDto.ActionLog, creepsDtoBodyPart, intentMap, creep._oldFatigue);
                if (creep.Store != null)
                {
                    ProcessStore(creepsDto.Store, creep.Store);
                }

                creepsDto.BodyParts.Move += creepsDtoBodyPart.Move / ConfigSettingsState.TicksInObject;
                creepsDto.BodyParts.Work += creepsDtoBodyPart.Work / ConfigSettingsState.TicksInObject;
                creepsDto.BodyParts.Carry += creepsDtoBodyPart.Carry / ConfigSettingsState.TicksInObject;
                creepsDto.BodyParts.Attack += creepsDtoBodyPart.Attack / ConfigSettingsState.TicksInObject;
                creepsDto.BodyParts.RangedAttack += creepsDtoBodyPart.RangedAttack / ConfigSettingsState.TicksInObject;
                creepsDto.BodyParts.Tough += creepsDtoBodyPart.Tough / ConfigSettingsState.TicksInObject;
                creepsDto.BodyParts.Heal += creepsDtoBodyPart.Heal / ConfigSettingsState.TicksInObject;
                creepsDto.BodyParts.Claim += creepsDtoBodyPart.Claim / ConfigSettingsState.TicksInObject;

                creepsDtoBodyPart.Clear();
                intentMap.Clear();
            }
            return creepsDto;
        }
        public static void ConvertBody(BodyPart[] body, CountByPartDto bodyParts)
        {
            foreach (var bodyPart in body)
            {
                switch (bodyPart.Type)
                {
                    case "move":
                        bodyParts.Move += 1m;
                        break;
                    case "work":
                        bodyParts.Work += 1m;
                        break;
                    case "carry":
                        bodyParts.Carry += 1m;
                        break;
                    case "attack":
                        bodyParts.Attack += 1m;
                        break;
                    case "ranged_attack":
                        bodyParts.RangedAttack += 1m;
                        break;
                    case "tough":
                        bodyParts.Tough += 1m;
                        break;
                    case "heal":
                        bodyParts.Heal += 1m;
                        break;
                    case "claim":
                        bodyParts.Claim += 1m;
                        break;
                    default:
                        break;
                }
            }
        }
        public static IntentMapDto ComputeExtraIntentPower(BodyPart[] body, CountByPartDto countByPart, IntentMapDto intentMap)
        {
            foreach (var bodyPart in body)
            {
                if (bodyPart.Hits == 0) continue;
                switch (bodyPart.Type)
                {
                    case "work":
                        switch (bodyPart.Boost)
                        {
                            case "UO":
                                intentMap.Harvest += 2m;
                                break;
                            case "UHO2":
                                intentMap.Harvest += 4m;
                                break;
                            case "XUHO2":
                                intentMap.Harvest += 6m;
                                break;
                            case "LH":
                                intentMap.Build += 0.5m;
                                intentMap.Repair += 0.5m;
                                break;
                            case "LH2O":
                                intentMap.Build += 0.8m;
                                intentMap.Repair += 0.8m;
                                break;
                            case "XLH2O":
                                intentMap.Build += 1m;
                                intentMap.Repair += 1m;
                                break;
                            case "ZH":
                                intentMap.Dismantle += 0.5m;
                                break;
                            case "ZH2O":
                                intentMap.Dismantle += 2m;
                                break;
                            case "XZH2O":
                                intentMap.Dismantle += 3m;
                                break;
                            case "GH":
                                intentMap.UpgradeController += 0.5m;
                                break;
                            case "GH2O":
                                intentMap.UpgradeController += 0.8m;
                                break;
                            case "XGH2O":
                                intentMap.UpgradeController += 1m;
                                break;
                            default:
                                break;
                        }
                        break;
                    case "attack":
                        switch (bodyPart.Boost)
                        {
                            case "UH":
                                intentMap.Attack += 30m;
                                break;
                            case "UH2O":
                                intentMap.Attack += 60m;
                                break;
                            case "XUH2O":
                                intentMap.Attack += 90m;
                                break;
                            default:
                                break;
                        }
                        break;
                    case "ranged_attack":
                        switch (bodyPart.Boost)
                        {
                            case "KO":
                                intentMap.RangedAttack += 10m;
                                intentMap.RangedAttack += 10m;
                                break;
                            case "KHO2":
                                intentMap.RangedAttack += 20m;
                                intentMap.RangedAttack += 20m;
                                break;
                            case "XKHO2":
                                intentMap.RangedAttack += 30m;
                                intentMap.RangedAttack += 30m;
                                break;
                            default:
                                break;
                        }
                        break;
                    case "heal":
                        switch (bodyPart.Boost)
                        {
                            case "LO":
                                intentMap.Heal += 12m;
                                intentMap.RangedHeal += 4m;
                                break;
                            case "LHO2":
                                intentMap.Heal += 24m;
                                intentMap.RangedHeal += 8m;
                                break;
                            case "XLHO2":
                                intentMap.Heal += 36m;
                                intentMap.RangedHeal += 12m;
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
            return intentMap;
        }
        public static void ConvertActiongLog(ActionLog actionLog, ActionLogDto actionLogDto, CountByPartDto body, IntentMapDto intentPowerMap, decimal? creep_oldFatigue)
        {
            if (actionLog == null) return;

            var attackCount = body.Attack;
            var healCount = body.Heal;
            var workCount = body.Work;

            #region Damage
            if (actionLog.Attacked != null)
            {
                actionLogDto.Attacked.Count += 1m / ConfigSettingsState.TicksInObject;
                actionLogDto.Attacked.Damage += (decimal)Convert.ToInt64(Math.Round(attackCount * 30 + intentPowerMap.Attack * 30)) / ConfigSettingsState.TicksInObject;
            }
            if (actionLog.Attack != null)
            {
                actionLogDto.Attack.Count += 1m / ConfigSettingsState.TicksInObject;
                actionLogDto.Attack.Damage += (decimal)Convert.ToInt64(Math.Round(attackCount * 30 + intentPowerMap.Attack * 30)) / ConfigSettingsState.TicksInObject;
            }
            if (actionLog.RangedAttack != null)
            {
                actionLogDto.Attacked.Count += 1m / ConfigSettingsState.TicksInObject;
                actionLogDto.RangedAttack.Damage += (decimal)Convert.ToInt64(Math.Round(attackCount * 10 + intentPowerMap.RangedAttack * 10)) / ConfigSettingsState.TicksInObject;
            }
            if (actionLog.RangedMassAttack != null)
            {
                actionLogDto.RangedMassAttack.Count += 1m / ConfigSettingsState.TicksInObject;
                actionLogDto.RangedMassAttack.Damage += (decimal)Convert.ToInt64(Math.Round(attackCount * 4 + intentPowerMap.RangedAttack * 4)) / ConfigSettingsState.TicksInObject;
            }
            #endregion
            #region Heal
            if (actionLog.Heal != null)
            {
                actionLogDto.Heal.Count += 1m / ConfigSettingsState.TicksInObject;
                actionLogDto.Heal.Heal += (decimal)Convert.ToInt64(Math.Round(healCount * 12 + intentPowerMap.Heal * 12)) / ConfigSettingsState.TicksInObject;
            }
            if (actionLog.Healed != null)
            {
                actionLogDto.Healed.Count += 1m / ConfigSettingsState.TicksInObject;
                actionLogDto.Healed.Heal += (decimal)Convert.ToInt64(Math.Round(healCount * 12 + intentPowerMap.Heal * 12)) / ConfigSettingsState.TicksInObject;
            }
            if (actionLog.RangedHeal != null)
            {
                actionLogDto.RangedHeal.Count += 1m / ConfigSettingsState.TicksInObject;
                actionLogDto.RangedHeal.Heal += (decimal)Convert.ToInt64(Math.Round(healCount * 4 + intentPowerMap.RangedHeal * 4)) / ConfigSettingsState.TicksInObject;
            }
            #endregion
            #region Inflow
            if (actionLog.Harvest != null)
            {
                actionLogDto.Harvest.Count += 1m / ConfigSettingsState.TicksInObject;
                actionLogDto.Harvest.Inflow += (decimal)Convert.ToInt64(Math.Round(workCount * 2 + intentPowerMap.Harvest * 2));
            }
            #endregion
            #region Outflow
            if (actionLog.Repair != null)
            {
                actionLogDto.Repair.Count += 1m / ConfigSettingsState.TicksInObject;
                actionLogDto.Repair.Outflow += workCount / ConfigSettingsState.TicksInObject;
                actionLogDto.Repair.Effect += (decimal)Convert.ToInt64(Math.Round(workCount * 100 + intentPowerMap.Repair * 100)) / ConfigSettingsState.TicksInObject;
            }
            if (actionLog.Build != null)
            {
                actionLogDto.Build.Count += 1m / ConfigSettingsState.TicksInObject;
                actionLogDto.Build.Outflow += workCount / ConfigSettingsState.TicksInObject;
                actionLogDto.Build.Effect += (decimal)Convert.ToInt64(Math.Round(workCount * 5 + intentPowerMap.Build * 5)) / ConfigSettingsState.TicksInObject;
            }
            if (actionLog.UpgradeController != null)
            {
                actionLogDto.UpgradeController.Count += 1m / ConfigSettingsState.TicksInObject;
                actionLogDto.UpgradeController.Outflow += workCount / ConfigSettingsState.TicksInObject;
                actionLogDto.UpgradeController.Effect += (decimal)Convert.ToInt64(Math.Round(workCount + intentPowerMap.UpgradeController)) / ConfigSettingsState.TicksInObject;
            }
            #endregion
            #region Generic
            //actionLogDto.Move.Count += creep_oldFatigue == 0 ? 1m : 0m / ConfigSettingsState.TicksInObject;
            if (actionLog.Say != null)
            {
                actionLogDto.Say.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            if (actionLog.ReserveController != null)
            {
                actionLogDto.ReserveController.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            if (actionLog.Produce != null)
            {
                actionLogDto.Produce.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            if (actionLog.TransferEnergy != null)
            {
                actionLogDto.TransferEnergy.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            if (actionLog.AttackController != null)
            {
                actionLogDto.AttackController.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            if (actionLog.RunReaction != null)
            {
                actionLogDto.RunReaction.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            if (actionLog.ReverseReaction != null)
            {
                actionLogDto.ReverseReaction.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            if (actionLog.Spawned != null)
            {
                actionLogDto.Spawned.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            if (actionLog.Power != null)
            {
                actionLogDto.Power.Count += 1m / ConfigSettingsState.TicksInObject;
            }
            #endregion
        }

        public static CreepDto CombineCreeps(CreepDto a, CreepDto b)
        {
            a.Count += b.Count;
            a.Store = CombineStores(a.Store, b.Store);
            a.BodyParts = CombineCountByParts(a.BodyParts, b.BodyParts);
            a.ActionLog = CombineActionLogs(a.ActionLog, b.ActionLog);
            return a;
        }

        public static Store CombineStores(Store a, Store b)
        {
            if (a == null) a = new Store();
            if (b == null) return a;
            
            var storeType = typeof(Store);
            var properties = storeType.GetProperties();
            
            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(decimal?) && prop.CanRead && prop.CanWrite)
                {
                    var aValueRaw = prop.GetValue(a);
                    var bValueRaw = prop.GetValue(b);
                    
                    decimal aValue = 0;
                    decimal bValue = 0;
                    
                    if (aValueRaw != null)
                    {
                        if (aValueRaw is decimal d1)
                            aValue = d1;
                        else if (aValueRaw is int i1)
                            aValue = i1;
                        else if (aValueRaw is long l1)
                            aValue = l1;
                        else if (aValueRaw is double db1)
                            aValue = (decimal)db1;
                        else if (aValueRaw is float f1)
                            aValue = (decimal)f1;
                    }
                    
                    if (bValueRaw != null)
                    {
                        if (bValueRaw is decimal d2)
                            bValue = d2;
                        else if (bValueRaw is int i2)
                            bValue = i2;
                        else if (bValueRaw is long l2)
                            bValue = l2;
                        else if (bValueRaw is double db2)
                            bValue = (decimal)db2;
                        else if (bValueRaw is float f2)
                            bValue = (decimal)f2;
                    }
                    
                    prop.SetValue(a, aValue + bValue);
                }
            }
            
            return a;
        }

        public static CountByPartDto CombineCountByParts(CountByPartDto a, CountByPartDto b)
        {
            a.Move += b.Move;
            a.Work += b.Work;
            a.Carry += b.Carry;
            a.Attack += b.Attack;
            a.RangedAttack += b.RangedAttack;
            a.Tough += b.Tough;
            a.Heal += b.Heal;
            a.Claim += b.Claim;
            return a;
        }

        public static ActionLogDto CombineActionLogs(ActionLogDto a, ActionLogDto b)
        {
            a.Attacked.Count += b.Attacked.Count;
            a.Attacked.Damage += b.Attacked.Damage;
            a.Attack.Count += b.Attack.Count;
            a.Attack.Damage += b.Attack.Damage;
            a.RangedAttack.Count += b.RangedAttack.Count;
            a.RangedAttack.Damage += b.RangedAttack.Damage;
            a.RangedMassAttack.Count += b.RangedMassAttack.Count;
            a.RangedMassAttack.Damage += b.RangedMassAttack.Damage;
            a.RangedHeal.Count += b.RangedHeal.Count;
            a.RangedHeal.Heal += b.RangedHeal.Heal;
            a.Healed.Count += b.Healed.Count;
            a.Healed.Heal += b.Healed.Heal;
            a.Heal.Count += b.Heal.Count;
            a.Heal.Heal += b.Heal.Heal;
            a.Harvest.Count += b.Harvest.Count;
            a.Harvest.Inflow += b.Harvest.Inflow;
            a.Repair.Count += b.Repair.Count;
            a.Repair.Outflow += b.Repair.Outflow;
            a.Repair.Effect += b.Repair.Effect;
            a.Build.Count += b.Build.Count;
            a.Build.Outflow += b.Build.Outflow;
            a.Build.Effect += b.Build.Effect;
            a.UpgradeController.Count += b.UpgradeController.Count;
            a.UpgradeController.Outflow += b.UpgradeController.Outflow;
            a.UpgradeController.Effect += b.UpgradeController.Effect;
            a.Move.Count += b.Move.Count;
            a.Say.Count += b.Say.Count;
            a.ReserveController.Count += b.ReserveController.Count;
            a.Produce.Count += b.Produce.Count;
            a.TransferEnergy.Count += b.TransferEnergy.Count;
            a.AttackController.Count += b.AttackController.Count;
            a.RunReaction.Count += b.RunReaction.Count;
            a.ReverseReaction.Count += b.ReverseReaction.Count;
            a.Spawned.Count += b.Spawned.Count;
            a.Power.Count += b.Power.Count;
            return a;
        }
    }
}
