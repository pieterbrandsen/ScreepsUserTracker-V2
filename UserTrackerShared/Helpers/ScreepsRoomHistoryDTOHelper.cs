using UserTrackerShared.Models;

namespace UserTrackerShared.Helpers
{
    public static class ScreepsRoomHistoryDTOHelper
    {
        public static StructuresDTO ConvertStructures(Structures structures)
        {
            var structuresDTO = new StructuresDTO();

            if (structures.Controller != null)
            {
                structuresDTO.Controller.Count = 1;
                structuresDTO.Controller.Level = structures.Controller.Level;
                structuresDTO.Controller.Progress = structures.Controller.Progress;
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
                structuresDTO.Wall.Hits += wall.Value.Hits;
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
                    var storeProperties = container.Value.Store.GetType().GetProperties();
                    foreach (var storeProperty in storeProperties)
                    {
                        var currentAmount = structuresDTO.Container.Store.ContainsKey(storeProperty.Name) ? structuresDTO.Container.Store[storeProperty.Name] : 0;
                        var toBeAddedAmount = Convert.ToInt64(storeProperty.GetValue(container.Value.Store));
                        structuresDTO.Container.Store[storeProperty.Name] = currentAmount + Convert.ToInt64(storeProperty.GetValue(container.Value.Store));
                    }
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

                var storeProperties = storage.Value.Store.GetType().GetProperties();
                foreach (var storeProperty in storeProperties)
                {
                    if (storage.Value.Store != null)
                    {
                        var currentAmount = structuresDTO.Storage.Store.ContainsKey(storeProperty.Name) ? structuresDTO.Storage.Store[storeProperty.Name] : 0;
                        var toBeAddedAmount = Convert.ToInt64(storeProperty.GetValue(storage.Value.Store));
                        structuresDTO.Storage.Store[storeProperty.Name] = currentAmount + Convert.ToInt64(storeProperty.GetValue(storage.Value.Store));
                    }
                }
            }
            foreach (var terminal in structures.Terminals)
            {
                structuresDTO.Terminal.Count += 1;

                var storeProperties = terminal.Value.Store.GetType().GetProperties();
                foreach (var storeProperty in storeProperties)
                {
                    if (terminal.Value.Store != null)
                    {
                        var currentAmount = structuresDTO.Terminal.Store.ContainsKey(storeProperty.Name) ? structuresDTO.Terminal.Store[storeProperty.Name] : 0;
                        var toBeAddedAmount = Convert.ToInt64(storeProperty.GetValue(terminal.Value.Store));
                        structuresDTO.Terminal.Store[storeProperty.Name] = currentAmount + Convert.ToInt64(storeProperty.GetValue(terminal.Value.Store));
                    }
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
        public static CreepDTO ConvertCreeps(List<BaseCreep> creeps)
        {
            var creepsDTO = new CreepDTO();
            foreach (var creep in creeps)
            {
                creepsDTO.Count += 1;
                var body = ConvertBody(creep.Body);
                var actionLog = ConvertActiongLog(creep.ActionLog, body, ComputeExtraIntentPower(creep.Body, body));

                foreach (var bodyPart in body)
                {
                    var currentAmount = creepsDTO.BodyParts.ContainsKey(bodyPart.Key) ? creepsDTO.BodyParts[bodyPart.Key] : 0;
                    creepsDTO.BodyParts[bodyPart.Key] = currentAmount + bodyPart.Value;
                }

                if (creep.Store != null)
                {
                    var storeProperties = creep.Store.GetType().GetProperties();
                    foreach (var storeProperty in storeProperties)
                    {
                        var currentAmount = creepsDTO.Store.ContainsKey(storeProperty.Name) ? creepsDTO.Store[storeProperty.Name] : 0;
                        var toBeAddedAmount = Convert.ToInt64(storeProperty.GetValue(creep.Store));
                        creepsDTO.Store[storeProperty.Name] = currentAmount + Convert.ToInt64(storeProperty.GetValue(creep.Store));
                    }
                }


                #region Damage
                if (actionLog.Attacked != null)
                {
                    creepsDTO.ActionLog.Attacked.Count += actionLog.Attacked.Count;
                    creepsDTO.ActionLog.Attacked.Damage += actionLog.Attacked.Damage;
                }
                if (actionLog.Attack != null)
                {
                    creepsDTO.ActionLog.Attack.Count += actionLog.Attack.Count;
                    creepsDTO.ActionLog.Attack.Damage += actionLog.Attack.Damage;
                }
                if (actionLog.RangedAttack != null)
                {
                    creepsDTO.ActionLog.RangedAttack.Count += actionLog.RangedAttack.Count;
                    creepsDTO.ActionLog.RangedAttack.Damage += actionLog.RangedAttack.Damage;
                }
                if (actionLog.RangedMassAttack != null)
                {
                    creepsDTO.ActionLog.RangedMassAttack.Count += actionLog.RangedMassAttack.Count;
                    creepsDTO.ActionLog.RangedMassAttack.Damage += actionLog.RangedMassAttack.Damage;
                }
                #endregion
                #region Heal
                if (actionLog.Heal != null)
                {
                    creepsDTO.ActionLog.Heal.Count += actionLog.Heal.Count;
                    creepsDTO.ActionLog.Heal.Heal += actionLog.Heal.Heal;
                }
                if (actionLog.Healed != null)
                {
                    creepsDTO.ActionLog.Healed.Count += actionLog.Healed.Count;
                    creepsDTO.ActionLog.Healed.Heal += actionLog.Healed.Heal;
                }
                if (actionLog.RangedHeal != null)
                {
                    creepsDTO.ActionLog.RangedHeal.Count += actionLog.RangedHeal.Count;
                    creepsDTO.ActionLog.RangedHeal.Heal += actionLog.RangedHeal.Heal;
                }
                #endregion
                #region Inflow
                if (actionLog.Harvest != null)
                {
                    creepsDTO.ActionLog.Harvest.Count += actionLog.Harvest.Count;
                    creepsDTO.ActionLog.Harvest.Inflow += actionLog.Harvest.Inflow;
                }
                #endregion
                #region Outflow
                if (actionLog.Repair != null)
                {
                    creepsDTO.ActionLog.Repair.Count += actionLog.Repair.Count;
                    creepsDTO.ActionLog.Repair.Outlflow += actionLog.Repair.Outlflow;
                    creepsDTO.ActionLog.Repair.Effect += actionLog.Repair.Effect;
                }
                if (actionLog.Build != null)
                {
                    creepsDTO.ActionLog.Build.Count += actionLog.Build.Count;
                    creepsDTO.ActionLog.Build.Outlflow += actionLog.Build.Outlflow;
                    creepsDTO.ActionLog.Build.Effect += actionLog.Build.Effect;
                }
                if (actionLog.UpgradeController != null)
                {
                    creepsDTO.ActionLog.UpgradeController.Count += actionLog.UpgradeController.Count;
                    creepsDTO.ActionLog.UpgradeController.Outlflow += actionLog.UpgradeController.Outlflow;
                    creepsDTO.ActionLog.UpgradeController.Effect += actionLog.UpgradeController.Effect;
                }
                #endregion
                #region Generic
                creepsDTO.ActionLog.Move.Count += creep._oldFatigue == 0 ? 1 : 0;
                if (actionLog.Say != null)
                {
                    creepsDTO.ActionLog.Say.Count += actionLog.Say.Count;
                }
                if (actionLog.ReserveController != null)
                {
                    creepsDTO.ActionLog.ReserveController.Count += actionLog.ReserveController.Count;
                }
                if (actionLog.Produce != null)
                {
                    creepsDTO.ActionLog.Produce.Count += actionLog.Produce.Count;
                }
                if (actionLog.TransferEnergy != null)
                {
                    creepsDTO.ActionLog.TransferEnergy.Count += actionLog.TransferEnergy.Count;
                }
                if (actionLog.AttackController != null)
                {
                    creepsDTO.ActionLog.AttackController.Count += actionLog.AttackController.Count;
                }
                if (actionLog.RunReaction != null)
                {
                    creepsDTO.ActionLog.RunReaction.Count += actionLog.RunReaction.Count;
                }
                if (actionLog.ReverseReaction != null)
                {
                    creepsDTO.ActionLog.ReverseReaction.Count += actionLog.ReverseReaction.Count;
                }
                if (actionLog.Spawned != null)
                {
                    creepsDTO.ActionLog.Spawned.Count += actionLog.Spawned.Count;
                }
                if (actionLog.Power != null)
                {
                    creepsDTO.ActionLog.Power.Count += actionLog.Power.Count;
                }
                #endregion

            }
            return creepsDTO;
        }
        public static Dictionary<string, long> ConvertBody(BodyPart[] body)
        {
            var bodyDict = new Dictionary<string, long>();
            foreach (var bodyPart in body)
            {
                if (bodyDict.ContainsKey(bodyPart.Type))
                    bodyDict[bodyPart.Type]++;
                else
                    bodyDict[bodyPart.Type] = 1;
            }
            return bodyDict;
        }
        public static Dictionary<string, double> ComputeExtraIntentPower(BodyPart[] body, Dictionary<string, long> countByPart)
        {
            long moveCount = countByPart.ContainsKey("move") ? countByPart["move"] : 0;
            long workCount = countByPart.ContainsKey("work") ? countByPart["work"] : 0;
            long carryCount = countByPart.ContainsKey("carry") ? countByPart["carry"] : 0;
            long attackCount = countByPart.ContainsKey("attack") ? countByPart["attack"] : 0;
            long rangedAttackCount = countByPart.ContainsKey("ranged_attack") ? countByPart["ranged_attack"] : 0;
            long toughCount = countByPart.ContainsKey("tough") ? countByPart["tough"] : 0;
            long healCount = countByPart.ContainsKey("heal") ? countByPart["heal"] : 0;
            long claimCount = countByPart.ContainsKey("claim") ? countByPart["claim"] : 0;

            var intentMap = new Dictionary<string, double>()
            {
                {"harvest", 0 },
                {"build", 0 },
                {"repair",0 },
                {"dismantle", 0 },
                {"upgradeController",0 },
                {"attack",0 },
                {"rangedAttack",0},
                {"rangedMassAttack",0 },
                {"heal",0 },
                {"rangedHeal",0 },
            };

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
                                intentMap["harvest"] += 2;
                                break;
                            case "UHO2":
                                intentMap["harvest"] += 4;
                                break;
                            case "XUHO2":
                                intentMap["harvest"] += 6;
                                break;
                            case "LH":
                                intentMap["build"] += 0.5;
                                intentMap["repair"] += 0.5;
                                break;
                                case "LH2O":
                                intentMap["build"] += 0.8;
                                intentMap["repair"] += 0.8;
                                break;
                            case "XLH2O":
                                intentMap["build"] += 1;
                                intentMap["repair"] += 1;
                                break;
                            case "ZH":
                                intentMap["dismantle"] += 0.5;
                                break;
                            case "ZH2O":
                                intentMap["dismantle"] += 2;
                                break;
                            case "XZH2O":
                                intentMap["dismantle"] += 3;
                                break;
                            case "GH":
                                intentMap["upgradeController"] += 0.5;
                                break;
                            case "GH2O":
                                intentMap["upgradeController"] += 0.8;
                                break;
                            case "XGH2O":
                                intentMap["upgradeController"] += 1;
                                break;
                            default:
                                break;
                        }
                        break;
                    case "attack":
                        switch (bodyPart.Boost)
                        {
                            case "UH":
                                intentMap["attack"] += 30;
                                break;
                            case "UH2O":
                                intentMap["attack"] += 60;
                                break;
                            case "XUH2O":
                                intentMap["attack"] += 90;
                                break;
                            default:
                                break;
                        }
                        break;
                    case "ranged_attack":
                        switch (bodyPart.Boost)
                        {
                            case "KO":
                                intentMap["rangedAttack"] += 10;
                                intentMap["rangedMassAttack"] += 10;
                                break;
                            case "KHO2":
                                intentMap["rangedAttack"] += 20;
                                intentMap["rangedMassAttack"] += 20;
                                break;
                            case "XKHO2":
                                intentMap["rangedAttack"] += 30;
                                intentMap["rangedMassAttack"] += 30;
                                break;
                            default:
                                break;
                        }
                        break;
                    case "heal":
                        switch (bodyPart.Boost)
                        {
                            case "LO":
                                intentMap["heal"] += 12;
                                intentMap["rangedHeal"] += 4;
                                break;
                            case "LHO2":
                                intentMap["heal"] += 24;
                                intentMap["rangedHeal"] += 8;
                                break;
                            case "XLHO2":
                                intentMap["heal"] += 36;
                                intentMap["rangedHeal"] += 12;
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
        public static ActionLogDTO ConvertActiongLog(ActionLog actionLog, Dictionary<string, long> body, Dictionary<string, double> intentPowerMap)
        {
            if (actionLog == null)
                return new ActionLogDTO();

            var convertedActionLog = new ActionLogDTO();
            long moveCount = body.ContainsKey("move") ? body["move"] : 0;
            long workCount = body.ContainsKey("work") ? body["work"] : 0;
            long carryCount = body.ContainsKey("carry") ? body["carry"] : 0;
            long attackCount = body.ContainsKey("attack") ? body["attack"] : 0;
            long rangedAttackCount = body.ContainsKey("ranged_attack") ? body["ranged_attack"] : 0;
            long toughCount = body.ContainsKey("tough") ? body["tough"] : 0;
            long healCount = body.ContainsKey("heal") ? body["heal"] : 0;
            long claimCount = body.ContainsKey("claim") ? body["claim"] : 0;


            #region Damage
            if (actionLog.Attacked != null)
            {
                convertedActionLog.Attacked = new DamageActionDTO
                {
                    Count = 1,
                    Damage = Convert.ToInt64(Math.Round(attackCount * 30 + intentPowerMap["attack"] * 30))
                };
            }
            if (actionLog.Attack != null)
            {
                convertedActionLog.Attack = new DamageActionDTO
                {
                    Count = 1,
                    Damage = Convert.ToInt64(Math.Round(attackCount * 30 + intentPowerMap["attack"] * 30))
                };
            }
            if (actionLog.RangedAttack != null)
            {
                convertedActionLog.RangedAttack = new DamageActionDTO
                {
                    Count = 1,
                    Damage = Convert.ToInt64(Math.Round(attackCount * 10 + intentPowerMap["rangedAttack"] * 10))
                };
            }
            if (actionLog.RangedMassAttack != null)
            {
                convertedActionLog.RangedMassAttack = new DamageActionDTO
                {
                    Count = 1,
                    Damage = Convert.ToInt64(Math.Round(attackCount * 4 + intentPowerMap["rangedMassAttack"] * 4))
                };
            }
            #endregion
            #region Heal
            if (actionLog.Heal != null)
            {
                convertedActionLog.Heal = new HealActionDTO
                {
                    Count = 1,
                    Heal = Convert.ToInt64(Math.Round(healCount * 12 + intentPowerMap["heal"] * 12))
                };
            }
            if (actionLog.Healed != null)
            {
                convertedActionLog.Healed = new HealActionDTO
                {
                    Count = 1,
                    Heal = Convert.ToInt64(Math.Round(healCount * 12 + intentPowerMap["heal"] * 12))
                };
            }
            if (actionLog.RangedHeal != null)
            {
                convertedActionLog.RangedHeal = new HealActionDTO
                {
                    Count = 1,
                    Heal = Convert.ToInt64(Math.Round(healCount * 4 + intentPowerMap["rangedHeal"] * 4))
                };
            }
            #endregion
            #region Inflow
            if (actionLog.Harvest != null)
            {
                convertedActionLog.Harvest = new InflowActionDTO
                {
                    Count = 1,
                    Inflow = Convert.ToInt64(Math.Round(workCount * 2 + intentPowerMap["harvest"] * 2))
                };
            }
            #endregion
            #region Outflow
            if (actionLog.Repair != null)
            {
                convertedActionLog.Repair = new OutlflowActionDTO
                {
                    Count = 1,
                    Outlflow = workCount * 1,
                    Effect = Convert.ToInt64(Math.Round(workCount * 100 + intentPowerMap["repair"] * 100))
                };
            }
            if (actionLog.Build != null)
            {
                convertedActionLog.Build = new OutlflowActionDTO
                {
                    Count = 1,
                    Outlflow = workCount * 1,
                    Effect = Convert.ToInt64(Math.Round(workCount * 5 + intentPowerMap["build"] * 5))
                };
            }
            if (actionLog.UpgradeController != null)
            {
                convertedActionLog.UpgradeController = new OutlflowActionDTO
                {
                    Count = 1,
                    Outlflow = workCount * 1,
                    Effect = Convert.ToInt64(Math.Round(workCount + intentPowerMap["upgradeController"]))
                };
            }
            #endregion
            #region Generic
            if (actionLog.Say != null)
            {
                convertedActionLog.Say = new GenericActionDTO
                {
                    Count = 1,
                };
            }
            if (actionLog.ReserveController != null)
            {
                convertedActionLog.ReserveController = new GenericActionDTO
                {
                    Count = 1,
                };
            }
            if (actionLog.Produce != null)
            {
                convertedActionLog.Produce = new GenericActionDTO
                {
                    Count = 1,
                };
            }
            if (actionLog.TransferEnergy != null)
            {
                convertedActionLog.TransferEnergy = new GenericActionDTO
                {
                    Count = 1,
                };
            }
            if (actionLog.AttackController != null)
            {
                convertedActionLog.AttackController = new GenericActionDTO
                {
                    Count = 1,
                };
            }
            if (actionLog.RunReaction != null)
            {
                convertedActionLog.RunReaction = new GenericActionDTO
                {
                    Count = 1,
                };
            }
            if (actionLog.ReverseReaction != null)
            {
                convertedActionLog.ReverseReaction = new GenericActionDTO
                {
                    Count = 1,
                };
            }
            if (actionLog.Spawned != null)
            {
                convertedActionLog.Spawned = new GenericActionDTO
                {
                    Count = 1,
                };
            }
            if (actionLog.Power != null)
            {
                convertedActionLog.Power = new GenericActionDTO
                {
                    Count = 1,
                };
            }
            #endregion

            return convertedActionLog;
        }
    }
}
