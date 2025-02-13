using UserTrackerShared.Models;

namespace UserTrackerShared.Helpers
{
    public static class ScreepsRoomHistoryDTOHelper
    {
        private static void UpdateStore(Store currentStore, Store store)
        {
            currentStore.energy = (currentStore.energy ?? 0) + (store.energy ?? 0);
            currentStore.power = (currentStore.power ?? 0) + (store.power ?? 0);
            currentStore.H = (currentStore.H ?? 0) + (store.H ?? 0);
            currentStore.O = (currentStore.O ?? 0) + (store.O ?? 0);
            currentStore.U = (currentStore.U ?? 0) + (store.U ?? 0);
            currentStore.L = (currentStore.L ?? 0) + (store.L ?? 0);
            currentStore.K = (currentStore.K ?? 0) + (store.K ?? 0);
            currentStore.Z = (currentStore.Z ?? 0) + (store.Z ?? 0);
            currentStore.X = (currentStore.X ?? 0) + (store.X ?? 0);
            currentStore.G = (currentStore.G ?? 0) + (store.G ?? 0);
            currentStore.silicon = (currentStore.silicon ?? 0) + (store.silicon ?? 0);
            currentStore.metal = (currentStore.metal ?? 0) + (store.metal ?? 0);
            currentStore.biomass = (currentStore.biomass ?? 0) + (store.biomass ?? 0);
            currentStore.mist = (currentStore.mist ?? 0) + (store.mist ?? 0);
            currentStore.OH = (currentStore.OH ?? 0) + (store.OH ?? 0);
            currentStore.ZK = (currentStore.ZK ?? 0) + (store.ZK ?? 0);
            currentStore.UL = (currentStore.UL ?? 0) + (store.UL ?? 0);
            currentStore.UH = (currentStore.UH ?? 0) + (store.UH ?? 0);
            currentStore.UO = (currentStore.UO ?? 0) + (store.UO ?? 0);
            currentStore.KH = (currentStore.KH ?? 0) + (store.KH ?? 0);
            currentStore.KO = (currentStore.KO ?? 0) + (store.KO ?? 0);
            currentStore.LH = (currentStore.LH ?? 0) + (store.LH ?? 0);
            currentStore.LO = (currentStore.LO ?? 0) + (store.LO ?? 0);
            currentStore.ZH = (currentStore.ZH ?? 0) + (store.ZH ?? 0);
            currentStore.ZO = (currentStore.ZO ?? 0) + (store.ZO ?? 0);
            currentStore.GH = (currentStore.GH ?? 0) + (store.GH ?? 0);
            currentStore.GO = (currentStore.GO ?? 0) + (store.GO ?? 0);
            currentStore.UH2O = (currentStore.UH2O ?? 0) + (store.UH2O ?? 0);
            currentStore.UHO2 = (currentStore.UHO2 ?? 0) + (store.UHO2 ?? 0);
            currentStore.KH2O = (currentStore.KH2O ?? 0) + (store.KH2O ?? 0);
            currentStore.KHO2 = (currentStore.KHO2 ?? 0) + (store.KHO2 ?? 0);
            currentStore.LH2O = (currentStore.LH2O ?? 0) + (store.LH2O ?? 0);
            currentStore.LHO2 = (currentStore.LHO2 ?? 0) + (store.LHO2 ?? 0);
            currentStore.ZH2O = (currentStore.ZH2O ?? 0) + (store.ZH2O ?? 0);
            currentStore.ZHO2 = (currentStore.ZHO2 ?? 0) + (store.ZHO2 ?? 0);
            currentStore.GH2O = (currentStore.GH2O ?? 0) + (store.GH2O ?? 0);
            currentStore.GHO2 = (currentStore.GHO2 ?? 0) + (store.GHO2 ?? 0);
            currentStore.XUH2O = (currentStore.XUH2O ?? 0) + (store.XUH2O ?? 0);
            currentStore.XUHO2 = (currentStore.XUHO2 ?? 0) + (store.XUHO2 ?? 0);
            currentStore.XKH2O = (currentStore.XKH2O ?? 0) + (store.XKH2O ?? 0);
            currentStore.XKHO2 = (currentStore.XKHO2 ?? 0) + (store.XKHO2 ?? 0);
            currentStore.XLH2O = (currentStore.XLH2O ?? 0) + (store.XLH2O ?? 0);
            currentStore.XLHO2 = (currentStore.XLHO2 ?? 0) + (store.XLHO2 ?? 0);
            currentStore.XZH2O = (currentStore.XZH2O ?? 0) + (store.XZH2O ?? 0);
            currentStore.XZHO2 = (currentStore.XZHO2 ?? 0) + (store.XZHO2 ?? 0);
            currentStore.XGH2O = (currentStore.XGH2O ?? 0) + (store.XGH2O ?? 0);
            currentStore.XGHO2 = (currentStore.XGHO2 ?? 0) + (store.XGHO2 ?? 0);
            currentStore.ops = (currentStore.ops ?? 0) + (store.ops ?? 0);
            currentStore.utrium_bar = (currentStore.utrium_bar ?? 0) + (store.utrium_bar ?? 0);
            currentStore.lemergium_bar = (currentStore.lemergium_bar ?? 0) + (store.lemergium_bar ?? 0);
            currentStore.zynthium_bar = (currentStore.zynthium_bar ?? 0) + (store.zynthium_bar ?? 0);
            currentStore.keanium_bar = (currentStore.keanium_bar ?? 0) + (store.keanium_bar ?? 0);
            currentStore.ghodium_melt = (currentStore.ghodium_melt ?? 0) + (store.ghodium_melt ?? 0);
            currentStore.oxidant = (currentStore.oxidant ?? 0) + (store.oxidant ?? 0);
            currentStore.reductant = (currentStore.reductant ?? 0) + (store.reductant ?? 0);
            currentStore.purifier = (currentStore.purifier ?? 0) + (store.purifier ?? 0);
            currentStore.battery = (currentStore.battery ?? 0) + (store.battery ?? 0);
            currentStore.composite = (currentStore.composite ?? 0) + (store.composite ?? 0);
            currentStore.crystal = (currentStore.crystal ?? 0) + (store.crystal ?? 0);
            currentStore.liquid = (currentStore.liquid ?? 0) + (store.liquid ?? 0);
            currentStore.wire = (currentStore.wire ?? 0) + (store.wire ?? 0);
            currentStore.Switch = (currentStore.Switch ?? 0) + (store.Switch ?? 0);
            currentStore.transistor = (currentStore.transistor ?? 0) + (store.transistor ?? 0);
            currentStore.microchip = (currentStore.microchip ?? 0) + (store.microchip ?? 0);
            currentStore.circuit = (currentStore.circuit ?? 0) + (store.circuit ?? 0);
            currentStore.device = (currentStore.device ?? 0) + (store.device ?? 0);
            currentStore.cell = (currentStore.cell ?? 0) + (store.cell ?? 0);
            currentStore.phlegm = (currentStore.phlegm ?? 0) + (store.phlegm ?? 0);
            currentStore.tissue = (currentStore.tissue ?? 0) + (store.tissue ?? 0);
            currentStore.muscle = (currentStore.muscle ?? 0) + (store.muscle ?? 0);
            currentStore.organoid = (currentStore.organoid ?? 0) + (store.organoid ?? 0);
            currentStore.organism = (currentStore.organism ?? 0) + (store.organism ?? 0);
            currentStore.alloy = (currentStore.alloy ?? 0) + (store.alloy ?? 0);
            currentStore.tube = (currentStore.tube ?? 0) + (store.tube ?? 0);
            currentStore.fixtures = (currentStore.fixtures ?? 0) + (store.fixtures ?? 0);
            currentStore.frame = (currentStore.frame ?? 0) + (store.frame ?? 0);
            currentStore.hydraulics = (currentStore.hydraulics ?? 0) + (store.hydraulics ?? 0);
            currentStore.machine = (currentStore.machine ?? 0) + (store.machine ?? 0);
            currentStore.condensate = (currentStore.condensate ?? 0) + (store.condensate ?? 0);
            currentStore.concentrate = (currentStore.concentrate ?? 0) + (store.concentrate ?? 0);
            currentStore.extract = (currentStore.extract ?? 0) + (store.extract ?? 0);
            currentStore.spirit = (currentStore.spirit ?? 0) + (store.spirit ?? 0);
            currentStore.emanation = (currentStore.emanation ?? 0) + (store.emanation ?? 0);
            currentStore.essence = (currentStore.essence ?? 0) + (store.essence ?? 0);
        }
        public static StructuresDTO ConvertStructures(Structures structures, StructuresDTO structuresDTO)
        {
            if (structures.Controller != null)
            {
                structuresDTO.Controller.Count = 1;
                structuresDTO.Controller.Level = structures.Controller.Level;
                structuresDTO.Controller.Progress = structures.Controller.Progress ?? 0;
                structuresDTO.Controller.ProgressTotal = structures.Controller.ProgressTotal;
            }
            if (structures.Mineral != null)
            {
                structuresDTO.Mineral.Count = 1;
            }
            if (structures.Deposit != null)
            {
                structuresDTO.Deposit.Count = 1;
            }
            foreach (var wall in structures.Walls)
            {
                structuresDTO.Wall.Count += 1;
                structuresDTO.Wall.Hits += wall.Value.Hits ?? 0;
            }
            foreach (var constructionSite in structures.ConstructionSites)
            {
                structuresDTO.ConstructionSite.Count += 1;
                structuresDTO.ConstructionSite.Progress += constructionSite.Value.Progress;
                structuresDTO.ConstructionSite.ProgressTotal += constructionSite.Value.ProgressTotal;

                var typeBeingBuild = constructionSite.Value.StructureType;
                var current = structuresDTO.ConstructionSite.TypesBuilding.ContainsKey(typeBeingBuild) ? structuresDTO.ConstructionSite.TypesBuilding[typeBeingBuild] : 0;
                structuresDTO.ConstructionSite.TypesBuilding[typeBeingBuild] = current + 1;
            }
            foreach (var container in structures.Containers)
            {
                structuresDTO.Container.Count += 1;

                if (container.Value.Store != null)
                {
                    UpdateStore(structuresDTO.Container.Store, container.Value.Store);
                }
            }
            foreach (var extension in structures.Extensions)
            {
                structuresDTO.Extension.Count += 1;
                structuresDTO.Extension.Energy += extension.Value.Store.energy ?? 0;
                structuresDTO.Extension.EnergyCapacity += extension.Value.StoreCapacityResource.energy ?? 0;
            }
            foreach (var extractor in structures.Extractors)
            {
                structuresDTO.Extractor.Count += 1;
            }
            foreach (var factory in structures.Factories)
            {
                structuresDTO.Factory.Count += 1;
            }
            foreach (var invderCore in structures.InvaderCores)
            {
                structuresDTO.InvaderCore.Count += 1;
            }
            foreach (var invderCore in structures.InvaderCores)
            {
                structuresDTO.InvaderCore.Count += 1;
            }
            foreach (var keeperLair in structures.KeeperLairs)
            {
                structuresDTO.KeeperLair.Count += 1;
            }
            foreach (var lab in structures.Labs)
            {
                structuresDTO.Lab.Count += 1;
            }
            foreach (var link in structures.Links)
            {
                structuresDTO.Link.Count += 1;
                structuresDTO.Link.Energy += link.Value.Store.energy ?? 0;
                structuresDTO.Link.EnergyCapacity += link.Value.StoreCapacityResource.energy ?? 0;
            }
            foreach (var observer in structures.Observers)
            {
                structuresDTO.Observer.Count += 1;
            }
            foreach (var portal in structures.Portals)
            {
                structuresDTO.Portal.Count += 1;
            }
            foreach (var powerBank in structures.PowerBanks)
            {
                structuresDTO.PowerBank.Count += 1;
            }
            foreach (var powerSpawns in structures.PowerSpawns)
            {
                structuresDTO.PowerSpawn.Count += 1;
            }
            foreach (var rampart in structures.Ramparts)
            {
                structuresDTO.Rampart.Count += 1;
                structuresDTO.Rampart.Hits += rampart.Value.Hits;
            }
            foreach (var road in structures.Roads)
            {
                structuresDTO.Road.Count += 1;
            }
            foreach (var ruin in structures.Ruins)
            {
                structuresDTO.Ruin.Count += 1;
            }
            foreach (var rampart in structures.Ramparts)
            {
                structuresDTO.Rampart.Count += 1;
                structuresDTO.Rampart.Hits += rampart.Value.Hits;
            }
            foreach (var source in structures.Sources)
            {
                structuresDTO.Source.Count += 1;
                structuresDTO.Source.Energy += source.Value.Energy;
                structuresDTO.Source.EnergyCapacity += source.Value.EnergyCapacity;
            }
            foreach (var rampart in structures.Ramparts)
            {
                structuresDTO.Rampart.Count += 1;
                structuresDTO.Rampart.Hits += rampart.Value.Hits;
            }
            foreach (var spawn in structures.Spawns)
            {
                structuresDTO.Spawn.Count += 1;
            }
            foreach (var storage in structures.Storages)
            {
                structuresDTO.Storage.Count += 1;

                if (storage.Value.Store != null)
                {
                    UpdateStore(structuresDTO.Storage.Store, storage.Value.Store);
                }
            }
            foreach (var terminal in structures.Terminals)
            {
                structuresDTO.Terminal.Count += 1;

                if (terminal.Value.Store != null)
                {
                    UpdateStore(structuresDTO.Terminal.Store, terminal.Value.Store);
                }
            }
            foreach (var tombstone in structures.Tombstones)
            {
                structuresDTO.Tombstone.Count += 1;
            }
            foreach (var tower in structures.Towers)
            {
                structuresDTO.Tower.Count += 1;
                structuresDTO.Tower.Energy += tower.Value.Store?.energy ?? 0;
            }
            foreach (var nuker in structures.Nukers)
            {
                structuresDTO.Nuker.Count += 1;
            }
            foreach (var nuke in structures.Nukes)
            {
                structuresDTO.Nuke.Count += 1;
            }

            return structuresDTO;
        }
        public class IntentMapDTO
        {
            public double Harvest { get; set; } = 0;
            public double Build { get; set; } = 0;
            public double Repair { get; set; } = 0;
            public double Dismantle { get; set; } = 0;
            public double UpgradeController { get; set; } = 0;
            public double Attack { get; set; } = 0;
            public double RangedAttack { get; set; } = 0;
            public double RangedMassAttack { get; set; } = 0;
            public double Heal { get; set; } = 0;
            public double RangedHeal { get; set; } = 0;
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
        public static CreepDTO ConvertCreeps(List<BaseCreep> creeps, CreepDTO creepsDTO)
        {
            var intentMap = new IntentMapDTO();
            var creepsDTOBodyPart = new CountByPartDTO();
            foreach (var creep in creeps)
            {
                creepsDTO.Count += 1;
                ConvertBody(creep.Body, creepsDTOBodyPart);
                ComputeExtraIntentPower(creep.Body, creepsDTOBodyPart, intentMap);
                ConvertActiongLog(creep.ActionLog, creepsDTO.ActionLog, creepsDTOBodyPart, intentMap, creep._oldFatigue);
                if (creep.Store != null)
                {
                    UpdateStore(creepsDTO.Store, creep.Store);
                }

                creepsDTO.BodyParts.Move += creepsDTOBodyPart.Move;
                creepsDTO.BodyParts.Work += creepsDTOBodyPart.Work;
                creepsDTO.BodyParts.Carry += creepsDTOBodyPart.Carry;
                creepsDTO.BodyParts.Attack += creepsDTOBodyPart.Attack;
                creepsDTO.BodyParts.RangedAttack += creepsDTOBodyPart.RangedAttack;
                creepsDTO.BodyParts.Tough += creepsDTOBodyPart.Tough;
                creepsDTO.BodyParts.Heal += creepsDTOBodyPart.Heal;
                creepsDTO.BodyParts.Claim += creepsDTOBodyPart.Claim;

                creepsDTOBodyPart.Clear();
                intentMap.Clear();
            }
            return creepsDTO;
        }
        public static void ConvertBody(BodyPart[] body, CountByPartDTO bodyParts)
        {
            foreach (var bodyPart in body)
            {
                switch (bodyPart.Type)
                {
                    case "move":
                        bodyParts.Move += 1;
                        break;
                    case "work":
                        bodyParts.Work += 1;
                        break;
                    case "carry":
                        bodyParts.Carry += 1;
                        break;
                    case "attack":
                        bodyParts.Attack += 1;
                        break;
                    case "ranged_attack":
                        bodyParts.RangedAttack += 1;
                        break;
                    case "tough":
                        bodyParts.Tough += 1;
                        break;
                    case "heal":
                        bodyParts.Heal += 1;
                        break;
                    case "claim":
                        bodyParts.Claim += 1;
                        break;
                    default:
                        break;
                }
            }
        }
        public static IntentMapDTO ComputeExtraIntentPower(BodyPart[] body, CountByPartDTO countByPart, IntentMapDTO intentMap)
        {
            var bodyDict = new Dictionary<string, long>();
            foreach (var bodyPart in body)
            {
                if (bodyPart.Hits == 0) continue;
                switch (bodyPart.Type)
                {
                    case "work":
                        switch (bodyPart.Boost)
                        {
                            case "UO":
                                intentMap.Harvest += 2;
                                break;
                            case "UHO2":
                                intentMap.Harvest += 4;
                                break;
                            case "XUHO2":
                                intentMap.Harvest += 6;
                                break;
                            case "LH":
                                intentMap.Build += 0.5;
                                intentMap.Repair += 0.5;
                                break;
                            case "LH2O":
                                intentMap.Build += 0.8;
                                intentMap.Repair += 0.8;
                                break;
                            case "XLH2O":
                                intentMap.Build += 1;
                                intentMap.Repair += 1;
                                break;
                            case "ZH":
                                intentMap.Dismantle += 0.5;
                                break;
                            case "ZH2O":
                                intentMap.Dismantle += 2;
                                break;
                            case "XZH2O":
                                intentMap.Dismantle += 3;
                                break;
                            case "GH":
                                intentMap.UpgradeController += 0.5;
                                break;
                            case "GH2O":
                                intentMap.UpgradeController += 0.8;
                                break;
                            case "XGH2O":
                                intentMap.UpgradeController += 1;
                                break;
                            default:
                                break;
                        }
                        break;
                    case "attack":
                        switch (bodyPart.Boost)
                        {
                            case "UH":
                                intentMap.Attack += 30;
                                break;
                            case "UH2O":
                                intentMap.Attack += 60;
                                break;
                            case "XUH2O":
                                intentMap.Attack += 90;
                                break;
                            default:
                                break;
                        }
                        break;
                    case "ranged_attack":
                        switch (bodyPart.Boost)
                        {
                            case "KO":
                                intentMap.RangedAttack += 10;
                                intentMap.RangedAttack += 10;
                                break;
                            case "KHO2":
                                intentMap.RangedAttack += 20;
                                intentMap.RangedAttack += 20;
                                break;
                            case "XKHO2":
                                intentMap.RangedAttack += 30;
                                intentMap.RangedAttack += 30;
                                break;
                            default:
                                break;
                        }
                        break;
                    case "heal":
                        switch (bodyPart.Boost)
                        {
                            case "LO":
                                intentMap.Heal += 12;
                                intentMap.RangedHeal += 4;
                                break;
                            case "LHO2":
                                intentMap.Heal += 24;
                                intentMap.RangedHeal += 8;
                                break;
                            case "XLHO2":
                                intentMap.Heal += 36;
                                intentMap.RangedHeal += 12;
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
        public static void ConvertActiongLog(ActionLog actionLog, ActionLogDTO actionLogDTO, CountByPartDTO body, IntentMapDTO intentPowerMap, long? creep_oldFatigue)
        {
            if (actionLog == null) return;

            var attackCount = body.Attack;
            var healCount = body.Heal;
            var workCount = body.Work;

            #region Damage
            if (actionLog.Attacked != null)
            {
                actionLogDTO.Attacked.Count += 1;
                actionLogDTO.Attacked.Damage += Convert.ToInt64(Math.Round(attackCount * 30 + intentPowerMap.Attack * 30));
            }
            if (actionLog.Attack != null)
            {
                actionLogDTO.Attack.Count += 1;
                actionLogDTO.Attack.Damage += Convert.ToInt64(Math.Round(attackCount * 30 + intentPowerMap.Attack * 30));
            }
            if (actionLog.RangedAttack != null)
            {
                actionLogDTO.Attacked.Count += 1;
                actionLogDTO.RangedAttack.Damage += Convert.ToInt64(Math.Round(attackCount * 10 + intentPowerMap.RangedAttack * 10));
            }
            if (actionLog.RangedMassAttack != null)
            {
                actionLogDTO.RangedMassAttack.Count += 1;
                actionLogDTO.RangedMassAttack.Damage += Convert.ToInt64(Math.Round(attackCount * 4 + intentPowerMap.RangedAttack * 4));
            }
            #endregion
            #region Heal
            if (actionLog.Heal != null)
            {
                actionLogDTO.Heal.Count += 1;
                actionLogDTO.Heal.Heal += Convert.ToInt64(Math.Round(healCount * 12 + intentPowerMap.Heal * 12));
            }
            if (actionLog.Healed != null)
            {
                actionLogDTO.Healed.Count += 1;
                actionLogDTO.Healed.Heal += Convert.ToInt64(Math.Round(healCount * 12 + intentPowerMap.Heal * 12));
            }
            if (actionLog.RangedHeal != null)
            {
                actionLogDTO.RangedHeal.Count += 1;
                actionLogDTO.RangedHeal.Heal += Convert.ToInt64(Math.Round(healCount * 4 + intentPowerMap.RangedHeal * 4));
            }
            #endregion
            #region Inflow
            if (actionLog.Harvest != null)
            {
                actionLogDTO.Harvest.Count += 1;
                actionLogDTO.Harvest.Inflow += Convert.ToInt64(Math.Round(workCount * 2 + intentPowerMap.Harvest * 2));
            }
            #endregion
            #region Outflow
            if (actionLog.Repair != null)
            {
                actionLogDTO.Repair.Count += 1;
                actionLogDTO.Repair.Outlflow += workCount;
                actionLogDTO.Repair.Effect += Convert.ToInt64(Math.Round(workCount * 100 + intentPowerMap.Repair * 100));
            }
            if (actionLog.Build != null)
            {
                actionLogDTO.Build.Count += 1;
                actionLogDTO.Build.Outlflow += workCount;
                actionLogDTO.Build.Effect += Convert.ToInt64(Math.Round(workCount * 5 + intentPowerMap.Build * 5));
            }
            if (actionLog.UpgradeController != null)
            {
                actionLogDTO.UpgradeController.Count += 1;
                actionLogDTO.UpgradeController.Outlflow += workCount;
                actionLogDTO.UpgradeController.Effect += Convert.ToInt64(Math.Round(workCount + intentPowerMap.UpgradeController));
            }
            #endregion
            #region Generic
            actionLogDTO.Move.Count += creep_oldFatigue == 0 ? 1 : 0;
            if (actionLog.Say != null)
            {
                actionLogDTO.Say.Count += 1;
            }
            if (actionLog.ReserveController != null)
            {
                actionLogDTO.ReserveController.Count += 1;
            }
            if (actionLog.Produce != null)
            {
                actionLogDTO.Produce.Count += 1;
            }
            if (actionLog.TransferEnergy != null)
            {
                actionLogDTO.TransferEnergy.Count += 1;
            }
            if (actionLog.AttackController != null)
            {
                actionLogDTO.AttackController.Count += 1;
            }
            if (actionLog.RunReaction != null)
            {
                actionLogDTO.RunReaction.Count += 1;
            }
            if (actionLog.ReverseReaction != null)
            {
                actionLogDTO.ReverseReaction.Count += 1;
            }
            if (actionLog.Spawned != null)
            {
                actionLogDTO.Spawned.Count += 1;
            }
            if (actionLog.Power != null)
            {
                actionLogDTO.Power.Count += 1;
            }
            #endregion
        }
    }
}
