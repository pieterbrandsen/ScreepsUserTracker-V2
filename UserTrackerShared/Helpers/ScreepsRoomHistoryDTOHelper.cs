using UserTrackerShared.Models;
using UserTrackerShared.States;

namespace UserTrackerShared.Helpers
{
    public static class ScreepsRoomHistoryDtoHelper
    {
        private static void UpdateStore(Store currentStore, Store store)
        {
            currentStore.energy = (currentStore.energy ?? 0) + (store.energy / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.power = (currentStore.power ?? 0) + (store.power / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.H = (currentStore.H ?? 0) + (store.H / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.O = (currentStore.O ?? 0) + (store.O / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.U = (currentStore.U ?? 0) + (store.U / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.L = (currentStore.L ?? 0) + (store.L / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.K = (currentStore.K ?? 0) + (store.K / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.Z = (currentStore.Z ?? 0) + (store.Z / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.X = (currentStore.X ?? 0) + (store.X / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.G = (currentStore.G ?? 0) + (store.G / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.silicon = (currentStore.silicon ?? 0) + (store.silicon / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.metal = (currentStore.metal ?? 0) + (store.metal / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.biomass = (currentStore.biomass ?? 0) + (store.biomass / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.mist = (currentStore.mist ?? 0) + (store.mist / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.OH = (currentStore.OH ?? 0) + (store.OH / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.ZK = (currentStore.ZK ?? 0) + (store.ZK / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.UL = (currentStore.UL ?? 0) + (store.UL / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.UH = (currentStore.UH ?? 0) + (store.UH / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.UO = (currentStore.UO ?? 0) + (store.UO / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.KH = (currentStore.KH ?? 0) + (store.KH / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.KO = (currentStore.KO ?? 0) + (store.KO / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.LH = (currentStore.LH ?? 0) + (store.LH / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.LO = (currentStore.LO ?? 0) + (store.LO / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.ZH = (currentStore.ZH ?? 0) + (store.ZH / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.ZO = (currentStore.ZO ?? 0) + (store.ZO / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.GH = (currentStore.GH ?? 0) + (store.GH / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.GO = (currentStore.GO ?? 0) + (store.GO / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.UH2O = (currentStore.UH2O ?? 0) + (store.UH2O / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.UHO2 = (currentStore.UHO2 ?? 0) + (store.UHO2 / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.KH2O = (currentStore.KH2O ?? 0) + (store.KH2O / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.KHO2 = (currentStore.KHO2 ?? 0) + (store.KHO2 / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.LH2O = (currentStore.LH2O ?? 0) + (store.LH2O / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.LHO2 = (currentStore.LHO2 ?? 0) + (store.LHO2 / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.ZH2O = (currentStore.ZH2O ?? 0) + (store.ZH2O / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.ZHO2 = (currentStore.ZHO2 ?? 0) + (store.ZHO2 / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.GH2O = (currentStore.GH2O ?? 0) + (store.GH2O / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.GHO2 = (currentStore.GHO2 ?? 0) + (store.GHO2 / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.XUH2O = (currentStore.XUH2O ?? 0) + (store.XUH2O / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.XUHO2 = (currentStore.XUHO2 ?? 0) + (store.XUHO2 / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.XKH2O = (currentStore.XKH2O ?? 0) + (store.XKH2O / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.XKHO2 = (currentStore.XKHO2 ?? 0) + (store.XKHO2 / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.XLH2O = (currentStore.XLH2O ?? 0) + (store.XLH2O / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.XLHO2 = (currentStore.XLHO2 ?? 0) + (store.XLHO2 / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.XZH2O = (currentStore.XZH2O ?? 0) + (store.XZH2O / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.XZHO2 = (currentStore.XZHO2 ?? 0) + (store.XZHO2 / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.XGH2O = (currentStore.XGH2O ?? 0) + (store.XGH2O / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.XGHO2 = (currentStore.XGHO2 ?? 0) + (store.XGHO2 / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.ops = (currentStore.ops ?? 0) + (store.ops / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.utrium_bar = (currentStore.utrium_bar ?? 0) + (store.utrium_bar / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.lemergium_bar = (currentStore.lemergium_bar ?? 0) + (store.lemergium_bar / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.zynthium_bar = (currentStore.zynthium_bar ?? 0) + (store.zynthium_bar / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.keanium_bar = (currentStore.keanium_bar ?? 0) + (store.keanium_bar / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.ghodium_melt = (currentStore.ghodium_melt ?? 0) + (store.ghodium_melt / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.oxidant = (currentStore.oxidant ?? 0) + (store.oxidant / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.reductant = (currentStore.reductant ?? 0) + (store.reductant / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.purifier = (currentStore.purifier ?? 0) + (store.purifier / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.battery = (currentStore.battery ?? 0) + (store.battery / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.composite = (currentStore.composite ?? 0) + (store.composite / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.crystal = (currentStore.crystal ?? 0) + (store.crystal / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.liquid = (currentStore.liquid ?? 0) + (store.liquid / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.wire = (currentStore.wire ?? 0) + (store.wire / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.Switch = (currentStore.Switch ?? 0) + (store.Switch / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.transistor = (currentStore.transistor ?? 0) + (store.transistor / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.microchip = (currentStore.microchip ?? 0) + (store.microchip / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.circuit = (currentStore.circuit ?? 0) + (store.circuit / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.device = (currentStore.device ?? 0) + (store.device / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.cell = (currentStore.cell ?? 0) + (store.cell / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.phlegm = (currentStore.phlegm ?? 0) + (store.phlegm / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.tissue = (currentStore.tissue ?? 0) + (store.tissue / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.muscle = (currentStore.muscle ?? 0) + (store.muscle / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.organoid = (currentStore.organoid ?? 0) + (store.organoid / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.organism = (currentStore.organism ?? 0) + (store.organism / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.alloy = (currentStore.alloy ?? 0) + (store.alloy / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.tube = (currentStore.tube ?? 0) + (store.tube / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.fixtures = (currentStore.fixtures ?? 0) + (store.fixtures / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.frame = (currentStore.frame ?? 0) + (store.frame / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.hydraulics = (currentStore.hydraulics ?? 0) + (store.hydraulics / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.machine = (currentStore.machine ?? 0) + (store.machine / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.condensate = (currentStore.condensate ?? 0) + (store.condensate / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.concentrate = (currentStore.concentrate ?? 0) + (store.concentrate / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.extract = (currentStore.extract ?? 0) + (store.extract / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.spirit = (currentStore.spirit ?? 0) + (store.spirit / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.emanation = (currentStore.emanation ?? 0) + (store.emanation / ConfigSettingsState.TicksInObject ?? 0);
            currentStore.essence = (currentStore.essence ?? 0) + (store.essence / ConfigSettingsState.TicksInObject ?? 0);
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
                    UpdateStore(structuresDto.Container.Store, container.Store);
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
                    UpdateStore(structuresDto.Storage.Store, storage.Store);
                }
            }
            foreach (var terminal in structures.Terminals.Select(x => x.Value))
            {
                structuresDto.Terminal.Count += 1m / ConfigSettingsState.TicksInObject;

                if (terminal.Store != null)
                {
                    UpdateStore(structuresDto.Terminal.Store, terminal.Store);
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
            if (string.IsNullOrEmpty(structuresDtoA.Controller.UserId))
                structuresDtoA.Controller.UserId = structuresDtoB.Controller.UserId;
            if (string.IsNullOrEmpty(structuresDtoA.Controller.ReservationUserId))
                structuresDtoA.Controller.ReservationUserId = structuresDtoB.Controller.ReservationUserId;

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
                if (!structuresDtoA.ConstructionSite.TypesBuilding.ContainsKey(kvp.Key))
                    structuresDtoA.ConstructionSite.TypesBuilding[kvp.Key] = 0;
                structuresDtoA.ConstructionSite.TypesBuilding[kvp.Key] += kvp.Value;
            }

            // Container
            structuresDtoA.Container.Count += structuresDtoB.Container.Count;
            UpdateStore(structuresDtoA.Container.Store, structuresDtoB.Container.Store);

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
            UpdateStore(structuresDtoA.Storage.Store, structuresDtoB.Storage.Store);

            // Terminal
            structuresDtoA.Terminal.Count += structuresDtoB.Terminal.Count;
            UpdateStore(structuresDtoA.Terminal.Store, structuresDtoB.Terminal.Store);

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
                    UpdateStore(creepsDto.Store, creep.Store);
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
            actionLogDto.Move.Count += creep_oldFatigue == 0 ? 1m : 0m / ConfigSettingsState.TicksInObject;
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
            a.energy = (a.energy ?? 0) + (b.energy ?? 0);
            a.power = (a.power ?? 0) + (b.power ?? 0);
            a.H = (a.H ?? 0) + (b.H ?? 0);
            a.O = (a.O ?? 0) + (b.O ?? 0);
            a.U = (a.U ?? 0) + (b.U ?? 0);
            a.L = (a.L ?? 0) + (b.L ?? 0);
            a.K = (a.K ?? 0) + (b.K ?? 0);
            a.Z = (a.Z ?? 0) + (b.Z ?? 0);
            a.X = (a.X ?? 0) + (b.X ?? 0);
            a.G = (a.G ?? 0) + (b.G ?? 0);
            a.silicon = (a.silicon ?? 0) + (b.silicon ?? 0);
            a.metal = (a.metal ?? 0) + (b.metal ?? 0);
            a.biomass = (a.biomass ?? 0) + (b.biomass ?? 0);
            a.mist = (a.mist ?? 0) + (b.mist ?? 0);
            a.OH = (a.OH ?? 0) + (b.OH ?? 0);
            a.ZK = (a.ZK ?? 0) + (b.ZK ?? 0);
            a.UL = (a.UL ?? 0) + (b.UL ?? 0);
            a.UH = (a.UH ?? 0) + (b.UH ?? 0);
            a.UO = (a.UO ?? 0) + (b.UO ?? 0);
            a.KH = (a.KH ?? 0) + (b.KH ?? 0);
            a.KO = (a.KO ?? 0) + (b.KO ?? 0);
            a.LH = (a.LH ?? 0) + (b.LH ?? 0);
            a.LO = (a.LO ?? 0) + (b.LO ?? 0);
            a.ZH = (a.ZH ?? 0) + (b.ZH ?? 0);
            a.ZO = (a.ZO ?? 0) + (b.ZO ?? 0);
            a.GH = (a.GH ?? 0) + (b.GH ?? 0);
            a.GO = (a.GO ?? 0) + (b.GO ?? 0);
            a.UH2O = (a.UH2O ?? 0) + (b.UH2O ?? 0);
            a.UHO2 = (a.UHO2 ?? 0) + (b.UHO2 ?? 0);
            a.KH2O = (a.KH2O ?? 0) + (b.KH2O ?? 0);
            a.KHO2 = (a.KHO2 ?? 0) + (b.KHO2 ?? 0);
            a.LH2O = (a.LH2O ?? 0) + (b.LH2O ?? 0);
            a.LHO2 = (a.LHO2 ?? 0) + (b.LHO2 ?? 0);
            a.ZH2O = (a.ZH2O ?? 0) + (b.ZH2O ?? 0);
            a.ZHO2 = (a.ZHO2 ?? 0) + (b.ZHO2 ?? 0);
            a.GH2O = (a.GH2O ?? 0) + (b.GH2O ?? 0);
            a.GHO2 = (a.GHO2 ?? 0) + (b.GHO2 ?? 0);
            a.XUH2O = (a.XUH2O ?? 0) + (b.XUH2O ?? 0);
            a.XUHO2 = (a.XUHO2 ?? 0) + (b.XUHO2 ?? 0);
            a.XKH2O = (a.XKH2O ?? 0) + (b.XKH2O ?? 0);
            a.XKHO2 = (a.XKHO2 ?? 0) + (b.XKHO2 ?? 0);
            a.XLH2O = (a.XLH2O ?? 0) + (b.XLH2O ?? 0);
            a.XLHO2 = (a.XLHO2 ?? 0) + (b.XLHO2 ?? 0);
            a.XZH2O = (a.XZH2O ?? 0) + (b.XZH2O ?? 0);
            a.XZHO2 = (a.XZHO2 ?? 0) + (b.XZHO2 ?? 0);
            a.XGH2O = (a.XGH2O ?? 0) + (b.XGH2O ?? 0);
            a.XGHO2 = (a.XGHO2 ?? 0) + (b.XGHO2 ?? 0);
            a.ops = (a.ops ?? 0) + (b.ops ?? 0);
            a.utrium_bar = (a.utrium_bar ?? 0) + (b.utrium_bar ?? 0);
            a.lemergium_bar = (a.lemergium_bar ?? 0) + (b.lemergium_bar ?? 0);
            a.zynthium_bar = (a.zynthium_bar ?? 0) + (b.zynthium_bar ?? 0);
            a.keanium_bar = (a.keanium_bar ?? 0) + (b.keanium_bar ?? 0);
            a.ghodium_melt = (a.ghodium_melt ?? 0) + (b.ghodium_melt ?? 0);
            a.oxidant = (a.oxidant ?? 0) + (b.oxidant ?? 0);
            a.reductant = (a.reductant ?? 0) + (b.reductant ?? 0);
            a.purifier = (a.purifier ?? 0) + (b.purifier ?? 0);
            a.battery = (a.battery ?? 0) + (b.battery ?? 0);
            a.composite = (a.composite ?? 0) + (b.composite ?? 0);
            a.crystal = (a.crystal ?? 0) + (b.crystal ?? 0);
            a.liquid = (a.liquid ?? 0) + (b.liquid ?? 0);
            a.wire = (a.wire ?? 0) + (b.wire ?? 0);
            a.Switch = (a.Switch ?? 0) + (b.Switch ?? 0);
            a.transistor = (a.transistor ?? 0) + (b.transistor ?? 0);
            a.microchip = (a.microchip ?? 0) + (b.microchip ?? 0);
            a.circuit = (a.circuit ?? 0) + (b.circuit ?? 0);
            a.device = (a.device ?? 0) + (b.device ?? 0);
            a.cell = (a.cell ?? 0) + (b.cell ?? 0);
            a.phlegm = (a.phlegm ?? 0) + (b.phlegm ?? 0);
            a.tissue = (a.tissue ?? 0) + (b.tissue ?? 0);
            a.muscle = (a.muscle ?? 0) + (b.muscle ?? 0);
            a.organoid = (a.organoid ?? 0) + (b.organoid ?? 0);
            a.organism = (a.organism ?? 0) + (b.organism ?? 0);
            a.alloy = (a.alloy ?? 0) + (b.alloy ?? 0);
            a.tube = (a.tube ?? 0) + (b.tube ?? 0);
            a.fixtures = (a.fixtures ?? 0) + (b.fixtures ?? 0);
            a.frame = (a.frame ?? 0) + (b.frame ?? 0);
            a.hydraulics = (a.hydraulics ?? 0) + (b.hydraulics ?? 0);
            a.machine = (a.machine ?? 0) + (b.machine ?? 0);
            a.condensate = (a.condensate ?? 0) + (b.condensate ?? 0);
            a.concentrate = (a.concentrate ?? 0) + (b.concentrate ?? 0);
            a.extract = (a.extract ?? 0) + (b.extract ?? 0);
            a.spirit = (a.spirit ?? 0) + (b.spirit ?? 0);
            a.emanation = (a.emanation ?? 0) + (b.emanation ?? 0);
            a.essence = (a.essence ?? 0) + (b.essence ?? 0);
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
