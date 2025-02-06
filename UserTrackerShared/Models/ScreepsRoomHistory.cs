using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTrackerShared.Models
{
    #region Base
    public class DecayTime
    {
        public long Timestamp { get; set; }
    }
    public class Effect
    {
        public long EffectType { get; set; }
        public long Power { get; set; }
        public long Level { get; set; }
        public long EndTime { get; set; }
        public long Duration { get; set; }
    }
    public class Store
    {
        public long? energy { get; set; }
        public long? power { get; set; }

        public long? H { get; set; }
        public long? O { get; set; }
        public long? U { get; set; }
        public long? L { get; set; }
        public long? K { get; set; }
        public long? Z { get; set; }
        public long? X { get; set; }
        public long? G { get; set; }

        public long? silicon { get; set; }
        public long? metal { get; set; }
        public long? biomass { get; set; }
        public long? mist { get; set; }

        public long? OH { get; set; }
        public long? ZK { get; set; }
        public long? UL { get; set; }

        public long? UH { get; set; }
        public long? UO { get; set; }
        public long? KH { get; set; }
        public long? KO { get; set; }
        public long? LH { get; set; }
        public long? LO { get; set; }
        public long? ZH { get; set; }
        public long? ZO { get; set; }
        public long? GH { get; set; }
        public long? GO { get; set; }

        public long? UH2O { get; set; }
        public long? UHO2 { get; set; }
        public long? KH2O { get; set; }
        public long? KHO2 { get; set; }
        public long? LH2O { get; set; }
        public long? LHO2 { get; set; }
        public long? ZH2O { get; set; }
        public long? ZHO2 { get; set; }
        public long? GH2O { get; set; }
        public long? GHO2 { get; set; }

        public long? XUH2O { get; set; }
        public long? XUHO2 { get; set; }
        public long? XKH2O { get; set; }
        public long? XKHO2 { get; set; }
        public long? XLH2O { get; set; }
        public long? XLHO2 { get; set; }
        public long? XZH2O { get; set; }
        public long? XZHO2 { get; set; }
        public long? XGH2O { get; set; }
        public long? XGHO2 { get; set; }

        public long? ops { get; set; }

        public long? utrium_bar { get; set; }
        public long? lemergium_bar { get; set; }
        public long? zynthium_bar { get; set; }
        public long? keanium_bar { get; set; }
        public long? ghodium_melt { get; set; }
        public long? oxidant { get; set; }
        public long? reductant { get; set; }
        public long? purifier { get; set; }
        public long? battery { get; set; }

        public long? composite { get; set; }
        public long? crystal { get; set; }
        public long? liquid { get; set; }

        public long? wire { get; set; }
        public long? Switch { get; set; }
        public long? transistor { get; set; }
        public long? microchip { get; set; }
        public long? circuit { get; set; }
        public long? device { get; set; }

        public long? cell { get; set; }
        public long? phlegm { get; set; }
        public long? tissue { get; set; }
        public long? muscle { get; set; }
        public long? organoid { get; set; }
        public long? organism { get; set; }

        public long? alloy { get; set; }
        public long? tube { get; set; }
        public long? fixtures { get; set; }
        public long? frame { get; set; }
        public long? hydraulics { get; set; }
        public long? machine { get; set; }

        public long? condensate { get; set; }
        public long? concentrate { get; set; }
        public long? extract { get; set; }
        public long? spirit { get; set; }
        public long? emanation { get; set; }
        public long? essence { get; set; }
    }
    public class Reservation
    {
        public string User { get; set; }
        public long EndTime { get; set; }
    }
    public class Datetime
    {
        public long Sign { get; set; }
    }
    public class Sign
    {
        public string User { get; set; }
        public string Text { get; set; }
        public long Time { get; set; }
        public long Datetime { get; set; }
    }
    public class HardSign
    {
        public string Text { get; set; }
        public long Time { get; set; }
        public long Datetime { get; set; }
        public long EndDatetime { get; set; }
    }
    public class BodyPart
    {
        public string Type { get; set; }
        public long Hits { get; set; }
        public string Boost { get; set; }
    }
    public class ReactionBase
    {
        public long X1 { get; set; }
        public long Y1 { get; set; }
        public long X2 { get; set; }
        public long Y2 { get; set; }
    }
    public class RunReaction : ReactionBase
    {
    }
    public class ReverseReaction : ReactionBase
    {
    }
    public class Produce : Coordinate
    {
        public string ResourceType { get; set; }
    }
    public class Population
    {
        public string Body { get; set; }
        public string Behavior { get; set; }
    }
    public class ActionLog
    {
        public Coordinate Attacked { get; set; }
        public Coordinate Healed { get; set; }
        public Coordinate Attack { get; set; }
        public Coordinate RangedAttack { get; set; }
        public Coordinate RangedMassAttack { get; set; }
        public Coordinate RangedHeal { get; set; }
        public Coordinate Harvest { get; set; }
        public Coordinate Heal { get; set; }
        public Coordinate Repair { get; set; }
        public Coordinate Build { get; set; }
        public SayAction Say { get; set; }
        public Coordinate UpgradeController { get; set; }
        public Coordinate ReserveController { get; set; }
        public Produce Produce { get; set; }
        public Coordinate TransferEnergy { get; set; }
        public object AttackController { get; set; }
        public RunReaction RunReaction { get; set; }
        public ReverseReaction ReverseReaction { get; set; }
        public object Spawned { get; set; }
        public PowerAction Power { get; set; }
    }
    public class PowerAction
    {
        public long Id { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
    }
    public class SayAction
    {
        public string Message { get; set; }
        public bool IsPublic { get; set; }
    }
    public class Coordinate
    {
        public long X { get; set; }
        public long Y { get; set; }
    }
    public class MemoryMove
    {
        public string Dest { get; set; }
        public string Path { get; set; }
        public long? Time { get; set; }
        public long LastMove { get; set; }
    }
    public class InterRoom
    {
        public string Room { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
    }
    public class Destination
    {
        public string Room { get; set; }
        public string Shard { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
    }
    public class Power
    {
        public long Level { get; set; }
        public long CooldownTime { get; set; }
    }
    public class RuinStructure
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public string User { get; set; }
    }
    public class Send
    {
        public string TargetRoomName { get; set; }
        public string ResourceType { get; set; }
        public long Amount { get; set; }
        public string Description { get; set; }
    }

    public class Spawning
    {
        public string Name { get; set; }
        public long NeedTime { get; set; }
        public long SpawnTime { get; set; }
        public long[] Directions { get; set; } = [];
    }
    #endregion
    #region Parent Base
    public class BaseStructure
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string StructureType { get; set; }
    }
    public class Creep
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
        public BodyPart[] Body { get; set; } = [];
        public Store Store { get; set; }
        public long StoreCapacity { get; set; }
        public string Type { get; set; }
        public string Room { get; set; }
        public string User { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public bool Spawning { get; set; }
        public bool NotifyWhenAttacked { get; set; }
        public ActionLog ActionLog { get; set; }
        public long AgeTime { get; set; }
        public InterRoom InterRoom { get; set; }
        public long fatigue { get; set; }
        public long? _fatigue { get; set; }
        public long? _oldFatigue { get; set; }
        public long TicksToLive { get; set; }
        public MemoryMove Memory_move { get; set; }
        public bool? _attack { get; set; }
        public string Memory_sourceId { get; set; }
        public string StrongholdId { get; set; }
        public string _pull { get; set; }
        public string _pulled { get; set; }
        public string Mission { get; set; }
        public long TombstoneDecay { get; set; }
        public bool NoCapacityRecalc { get; set; }
        public bool noInterShard { get; set; }
        public long? _healToApply { get; set; }
        public long? _damageToApply { get; set; }
    }
    public class PowerCreep
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ClassName { get; set; }
        public string User { get; set; }
        public long Level { get; set; }
        public long HitsMax { get; set; }
        public Store Store { get; set; }
        public long StoreCapacity { get; set; }
        public long? SpawnCooldownTime { get; set; }
        public Dictionary<string, Power> Powers { get; set; } = new();
        public string Shard { get; set; }
        public string Type { get; set; }
        public string Room { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
        public long Hits { get; set; }
        public long AgeTime { get; set; }
        public ActionLog ActionLog { get; set; }
        public bool NotifyWhenAttacked { get; set; }
        public long? _oldFatigue { get; set; }
        public long? _fatigue { get; set; }
        public long Fatigue { get; set; }
        public long? DeleteTime { get; set; }
        public InterRoom InterRoom { get; set; }
    }
    public class GroundResource : Store
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public long x { get; set; }
        public long y { get; set; }
        public string Room { get; set; }
        public string ResourceType { get; set; }
    }
    #endregion

    #region Structures
    public class StructureConstructionSite : BaseStructure
    {
        public long X { get; set; }
        public long Y { get; set; }
        public string Room { get; set; }
        public string User { get; set; }
        public long Progress { get; set; }
        public long ProgressTotal { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public long NextDecayTime { get; set; }
        public string Name { get; set; }
        public bool NotifyWhenAttacked { get; set; }
        public Store Store { get; set; }
        public Store StoreCapacityResource { get; set; }
    }   
    public class StructureTombstone : BaseStructure
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Room { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
        public string User { get; set; }
        public long DeathTime { get; set; }
        public long DecayTime { get; set; }
        public Store Store { get; set; }
        public long Tick { get; set; }
        public string CreepId { get; set; }
        public string CreepName { get; set; }
        public string CreepSaying { get; set; }
        public long CreepTicksToLive { get; set; }
        public string[] CreepBody { get; set; } = [];
        public string PowerCreepId { get; set; }
        public string PowerCreepName { get; set; }
        public long PowerCreepTicksToLive { get; set; }
        public string PowerCreepClassName { get; set; }
        public long PowerCreepLevel { get; set; }
        public Dictionary<string, Power> PowerCreepPowers { get; set; } = new();
        public string PowerCreepSaying { get; set; }
    }
    public class StructureRuin : BaseStructure
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Room { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
        public RuinStructure Structure { get; set; }
        public long DestroyTime { get; set; }
        public long DecayTime { get; set; }
        public string User { get; set; }
        public Store Store { get; set; }
    }
    public class StructureDepsoit : BaseStructure
    {
        public string DepositType { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
        public string Room { get; set; }
        public long Harvested { get; set; }
        public long DecayTime { get; set; }
        public long CooldownTime { get; set; }
        public long? _cooldown { get; set; }
    }
    public class StructureMineral : BaseStructure
    {
        public string MineralType { get; set; }
        public long MineralAmount { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
        public string Room { get; set; }
        public long Updated { get; set; }
        public long Density { get; set; }
        public long? NextRegenerationTime { get; set; }
        public Dictionary<string, Effect> Effects { get; set; } = new();
    }
    public class StructureSource : BaseStructure
    {
        public string Room { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
        public long Energy { get; set; }
        public long EnergyCapacity { get; set; }
        public long TicksToRegeneration { get; set; }
        public long? NextRegenerationTime { get; set; }
        public long InvaderHarvested { get; set; }
        public long Updated { get; set; }
        public Dictionary<string, Effect> Effects { get; set; } = new();
    }
    public class StructureSpawn : BaseStructure
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
        public string Room { get; set; }
        public bool NotifyWhenAttacked { get; set; }
        public string Name { get; set; }
        public string User { get; set; }
        public Store Store { get; set; }
        public Store StoreCapacityResource { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public bool Off { get; set; }
        public bool _off { get; set; }
        public Spawning Spawning { get; set; }
        public long Updated { get; set; }
        public Dictionary<string, Effect> Effects { get; set; } = new();
        public long Tick { get; set; }
    }
    public class StructureExtension : BaseStructure 
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
        public string Room { get; set; }
        public bool NotifyWhenAttacked { get; set; }
        public string User { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public bool Off { get; set; }
        public bool _off { get; set; }
        public long Updated { get; set; }
        public Store Store { get; set; }
        public Store StoreCapacityResource { get; set; }
    }
    public class StructureRoad : BaseStructure
    {
        public long X { get; set; }
        public long Y { get; set; }
        public string User { get; set; }
        public string Room   { get; set; }
        public bool NotifyWhenAttacked { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public long NextDecayTime { get; set; }
        public string StrongholdId { get; set; }
        public long DecayTime { get; set; }
        public Effect[] Effects { get; set; } = [];
        public long Updated { get; set; }
    }
    public class StructureWall : BaseStructure
    {
        public long X { get; set; }
        public long Y { get; set; }
        public string Room { get; set; }
        public bool NotifyWhenAttacked { get; set; }
        public string User { get; set; }
        public long Progress { get; set; }
        public long ProgressTotal { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public long Updated { get; set; }
        public DecayTime DecayTime { get; set; }
        public Dictionary<string, Effect> Effects { get; set; } = new();
    }
    public class StructureRampart : BaseStructure
    {
        public long X { get; set; }
        public long Y { get; set; }
        public string Room { get; set; }
        public bool NotifyWhenAttacked { get; set; }
        public string User { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public long NextDecayTime { get; set; }
        public long Updated { get; set; }
        public string StrongholdId { get; set; }
        public long DecayTime { get; set; }
        public Effect[] Effects { get; set; } = [];
        public long HitsTarget { get; set; }
        public bool IsPublic { get; set; }
        public long Progress { get; set; }
        public long ProgressTotal { get; set; }
    }
    public class StructureKeeperLair : BaseStructure
    {
        public string Room { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
        public long? NextSpawnTime { get; set; }
        public long Updated { get; set; }
    }
    public class StructurePortal : BaseStructure
    {
        public string Room { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
        public Destination Destination { get; set; }
        public long UnstableDate { get; set; }
        public long DecayTime { get; set; }
        public long Tick { get; set; }
        public bool Disabled { get; set; }
    }
    public class StructureController : BaseStructure
    {
        public string Room { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
        public long Level { get; set; }
        public string User { get; set; }
        public long Progress { get; set; }
        public long TicksToDowngrade { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public long ProgressTotal { get; set; }
        public long? DowngradeTime { get; set; }
        public Reservation Reservation { get; set; }
        public long Updated { get; set; }
        public Sign Sign { get; set; }
        public Datetime Datetime { get; set; }
        public long SafeModeAvailable { get; set; }
        public long? SafeMode { get; set; }
        public long? SafeModeCooldown { get; set; }
        public long? UpgradeBlocked { get; set; }
        public bool? IsPowerEnabled { get; set; }
        public Dictionary<string, Effect> Effects { get; set; } = new();
        public long? _upgraded { get; set; }
        public long NewField { get; set; }
        public HardSign HardSign { get; set; }
        public long PromoPeriodUntil { get; set; }
        public long New_field { get; set; }
        public Store Store { get; set; }
        public Store StoreCapacityResource { get; set; }
    }
    public class StructureLink : BaseStructure
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
        public string Room { get; set; }
        public bool NotifyWhenAttacked { get; set; }
        public string User { get; set; }
        public long Cooldown { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public ActionLog ActionLog { get; set; }
        public long Updated { get; set; }
        public Store Store { get; set; }
        public Store StoreCapacityResource { get; set; }
        public ActionLog _actionLog { get; set; }
    }
    public class StructureStorage : BaseStructure
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
        public string Room { get; set; }
        public bool NotifyWhenAttacked { get; set; }
        public string User { get; set; }
        public Store Store { get; set; }
        public long StoreCapacity { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public long Updated { get; set; }
        public Dictionary<string, Effect> Effects { get; set; } = new();
    }
    public class StructureTower : BaseStructure
    {
        public long X { get; set; }
        public long Y { get; set; }
        public string Room { get; set; }
        public bool NotifyWhenAttacked { get; set; }
        public string User { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public ActionLog ActionLog { get; set; }
        public long Updated { get; set; }
        public Store Store { get; set; }
        public Store StoreCapacityResource { get; set; }
        public ActionLog _actionLog { get; set; }
        public string StrongholdId { get; set; }
        public long DecayTime { get; set; }
        public Dictionary<string, Effect> Effects { get; set; } = new();
        public long Tick { get; set; }
    }
    public class StructureObserver : BaseStructure
    {
        public long X { get; set; }
        public long Y { get; set; }
        public string Room { get; set; }
        public bool NotifyWhenAttacked { get; set; }
        public string User { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public string ObserveRoom { get; set; }
        public long Updated { get; set; }
        public Dictionary<string, Effect> Effects { get; set; } = new();
    }
    public class StructurePowerBank : BaseStructure
    {
        public long X { get; set; }
        public long Y { get; set; }
        public string Room { get; set; }
        public Store Store { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public long DecayTime { get; set; }
    }
    public class StructurePowerSpawn : BaseStructure
    {
        public long X { get; set; }
        public long Y { get; set; }
        public string Room { get; set; }
        public bool NotifyWhenAttacked { get; set; }
        public string User { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public Store Store { get; set; }
        public Store StoreCapacityResource { get; set; }
        public Dictionary<string, Effect> Effects { get; set; } = new();
        public long Updated { get; set; }
    }
    public class StructureExtractor : BaseStructure
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
        public string Room { get; set; }
        public bool NotifyWhenAttacked { get; set; }
        public string User { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public long? _cooldown { get; set; }
        public long Cooldown { get; set; }
        public long Updated { get; set; }
    }
    public class StructureLab : BaseStructure
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
        public string Room { get; set; }
        public bool NotifyWhenAttacked { get; set; }
        public string User { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public long MineralAmount { get; set; }
        public long Cooldown { get; set; }
        public Store Store { get; set; }
        public long? StoreCapacity { get; set; }
        public Store StoreCapacityResource { get; set; }
        public ActionLog ActionLog { get; set; }
        public ActionLog _actionLog { get; set; }
        public long Updated { get; set; }
        public long CooldownTime { get; set; }
        public Dictionary<string, Effect> Effects { get; set; } = new();
        public string Tick { get; set; }
    }
    public class StructureTerminal : BaseStructure
    {
        public string Room { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
        public Store Store { get; set; }
        public long StoreCapacity { get; set; }
        public bool NotifyWhenAttacked { get; set; }
        public string User { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public long CooldownTime { get; set; }
        public Send Send { get; set; }
        public long Updated { get; set; }
        public Dictionary<string, Effect> Effects { get; set; } = new();
    }
    public class StructureContainer : BaseStructure
    {
        public long X { get; set; }
        public long Y { get; set; }
        public string Room { get; set; }
        public bool NotifyWhenAttacked { get; set; }
        public Store Store { get; set; }
        public long StoreCapacity { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public long NextDecayTime { get; set; }
        public string StrongholdId { get; set; }
        public long DecayTime { get; set; }
        public Effect[] Effects { get; set; } = [];
        public long Updated { get; set; }
    }
    public class StructureNuker : BaseStructure
    {
        public long X { get; set; }
        public long Y { get; set; }
        public string Room { get; set; }
        public bool NotifyWhenAttacked { get; set; }
        public string User { get; set; }
        public Store Store { get; set; }
        public Store StoreCapacityResource { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public long CooldownTime { get; set; }
        public long Updated { get; set; }
    }
    public class StructureFactory : BaseStructure
    {
        public long X { get; set; }
        public long Y { get; set; }
        public string Room { get; set; }
        public bool NotifyWhenAttacked { get; set; }
        public string User { get; set; }
        public Store Store { get; set; }
        public long StoreCapacity { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public long Cooldown { get; set; }
        public ActionLog ActionLog { get; set; }
        public long CooldownTime { get; set; }
        public ActionLog _actionLog { get; set; }
        public Dictionary<string, Effect> Effects { get; set; } = new();
        public long Level { get; set; }
    }
    public class StructureInvaderCore : BaseStructure
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public long Level { get; set; }
        public string StrongholdBehavior { get; set; }
        public string Room { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
        public string User { get; set; }
        public string TemplateName { get; set; }
        public long Hits { get; set; }
        public long HitsMax { get; set; }
        public long NextExpandTime { get; set; }
        public string DepositType { get; set; }
        public long? DeployTime { get; set; }
        public string StrongholdId { get; set; }
        public Effect[] Effects { get; set; } = [];
        public ActionLog ActionLog { get; set; }
        public long DecayTime { get; set; }
        public ActionLog _actionLog { get; set; }
        public Dictionary<string, Population> Population { get; set; } = new();
        public bool _spawning { get; set; }
        public Spawning Spawning { get; set; }
        public string Tick { get; set; }
    }
    public class StructureNuke : BaseStructure
    {
        public string Room { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
        public long LandTime { get; set; }
        public string LaunchRoomName { get; set; }
    }
    #endregion

    #region Groups
    public class Creeps
    {
        public Dictionary<string, Creep> OwnedCreeps { get; set; } = new Dictionary<string, Creep>();
        public Dictionary<string, Creep> EnemyCreeps { get; set; } = new Dictionary<string, Creep>();
        public Dictionary<string, Creep> OtherCreeps { get; set; } = new Dictionary<string, Creep>();
        public Dictionary<string, PowerCreep> PowerCreeps { get; set; } = new Dictionary<string, PowerCreep>();
    }


    public class Structures
    {
        public StructureController? Controller { get; set; }
        public StructureMineral? Mineral { get; set; }
        public StructureDepsoit? Deposit { get; set; }


        public Dictionary<string, StructureWall> Walls { get; set; } = new Dictionary<string, StructureWall>();
        public Dictionary<string, StructureConstructionSite> ConstructionSites { get; set; } = new Dictionary<string, StructureConstructionSite>();
        public Dictionary<string, StructureContainer> Containers { get; set; } = new Dictionary<string, StructureContainer>();
        public Dictionary<string, StructureExtension> Extensions { get; set; } = new Dictionary<string, StructureExtension>();
        public Dictionary<string, StructureExtractor> Extractors { get; set; } = new Dictionary<string, StructureExtractor>();
        public Dictionary<string, StructureFactory> Factories { get; set; } = new Dictionary<string, StructureFactory>();
        public Dictionary<string, StructureInvaderCore> InvaderCores { get; set; } = new Dictionary<string, StructureInvaderCore>();
        public Dictionary<string, StructureKeeperLair> KeeperLairs { get; set; } = new Dictionary<string, StructureKeeperLair>();
        public Dictionary<string, StructureLab> Labs { get; set; } = new Dictionary<string, StructureLab>();
        public Dictionary<string, StructureLink> Links { get; set; } = new Dictionary<string, StructureLink>();
        public Dictionary<string, StructureNuker> Nukers { get; set; } = new Dictionary<string, StructureNuker>();
        public Dictionary<string, StructureObserver> Observers { get; set; } = new Dictionary<string, StructureObserver>();
        public Dictionary<string, StructurePortal> Portals { get; set; } = new Dictionary<string, StructurePortal>();
        public Dictionary<string, StructurePowerBank> PowerBanks { get; set; } = new Dictionary<string, StructurePowerBank>();
        public Dictionary<string, StructurePowerSpawn> PowerSpawns { get; set; } = new Dictionary<string, StructurePowerSpawn>();
        public Dictionary<string, StructureRampart> Ramparts { get; set; } = new Dictionary<string, StructureRampart>();
        public Dictionary<string, StructureRoad> Roads { get; set; } = new Dictionary<string, StructureRoad>();
        public Dictionary<string, StructureRuin> Ruins { get; set; } = new Dictionary<string, StructureRuin>();
        public Dictionary<string, StructureSource> Sources { get; set; } = new Dictionary<string, StructureSource>();
        public Dictionary<string, StructureSpawn> Spawns { get; set; } = new Dictionary<string, StructureSpawn>();
        public Dictionary<string, StructureStorage> Storages { get; set; } = new Dictionary<string, StructureStorage>();
        public Dictionary<string, StructureTerminal> Terminals { get; set; } = new Dictionary<string, StructureTerminal>();
        public Dictionary<string, StructureTombstone> Tombstones { get; set; } = new Dictionary<string, StructureTombstone>();
        public Dictionary<string, StructureTower> Towers { get; set; } = new Dictionary<string, StructureTower>();
        public Dictionary<string, StructureNuke> Nukes { get; set; } = new Dictionary<string, StructureNuke>();
    }

    #endregion
    public class ScreepsRoomHistory
    {
        public long TimeStamp { get; set; }
        public long Tick { get; set; }
        public Dictionary<string, GroundResource> GroundResources { get; set; } = new Dictionary<string, GroundResource>();
        public Creeps Creeps { get; set; } = new Creeps();
        public Structures Structures { get; set; } = new Structures();
        public Dictionary<string, string> TypeMap { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> UserMap { get; set; } = new Dictionary<string, string>();
    }
}
