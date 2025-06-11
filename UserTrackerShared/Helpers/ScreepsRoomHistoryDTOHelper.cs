using UserTrackerShared.Models;

namespace UserTrackerShared.Helpers
{
    public static class ScreepsRoomHistoryDtoHelper
    {
        private static void UpdateStore(Store currentStore, Store store)
        {
            currentStore.energy = (currentStore.energy ?? 0) + (store.energy / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.power = (currentStore.power ?? 0) + (store.power / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.H = (currentStore.H ?? 0) + (store.H / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.O = (currentStore.O ?? 0) + (store.O / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.U = (currentStore.U ?? 0) + (store.U / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.L = (currentStore.L ?? 0) + (store.L / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.K = (currentStore.K ?? 0) + (store.K / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.Z = (currentStore.Z ?? 0) + (store.Z / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.X = (currentStore.X ?? 0) + (store.X / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.G = (currentStore.G ?? 0) + (store.G / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.silicon = (currentStore.silicon ?? 0) + (store.silicon / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.metal = (currentStore.metal ?? 0) + (store.metal / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.biomass = (currentStore.biomass ?? 0) + (store.biomass / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.mist = (currentStore.mist ?? 0) + (store.mist / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.OH = (currentStore.OH ?? 0) + (store.OH / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.ZK = (currentStore.ZK ?? 0) + (store.ZK / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.UL = (currentStore.UL ?? 0) + (store.UL / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.UH = (currentStore.UH ?? 0) + (store.UH / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.UO = (currentStore.UO ?? 0) + (store.UO / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.KH = (currentStore.KH ?? 0) + (store.KH / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.KO = (currentStore.KO ?? 0) + (store.KO / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.LH = (currentStore.LH ?? 0) + (store.LH / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.LO = (currentStore.LO ?? 0) + (store.LO / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.ZH = (currentStore.ZH ?? 0) + (store.ZH / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.ZO = (currentStore.ZO ?? 0) + (store.ZO / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.GH = (currentStore.GH ?? 0) + (store.GH / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.GO = (currentStore.GO ?? 0) + (store.GO / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.UH2O = (currentStore.UH2O ?? 0) + (store.UH2O / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.UHO2 = (currentStore.UHO2 ?? 0) + (store.UHO2 / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.KH2O = (currentStore.KH2O ?? 0) + (store.KH2O / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.KHO2 = (currentStore.KHO2 ?? 0) + (store.KHO2 / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.LH2O = (currentStore.LH2O ?? 0) + (store.LH2O / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.LHO2 = (currentStore.LHO2 ?? 0) + (store.LHO2 / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.ZH2O = (currentStore.ZH2O ?? 0) + (store.ZH2O / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.ZHO2 = (currentStore.ZHO2 ?? 0) + (store.ZHO2 / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.GH2O = (currentStore.GH2O ?? 0) + (store.GH2O / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.GHO2 = (currentStore.GHO2 ?? 0) + (store.GHO2 / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.XUH2O = (currentStore.XUH2O ?? 0) + (store.XUH2O / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.XUHO2 = (currentStore.XUHO2 ?? 0) + (store.XUHO2 / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.XKH2O = (currentStore.XKH2O ?? 0) + (store.XKH2O / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.XKHO2 = (currentStore.XKHO2 ?? 0) + (store.XKHO2 / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.XLH2O = (currentStore.XLH2O ?? 0) + (store.XLH2O / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.XLHO2 = (currentStore.XLHO2 ?? 0) + (store.XLHO2 / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.XZH2O = (currentStore.XZH2O ?? 0) + (store.XZH2O / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.XZHO2 = (currentStore.XZHO2 ?? 0) + (store.XZHO2 / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.XGH2O = (currentStore.XGH2O ?? 0) + (store.XGH2O / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.XGHO2 = (currentStore.XGHO2 ?? 0) + (store.XGHO2 / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.ops = (currentStore.ops ?? 0) + (store.ops / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.utrium_bar = (currentStore.utrium_bar ?? 0) + (store.utrium_bar / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.lemergium_bar = (currentStore.lemergium_bar ?? 0) + (store.lemergium_bar / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.zynthium_bar = (currentStore.zynthium_bar ?? 0) + (store.zynthium_bar / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.keanium_bar = (currentStore.keanium_bar ?? 0) + (store.keanium_bar / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.ghodium_melt = (currentStore.ghodium_melt ?? 0) + (store.ghodium_melt / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.oxidant = (currentStore.oxidant ?? 0) + (store.oxidant / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.reductant = (currentStore.reductant ?? 0) + (store.reductant / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.purifier = (currentStore.purifier ?? 0) + (store.purifier / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.battery = (currentStore.battery ?? 0) + (store.battery / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.composite = (currentStore.composite ?? 0) + (store.composite / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.crystal = (currentStore.crystal ?? 0) + (store.crystal / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.liquid = (currentStore.liquid ?? 0) + (store.liquid / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.wire = (currentStore.wire ?? 0) + (store.wire / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.Switch = (currentStore.Switch ?? 0) + (store.Switch / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.transistor = (currentStore.transistor ?? 0) + (store.transistor / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.microchip = (currentStore.microchip ?? 0) + (store.microchip / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.circuit = (currentStore.circuit ?? 0) + (store.circuit / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.device = (currentStore.device ?? 0) + (store.device / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.cell = (currentStore.cell ?? 0) + (store.cell / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.phlegm = (currentStore.phlegm ?? 0) + (store.phlegm / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.tissue = (currentStore.tissue ?? 0) + (store.tissue / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.muscle = (currentStore.muscle ?? 0) + (store.muscle / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.organoid = (currentStore.organoid ?? 0) + (store.organoid / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.organism = (currentStore.organism ?? 0) + (store.organism / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.alloy = (currentStore.alloy ?? 0) + (store.alloy / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.tube = (currentStore.tube ?? 0) + (store.tube / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.fixtures = (currentStore.fixtures ?? 0) + (store.fixtures / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.frame = (currentStore.frame ?? 0) + (store.frame / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.hydraulics = (currentStore.hydraulics ?? 0) + (store.hydraulics / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.machine = (currentStore.machine ?? 0) + (store.machine / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.condensate = (currentStore.condensate ?? 0) + (store.condensate / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.concentrate = (currentStore.concentrate ?? 0) + (store.concentrate / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.extract = (currentStore.extract ?? 0) + (store.extract / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.spirit = (currentStore.spirit ?? 0) + (store.spirit / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.emanation = (currentStore.emanation ?? 0) + (store.emanation / ConfigSettingsState.TicksInFile ?? 0);
            currentStore.essence = (currentStore.essence ?? 0) + (store.essence / ConfigSettingsState.TicksInFile ?? 0);
        }
        public static StructuresDto ConvertStructures(Structures structures, StructuresDto structuresDto)
        {
            if (structures.Controller != null)
            {
                structuresDto.Controller.Count += 1m / ConfigSettingsState.TicksInFile;
                structuresDto.Controller.Level += structures.Controller.Level / ConfigSettingsState.TicksInFile;
                structuresDto.Controller.Progress += structures.Controller.Progress / ConfigSettingsState.TicksInFile;
                structuresDto.Controller.ProgressTotal += structures.Controller.ProgressTotal / ConfigSettingsState.TicksInFile;
                structuresDto.Controller.UserId = structures.Controller.User;
                structuresDto.Controller.ReservationUserId = structures.Controller.Reservation?.User ?? "";
            }
            if (structures.Mineral != null)
            {
                structuresDto.Mineral.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            if (structures.Deposits != null)
            {
                foreach (var deposit in structures.Deposits.Select(x => x.Value))
                {
                    structuresDto.Deposit.Count += 1m / ConfigSettingsState.TicksInFile;
                }
            }
            foreach (var wall in structures.Walls)
            {
                structuresDto.Wall.Count += 1m / ConfigSettingsState.TicksInFile;
                structuresDto.Wall.Hits += wall.Value.Hits / ConfigSettingsState.TicksInFile;
            }
            foreach (var constructionSite in structures.ConstructionSites.Select(x => x.Value))
            {
                structuresDto.ConstructionSite.Count += 1m / ConfigSettingsState.TicksInFile;
                structuresDto.ConstructionSite.Progress += constructionSite.Progress / ConfigSettingsState.TicksInFile;
                structuresDto.ConstructionSite.ProgressTotal += constructionSite.ProgressTotal / ConfigSettingsState.TicksInFile;

                var typeBeingBuild = constructionSite.StructureType;
                if (!structuresDto.ConstructionSite.TypesBuilding.TryGetValue(typeBeingBuild, out var current))
                {
                    current = 0;
                }
                structuresDto.ConstructionSite.TypesBuilding[typeBeingBuild] = current + 1 / ConfigSettingsState.TicksInFile;
            }
            foreach (var container in structures.Containers.Select(x => x.Value))
            {
                structuresDto.Container.Count += 1m / ConfigSettingsState.TicksInFile;

                if (container.Store != null)
                {
                    UpdateStore(structuresDto.Container.Store, container.Store);
                }
            }
            foreach (var extension in structures.Extensions.Select(x => x.Value))
            {
                structuresDto.Extension.Count += 1m / ConfigSettingsState.TicksInFile;
                structuresDto.Extension.Energy += extension.Store?.energy / ConfigSettingsState.TicksInFile ?? 0;
                structuresDto.Extension.EnergyCapacity += extension.StoreCapacityResource?.energy / ConfigSettingsState.TicksInFile ?? 0;
            }
            foreach (var extractor in structures.Extractors.Select(x => x.Value))
            {
                structuresDto.Extractor.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            foreach (var factory in structures.Factories.Select(x => x.Value))
            {
                structuresDto.Factory.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            foreach (var invderCore in structures.InvaderCores.Select(x => x.Value))
            {
                structuresDto.InvaderCore.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            foreach (var invderCore in structures.InvaderCores.Select(x => x.Value))
            {
                structuresDto.InvaderCore.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            foreach (var keeperLair in structures.KeeperLairs.Select(x => x.Value))
            {
                structuresDto.KeeperLair.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            foreach (var lab in structures.Labs.Select(x => x.Value))
            {
                structuresDto.Lab.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            foreach (var link in structures.Links.Select(x => x.Value))
            {
                structuresDto.Link.Count += 1m / ConfigSettingsState.TicksInFile;
                structuresDto.Link.Energy += link.Store?.energy / ConfigSettingsState.TicksInFile ?? 0;
                structuresDto.Link.EnergyCapacity += link.StoreCapacityResource?.energy / ConfigSettingsState.TicksInFile ?? 0;
            }
            foreach (var observer in structures.Observers.Select(x => x.Value))
            {
                structuresDto.Observer.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            foreach (var portal in structures.Portals.Select(x => x.Value))
            {
                structuresDto.Portal.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            foreach (var powerBank in structures.PowerBanks.Select(x => x.Value))
            {
                structuresDto.PowerBank.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            foreach (var powerSpawns in structures.PowerSpawns.Select(x => x.Value))
            {
                structuresDto.PowerSpawn.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            foreach (var rampart in structures.Ramparts.Select(x => x.Value))
            {
                structuresDto.Rampart.Count += 1m / ConfigSettingsState.TicksInFile;
                structuresDto.Rampart.Hits += rampart.Hits / ConfigSettingsState.TicksInFile;
            }
            foreach (var road in structures.Roads.Select(x => x.Value))
            {
                structuresDto.Road.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            foreach (var ruin in structures.Ruins.Select(x => x.Value))
            {
                structuresDto.Ruin.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            foreach (var rampart in structures.Ramparts.Select(x => x.Value))
            {
                structuresDto.Rampart.Count += 1m / ConfigSettingsState.TicksInFile;
                structuresDto.Rampart.Hits += rampart.Hits / ConfigSettingsState.TicksInFile;
            }
            foreach (var source in structures.Sources.Select(x => x.Value))
            {
                structuresDto.Source.Count += 1m / ConfigSettingsState.TicksInFile;
                structuresDto.Source.Energy += source.Energy / ConfigSettingsState.TicksInFile;
                structuresDto.Source.EnergyCapacity += source.EnergyCapacity / ConfigSettingsState.TicksInFile;
            }
            foreach (var rampart in structures.Ramparts.Select(x => x.Value))
            {
                structuresDto.Rampart.Count += 1m / ConfigSettingsState.TicksInFile;
                structuresDto.Rampart.Hits += rampart.Hits / ConfigSettingsState.TicksInFile;
            }
            foreach (var spawn in structures.Spawns.Select(x => x.Value))
            {
                structuresDto.Spawn.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            foreach (var storage in structures.Storages.Select(x => x.Value))
            {
                structuresDto.Storage.Count += 1m / ConfigSettingsState.TicksInFile;

                if (storage.Store != null)
                {
                    UpdateStore(structuresDto.Storage.Store, storage.Store);
                }
            }
            foreach (var terminal in structures.Terminals)
            {
                structuresDto.Terminal.Count += 1m / ConfigSettingsState.TicksInFile;

                if (terminal.Value.Store != null)
                {
                    UpdateStore(structuresDto.Terminal.Store, terminal.Value.Store);
                }
            }
            foreach (var tombstone in structures.Tombstones.Select(x => x.Value))
            {
                structuresDto.Tombstone.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            foreach (var tower in structures.Towers.Select(x=>x.Value))
            {
                structuresDto.Tower.Count += 1m / ConfigSettingsState.TicksInFile;
                structuresDto.Tower.Energy += tower.Store?.energy / ConfigSettingsState.TicksInFile ?? 0;
            }
            foreach (var nuker in structures.Nukers.Select(x => x.Value))
            {
                structuresDto.Nuker.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            foreach (var nuke in structures.Nukes.Select(x => x.Value))
            {
                structuresDto.Nuke.Count += 1m / ConfigSettingsState.TicksInFile;
            }

            return structuresDto;
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
                creepsDto.Count += 1m / ConfigSettingsState.TicksInFile;
                ConvertBody(creep.Body, creepsDtoBodyPart);
                ComputeExtraIntentPower(creep.Body, creepsDtoBodyPart, intentMap);
                ConvertActiongLog(creep.ActionLog, creepsDto.ActionLog, creepsDtoBodyPart, intentMap, creep._oldFatigue);
                if (creep.Store != null)
                {
                    UpdateStore(creepsDto.Store, creep.Store);
                }

                creepsDto.BodyParts.Move += creepsDtoBodyPart.Move / ConfigSettingsState.TicksInFile;
                creepsDto.BodyParts.Work += creepsDtoBodyPart.Work / ConfigSettingsState.TicksInFile;
                creepsDto.BodyParts.Carry += creepsDtoBodyPart.Carry / ConfigSettingsState.TicksInFile;
                creepsDto.BodyParts.Attack += creepsDtoBodyPart.Attack / ConfigSettingsState.TicksInFile;
                creepsDto.BodyParts.RangedAttack += creepsDtoBodyPart.RangedAttack / ConfigSettingsState.TicksInFile;
                creepsDto.BodyParts.Tough += creepsDtoBodyPart.Tough / ConfigSettingsState.TicksInFile;
                creepsDto.BodyParts.Heal += creepsDtoBodyPart.Heal / ConfigSettingsState.TicksInFile;
                creepsDto.BodyParts.Claim += creepsDtoBodyPart.Claim / ConfigSettingsState.TicksInFile;

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
                actionLogDto.Attacked.Count += 1m / ConfigSettingsState.TicksInFile;
                actionLogDto.Attacked.Damage += Convert.ToInt64(Math.Round(attackCount * 30 + intentPowerMap.Attack * 30)) / ConfigSettingsState.TicksInFile;
            }
            if (actionLog.Attack != null)
            {
                actionLogDto.Attack.Count += 1m / ConfigSettingsState.TicksInFile;
                actionLogDto.Attack.Damage += Convert.ToInt64(Math.Round(attackCount * 30 + intentPowerMap.Attack * 30)) / ConfigSettingsState.TicksInFile;
            }
            if (actionLog.RangedAttack != null)
            {
                actionLogDto.Attacked.Count += 1m / ConfigSettingsState.TicksInFile;
                actionLogDto.RangedAttack.Damage += Convert.ToInt64(Math.Round(attackCount * 10 + intentPowerMap.RangedAttack * 10)) / ConfigSettingsState.TicksInFile;
            }
            if (actionLog.RangedMassAttack != null)
            {
                actionLogDto.RangedMassAttack.Count += 1m / ConfigSettingsState.TicksInFile;
                actionLogDto.RangedMassAttack.Damage += Convert.ToInt64(Math.Round(attackCount * 4 + intentPowerMap.RangedAttack * 4)) / ConfigSettingsState.TicksInFile;
            }
            #endregion
            #region Heal
            if (actionLog.Heal != null)
            {
                actionLogDto.Heal.Count += 1m / ConfigSettingsState.TicksInFile;
                actionLogDto.Heal.Heal += Convert.ToInt64(Math.Round(healCount * 12 + intentPowerMap.Heal * 12)) / ConfigSettingsState.TicksInFile;
            }
            if (actionLog.Healed != null)
            {
                actionLogDto.Healed.Count += 1m / ConfigSettingsState.TicksInFile;
                actionLogDto.Healed.Heal += Convert.ToInt64(Math.Round(healCount * 12 + intentPowerMap.Heal * 12)) / ConfigSettingsState.TicksInFile;
            }
            if (actionLog.RangedHeal != null)
            {
                actionLogDto.RangedHeal.Count += 1m / ConfigSettingsState.TicksInFile;
                actionLogDto.RangedHeal.Heal += Convert.ToInt64(Math.Round(healCount * 4 + intentPowerMap.RangedHeal * 4)) / ConfigSettingsState.TicksInFile;
            }
            #endregion
            #region Inflow
            if (actionLog.Harvest != null)
            {
                actionLogDto.Harvest.Count += 1m / ConfigSettingsState.TicksInFile;
                actionLogDto.Harvest.Inflow += Convert.ToInt64(Math.Round(workCount * 2 + intentPowerMap.Harvest * 2));
            }
            #endregion
            #region Outflow
            if (actionLog.Repair != null)
            {
                actionLogDto.Repair.Count += 1m / ConfigSettingsState.TicksInFile;
                actionLogDto.Repair.Outflow += workCount / ConfigSettingsState.TicksInFile;
                actionLogDto.Repair.Effect += Convert.ToInt64(Math.Round(workCount * 100 + intentPowerMap.Repair * 100)) / ConfigSettingsState.TicksInFile;
            }
            if (actionLog.Build != null)
            {
                actionLogDto.Build.Count += 1m / ConfigSettingsState.TicksInFile;
                actionLogDto.Build.Outflow += workCount / ConfigSettingsState.TicksInFile;
                actionLogDto.Build.Effect += Convert.ToInt64(Math.Round(workCount * 5 + intentPowerMap.Build * 5)) / ConfigSettingsState.TicksInFile;
            }
            if (actionLog.UpgradeController != null)
            {
                actionLogDto.UpgradeController.Count += 1m / ConfigSettingsState.TicksInFile;
                actionLogDto.UpgradeController.Outflow += workCount / ConfigSettingsState.TicksInFile;
                actionLogDto.UpgradeController.Effect += Convert.ToInt64(Math.Round(workCount + intentPowerMap.UpgradeController)) / ConfigSettingsState.TicksInFile;
            }
            #endregion
            #region Generic
            actionLogDto.Move.Count += creep_oldFatigue == 0 ? 1m : 0m / ConfigSettingsState.TicksInFile;
            if (actionLog.Say != null)
            {
                actionLogDto.Say.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            if (actionLog.ReserveController != null)
            {
                actionLogDto.ReserveController.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            if (actionLog.Produce != null)
            {
                actionLogDto.Produce.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            if (actionLog.TransferEnergy != null)
            {
                actionLogDto.TransferEnergy.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            if (actionLog.AttackController != null)
            {
                actionLogDto.AttackController.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            if (actionLog.RunReaction != null)
            {
                actionLogDto.RunReaction.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            if (actionLog.ReverseReaction != null)
            {
                actionLogDto.ReverseReaction.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            if (actionLog.Spawned != null)
            {
                actionLogDto.Spawned.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            if (actionLog.Power != null)
            {
                actionLogDto.Power.Count += 1m / ConfigSettingsState.TicksInFile;
            }
            #endregion
        }
    }
}
