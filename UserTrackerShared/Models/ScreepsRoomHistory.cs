using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTrackerShared.Models
{
    #region Base
    public class BaseStructure
    {
        public string Id { get; set; } = "";
    }
    public class Creep
    {
        public string Id { get; set; } = "";
    }
    public class PowerCreep
    {
        public string Id { get; set; } = "";
    }
    public class GroundResource
    {
        public string Id { get; set; } = "";
    }
    public class Store
    {
        public string? UTRIUM_LEMERGITE { get; set; } = null;
        public string? UTRIUM_HYDRIDE { get; set; } = null;
        public string? UTRIUM_OXIDE { get; set; } = null;
        public string? KEANIUM_HYDRIDE { get; set; } = null;
        public string? KEANIUM_OXIDE { get; set; } = null;
        public string? LEMERGIUM_HYDRIDE { get; set; } = null;
        public string? LEMERGIUM_OXIDE { get; set; } = null;
        public string? ZYNTHIUM_HYDRIDE { get; set; } = null;
        public string? ZYNTHIUM_OXIDE { get; set; } = null;
        public string? GHODIUM_HYDRIDE { get; set; } = null;
        public string? GHODIUM_OXIDE { get; set; } = null;
        public string? UTRIUM_ACID { get; set; } = null;
        public string? UTRIUM_ALKALIDE { get; set; } = null;
        public string? KEANIUM_ACID { get; set; } = null;
        public string? KEANIUM_ALKALIDE { get; set; } = null;
        public string? LEMERGIUM_ACID { get; set; } = null;
        public string? LEMERGIUM_ALKALIDE { get; set; } = null;
        public string? ZYNTHIUM_ACID { get; set; } = null;
        public string? ZYNTHIUM_ALKALIDE { get; set; } = null;
        public string? GHODIUM_ACID { get; set; } = null;
        public string? GHODIUM_ALKALIDE { get; set; } = null;
        public string? CATALYZED_UTRIUM_ACID { get; set; } = null;
        public string? CATALYZED_UTRIUM_ALKALIDE { get; set; } = null;
        public string? CATALYZED_KEANIUM_ACID { get; set; } = null;
        public string? CATALYZED_KEANIUM_ALKALIDE { get; set; } = null;
        public string? CATALYZED_LEMERGIUM_ACID { get; set; } = null;
        public string? CATALYZED_LEMERGIUM_ALKALIDE { get; set; } = null;
        public string? CATALYZED_ZYNTHIUM_ACID { get; set; } = null;
        public string? CATALYZED_ZYNTHIUM_ALKALIDE { get; set; } = null;
        public string? CATALYZED_GHODIUM_ACID { get; set; } = null;
        public string? CATALYZED_GHODIUM_ALKALIDE { get; set; } = null;
        public string? OPS { get; set; } = null;
        public string? UTRIUM_BAR { get; set; } = null;
        public string? LEMERGIUM_BAR { get; set; } = null;
        public string? ZYNTHIUM_BAR { get; set; } = null;
        public string? KEANIUM_BAR { get; set; } = null;
        public string? GHODIUM_MELT { get; set; } = null;
        public string? OXIDANT { get; set; } = null;
        public string? REDUCTANT { get; set; } = null;
        public string? PURIFIER { get; set; } = null;
        public string? BATTERY { get; set; } = null;
        public string? COMPOSITE { get; set; } = null;
        public string? CRYSTAL { get; set; } = null;
        public string? LIQUID { get; set; } = null;
        public string? WIRE { get; set; } = null;
        public string? SWITCH { get; set; } = null;
        public string? TRANSISTOR { get; set; } = null;
        public string? MICROCHIP { get; set; } = null;
        public string? CIRCUIT { get; set; } = null;
        public string? DEVICE { get; set; } = null;
        public string? CELL { get; set; } = null;
        public string? PHLEGM { get; set; } = null;
        public string? TISSUE { get; set; } = null;
        public string? MUSCLE { get; set; } = null;
        public string? ORGANOID { get; set; } = null;
        public string? ORGANISM { get; set; } = null;
        public string? ALLOY { get; set; } = null;
        public string? TUBE { get; set; } = null;
        public string? FIXTURES { get; set; } = null;
        public string? FRAME { get; set; } = null;
        public string? HYDRAULICS { get; set; } = null;
        public string? MACHINE { get; set; } = null;
        public string? CONDENSATE { get; set; } = null;
        public string? CONCENTRATE { get; set; } = null;
        public string? EXTRACT { get; set; } = null;
        public string? SPIRIT { get; set; } = null;
        public string? EMANATION { get; set; } = null;
        public string? ESSENCE { get; set; } = null;
    }
    #endregion

    #region Structures
    public class StructureConstructionSite : BaseStructure
    {
    }
    public class StructureTombstone : BaseStructure
    {
    }
    public class StructureRuin : BaseStructure
    {
    }
    public class StructureDepsoit : BaseStructure
    {
    }
    public class StructureMineral : BaseStructure
    {
    }
    public class StructureSource : BaseStructure
    {
    }

    public class StructureSpawn : BaseStructure
    {
    }
    public class StructureExtension : BaseStructure 
    {
    }
    public class StructureRoad : BaseStructure
    {
    }
    public class StructureWall : BaseStructure
    {
    }
    public class StructureRampart : BaseStructure
    {
    }
    public class StructureKeeperLair : BaseStructure
    {
    }
    public class StructurePortal : BaseStructure
    {
    }
    public class StructureController : BaseStructure
    {
    }
    public class StructureLink : BaseStructure
    {
    }
    public class StructureStorage : BaseStructure
    {
    }
    public class StructureTower : BaseStructure
    {
    }
    public class StructureObserver : BaseStructure
    {
    }
    public class StructurePowerBank : BaseStructure
    {
    }
    public class StructurePowerSpawn : BaseStructure
    {
    }
    public class StructureExtractor : BaseStructure
    {
    }
    public class StructureLab : BaseStructure
    {
    }
    public class StructureTerminal : BaseStructure
    {
    }
    public class StructureContainer : BaseStructure
    {
    }
    public class StructureNuker : BaseStructure
    {
    }
    public class StructureFactory : BaseStructure
    {
    }
    public class StructureInvaderCore : BaseStructure
    {
    }
    #endregion

    #region Groups
    public class Creeps
    {
        public List<PowerCreep> PowerCreeps = new List<PowerCreep>();
        public List<Creep> OwnedCreeps = new List<Creep>();
        public List<Creep> EnemyCreeps = new List<Creep>();
        public List<Creep> OtherCreeps = new List<Creep>();
    }


    public class Structures
    {
        public StructureDepsoit? Deposit = null;
        public StructureMineral? Mineral = null;
        public StructureController? Controller = null;

        public List<StructureConstructionSite> ConstructionSites = new List<StructureConstructionSite>();
        public List<StructureTombstone> Tombstones = new List<StructureTombstone>();
        public List<StructureRuin> Ruins = new List<StructureRuin>();
        public List<StructureSource> Sources = new List<StructureSource>();

        public List<StructureSpawn> Spawns = new List<StructureSpawn>();
        public List<StructureExtension> Extensions = new List<StructureExtension>();
        public List<StructureRoad> Roads = new List<StructureRoad>();
        public List<StructureWall> Walls = new List<StructureWall>();
        public List<StructureRampart> Ramparts = new List<StructureRampart>();
        public List<StructureKeeperLair> KeeperLairs = new List<StructureKeeperLair>();
        public List<StructurePortal> Portals = new List<StructurePortal>();
        public List<StructureLink> Links = new List<StructureLink>();
        public List<StructureStorage> Storages = new List<StructureStorage>();
        public List<StructureTower> Towers = new List<StructureTower>();
        public List<StructureObserver> Observers = new List<StructureObserver>();
        public List<StructurePowerBank> PowerBanks = new List<StructurePowerBank>();
        public List<StructurePowerSpawn> PowerSpawns = new List<StructurePowerSpawn>();
        public List<StructureExtractor> Extractors = new List<StructureExtractor>();
        public List<StructureLab> Labs = new List<StructureLab>();
        public List<StructureTerminal> Terminals = new List<StructureTerminal>();
        public List<StructureContainer> Containers = new List<StructureContainer>();
        public List<StructureNuker> Nukers = new List<StructureNuker>();
        public List<StructureFactory> Factories = new List<StructureFactory>();
        public List<StructureInvaderCore> InvaderCores = new List<StructureInvaderCore>();
    }

    #endregion
    public class ScreepsRoomHistory
    {
        public long TimeStamp { get; set; }
        public long Base { get; set; }

        public List<GroundResource> GroundResources = new List<GroundResource>();
        public Creeps Creeps { get; set; } = new Creeps();
        public Structures Structures { get; set; } = new Structures();

        public Dictionary<string,string> TypeMap = new Dictionary<string, string>();
    }
}
