using Newtonsoft.Json;

namespace UserTrackerShared.Models
{
    #region Base
    public class DecayTime
    {
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
    }

    public class Effect
    {
        [JsonProperty("effectType")]
        public long EffectType { get; set; }

        [JsonProperty("power")]
        public long Power { get; set; }

        [JsonProperty("level")]
        public long Level { get; set; }

        [JsonProperty("endTime")]
        public long EndTime { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }
    }

    public class Store
    {
        [JsonProperty("energy")]
        public long? energy { get; set; }

        [JsonProperty("power")]
        public long? power { get; set; }

        [JsonProperty("h")]
        public long? H { get; set; }

        [JsonProperty("o")]
        public long? O { get; set; }

        [JsonProperty("u")]
        public long? U { get; set; }

        [JsonProperty("l")]
        public long? L { get; set; }

        [JsonProperty("k")]
        public long? K { get; set; }

        [JsonProperty("z")]
        public long? Z { get; set; }

        [JsonProperty("x")]
        public long? X { get; set; }

        [JsonProperty("g")]
        public long? G { get; set; }

        [JsonProperty("silicon")]
        public long? silicon { get; set; }

        [JsonProperty("metal")]
        public long? metal { get; set; }

        [JsonProperty("biomass")]
        public long? biomass { get; set; }

        [JsonProperty("mist")]
        public long? mist { get; set; }

        [JsonProperty("oh")]
        public long? OH { get; set; }

        [JsonProperty("zk")]
        public long? ZK { get; set; }

        [JsonProperty("ul")]
        public long? UL { get; set; }

        [JsonProperty("uh")]
        public long? UH { get; set; }

        [JsonProperty("uo")]
        public long? UO { get; set; }

        [JsonProperty("kh")]
        public long? KH { get; set; }

        [JsonProperty("ko")]
        public long? KO { get; set; }

        [JsonProperty("lh")]
        public long? LH { get; set; }

        [JsonProperty("lo")]
        public long? LO { get; set; }

        [JsonProperty("zh")]
        public long? ZH { get; set; }

        [JsonProperty("zo")]
        public long? ZO { get; set; }

        [JsonProperty("gh")]
        public long? GH { get; set; }

        [JsonProperty("go")]
        public long? GO { get; set; }

        [JsonProperty("uh2o")]
        public long? UH2O { get; set; }

        [JsonProperty("uho2")]
        public long? UHO2 { get; set; }

        [JsonProperty("kh2o")]
        public long? KH2O { get; set; }

        [JsonProperty("kho2")]
        public long? KHO2 { get; set; }

        [JsonProperty("lh2o")]
        public long? LH2O { get; set; }

        [JsonProperty("lho2")]
        public long? LHO2 { get; set; }

        [JsonProperty("zh2o")]
        public long? ZH2O { get; set; }

        [JsonProperty("zho2")]
        public long? ZHO2 { get; set; }

        [JsonProperty("gh2o")]
        public long? GH2O { get; set; }

        [JsonProperty("gho2")]
        public long? GHO2 { get; set; }

        [JsonProperty("xuh2o")]
        public long? XUH2O { get; set; }

        [JsonProperty("xuho2")]
        public long? XUHO2 { get; set; }

        [JsonProperty("xkh2o")]
        public long? XKH2O { get; set; }

        [JsonProperty("xkho2")]
        public long? XKHO2 { get; set; }

        [JsonProperty("xlh2o")]
        public long? XLH2O { get; set; }

        [JsonProperty("xlho2")]
        public long? XLHO2 { get; set; }

        [JsonProperty("xzh2o")]
        public long? XZH2O { get; set; }

        [JsonProperty("xzho2")]
        public long? XZHO2 { get; set; }

        [JsonProperty("xgh2o")]
        public long? XGH2O { get; set; }

        [JsonProperty("xgho2")]
        public long? XGHO2 { get; set; }

        [JsonProperty("ops")]
        public long? ops { get; set; }

        [JsonProperty("utrium_bar")]
        public long? utrium_bar { get; set; }

        [JsonProperty("lemergium_bar")]
        public long? lemergium_bar { get; set; }

        [JsonProperty("zynthium_bar")]
        public long? zynthium_bar { get; set; }

        [JsonProperty("keanium_bar")]
        public long? keanium_bar { get; set; }

        [JsonProperty("ghodium_melt")]
        public long? ghodium_melt { get; set; }

        [JsonProperty("oxidant")]
        public long? oxidant { get; set; }

        [JsonProperty("reductant")]
        public long? reductant { get; set; }

        [JsonProperty("purifier")]
        public long? purifier { get; set; }

        [JsonProperty("battery")]
        public long? battery { get; set; }

        [JsonProperty("composite")]
        public long? composite { get; set; }

        [JsonProperty("crystal")]
        public long? crystal { get; set; }

        [JsonProperty("liquid")]
        public long? liquid { get; set; }

        [JsonProperty("wire")]
        public long? wire { get; set; }

        [JsonProperty("switch")]
        public long? Switch { get; set; }

        [JsonProperty("transistor")]
        public long? transistor { get; set; }

        [JsonProperty("microchip")]
        public long? microchip { get; set; }

        [JsonProperty("circuit")]
        public long? circuit { get; set; }

        [JsonProperty("device")]
        public long? device { get; set; }

        [JsonProperty("cell")]
        public long? cell { get; set; }

        [JsonProperty("phlegm")]
        public long? phlegm { get; set; }

        [JsonProperty("tissue")]
        public long? tissue { get; set; }

        [JsonProperty("muscle")]
        public long? muscle { get; set; }

        [JsonProperty("organoid")]
        public long? organoid { get; set; }

        [JsonProperty("organism")]
        public long? organism { get; set; }

        [JsonProperty("alloy")]
        public long? alloy { get; set; }

        [JsonProperty("tube")]
        public long? tube { get; set; }

        [JsonProperty("fixtures")]
        public long? fixtures { get; set; }

        [JsonProperty("frame")]
        public long? frame { get; set; }

        [JsonProperty("hydraulics")]
        public long? hydraulics { get; set; }

        [JsonProperty("machine")]
        public long? machine { get; set; }

        [JsonProperty("condensate")]
        public long? condensate { get; set; }

        [JsonProperty("concentrate")]
        public long? concentrate { get; set; }

        [JsonProperty("extract")]
        public long? extract { get; set; }

        [JsonProperty("spirit")]
        public long? spirit { get; set; }

        [JsonProperty("emanation")]
        public long? emanation { get; set; }

        [JsonProperty("essence")]
        public long? essence { get; set; }

        public void Clear()
        {
            energy = null;
            power = null;
            H = null;
            O = null;
            U = null;
            L = null;
            K = null;
            Z = null;
            X = null;
            G = null;
            silicon = null;
            metal = null;
            biomass = null;
            mist = null;
            OH = null;
            ZK = null;
            UL = null;
            UH = null;
            UO = null;
            KH = null;
            KO = null;
            LH = null;
            LO = null;
            ZH = null;
            ZO = null;
            GH = null;
            GO = null;
            UH2O = null;
            UHO2 = null;
            KH2O = null;
            KHO2 = null;
            LH2O = null;
            LHO2 = null;
            ZH2O = null;
            ZHO2 = null;
            GH2O = null;
            GHO2 = null;
            XUH2O = null;
            XUHO2 = null;
            XKH2O = null;
            XKHO2 = null;
            XLH2O = null;
            XLHO2 = null;
            XZH2O = null;
            XZHO2 = null;
            XGH2O = null;
            XGHO2 = null;
            ops = null;
            utrium_bar = null;
            lemergium_bar = null;
            zynthium_bar = null;
            keanium_bar = null;
            ghodium_melt = null;
            oxidant = null;
            reductant = null;
            purifier = null;
            battery = null;
            composite = null;
            crystal = null;
            liquid = null;
            wire = null;
            Switch = null;
            transistor = null;
            microchip = null;
            circuit = null;
            device = null;
            cell = null;
            phlegm = null;
            tissue = null;
            muscle = null;
            organoid = null;
            organism = null;
            alloy = null;
            tube = null;
            fixtures = null;
            frame = null;
            hydraulics = null;
            machine = null;
            condensate = null;
            concentrate = null;
            extract = null;
            spirit = null;
            emanation = null;
            essence = null;
        }
    }

    public class Reservation
    {
        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("endTime")]
        public long EndTime { get; set; }
    }

    public class Datetime
    {
        [JsonProperty("sign")]
        public long Sign { get; set; }
    }

    public class Sign
    {
        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("datetime")]
        public long Datetime { get; set; }
    }

    public class HardSign
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("datetime")]
        public long Datetime { get; set; }

        [JsonProperty("endDatetime")]
        public long EndDatetime { get; set; }
    }

    public class BodyPart
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("boost")]
        public string Boost { get; set; }
    }

    public class ReactionBase
    {
        [JsonProperty("x1")]
        public long X1 { get; set; }

        [JsonProperty("y1")]
        public long Y1 { get; set; }

        [JsonProperty("x2")]
        public long X2 { get; set; }

        [JsonProperty("y2")]
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
        [JsonProperty("resourceType")]
        public string ResourceType { get; set; }
    }

    public class Population
    {
        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("behavior")]
        public string Behavior { get; set; }
    }

    public class ActionLog
    {
        [JsonProperty("attacked")]
        public Coordinate Attacked { get; set; }

        [JsonProperty("healed")]
        public Coordinate Healed { get; set; }

        [JsonProperty("attack")]
        public Coordinate Attack { get; set; }

        [JsonProperty("rangedAttack")]
        public Coordinate RangedAttack { get; set; }

        [JsonProperty("rangedMassAttack")]
        public Coordinate RangedMassAttack { get; set; }

        [JsonProperty("rangedHeal")]
        public Coordinate RangedHeal { get; set; }

        [JsonProperty("harvest")]
        public Coordinate Harvest { get; set; }

        [JsonProperty("heal")]
        public Coordinate Heal { get; set; }

        [JsonProperty("repair")]
        public Coordinate Repair { get; set; }

        [JsonProperty("build")]
        public Coordinate Build { get; set; }

        [JsonProperty("say")]
        public SayAction Say { get; set; }

        [JsonProperty("upgradeController")]
        public Coordinate UpgradeController { get; set; }

        [JsonProperty("reserveController")]
        public Coordinate ReserveController { get; set; }

        [JsonProperty("produce")]
        public Produce Produce { get; set; }

        [JsonProperty("transferEnergy")]
        public Coordinate TransferEnergy { get; set; }

        [JsonProperty("attackController")]
        public object AttackController { get; set; }

        [JsonProperty("runReaction")]
        public RunReaction RunReaction { get; set; }

        [JsonProperty("reverseReaction")]
        public ReverseReaction ReverseReaction { get; set; }

        [JsonProperty("spawned")]
        public object Spawned { get; set; }

        [JsonProperty("power")]
        public PowerAction Power { get; set; }
    }

    public class PowerAction
    {
        [JsonProperty("_id")]
        public long Id { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }
    }

    public class SayAction
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("isPublic")]
        public bool IsPublic { get; set; }
    }

    public class Coordinate
    {
        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }
    }

    public class MemoryMove
    {
        [JsonProperty("dest")]
        public string Dest { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("time")]
        public long? Time { get; set; }

        [JsonProperty("lastMove")]
        public long LastMove { get; set; }
    }

    public class InterRoom
    {
        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }
    }

    public class Destination
    {
        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("shard")]
        public string Shard { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }
    }

    public class Power
    {
        [JsonProperty("level")]
        public long Level { get; set; }

        [JsonProperty("cooldownTime")]
        public long CooldownTime { get; set; }
    }

    public class RuinStructure
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }
    }

    public class Send
    {
        [JsonProperty("targetRoomName")]
        public string TargetRoomName { get; set; }

        [JsonProperty("resourceType")]
        public string ResourceType { get; set; }

        [JsonProperty("amount")]
        public long Amount { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class StockpileResource
    {
        [JsonProperty("stockpiled")]
        public long Stockpiled { get; set; }
        [JsonProperty("stockpileUseTick")]
        public long StockpileUseTick { get; set; }

        public void Clear()
        {
            Stockpiled = 0;
            StockpileUseTick = 0;
        }
    }

    public class MarketData
    {
        [JsonProperty("nextUpdate")]
        public long NextUpdate { get; set; }

        [JsonProperty("energy")]
        public StockpileResource energy { get; set; }

        [JsonProperty("power")]
        public StockpileResource power { get; set; }

        [JsonProperty("h")]
        public StockpileResource H { get; set; }

        [JsonProperty("o")]
        public StockpileResource O { get; set; }

        [JsonProperty("u")]
        public StockpileResource U { get; set; }

        [JsonProperty("l")]
        public StockpileResource L { get; set; }

        [JsonProperty("k")]
        public StockpileResource K { get; set; }

        [JsonProperty("z")]
        public StockpileResource Z { get; set; }

        [JsonProperty("x")]
        public StockpileResource X { get; set; }

        [JsonProperty("g")]
        public StockpileResource G { get; set; }

        [JsonProperty("silicon")]
        public StockpileResource silicon { get; set; }

        [JsonProperty("metal")]
        public StockpileResource metal { get; set; }

        [JsonProperty("biomass")]
        public StockpileResource biomass { get; set; }

        [JsonProperty("mist")]
        public StockpileResource mist { get; set; }

        [JsonProperty("oh")]
        public StockpileResource OH { get; set; }

        [JsonProperty("zk")]
        public StockpileResource ZK { get; set; }

        [JsonProperty("ul")]
        public StockpileResource UL { get; set; }

        [JsonProperty("uh")]
        public StockpileResource UH { get; set; }

        [JsonProperty("uo")]
        public StockpileResource UO { get; set; }

        [JsonProperty("kh")]
        public StockpileResource KH { get; set; }

        [JsonProperty("ko")]
        public StockpileResource KO { get; set; }

        [JsonProperty("lh")]
        public StockpileResource LH { get; set; }

        [JsonProperty("lo")]
        public StockpileResource LO { get; set; }

        [JsonProperty("zh")]
        public StockpileResource ZH { get; set; }

        [JsonProperty("zo")]
        public StockpileResource ZO { get; set; }

        [JsonProperty("gh")]
        public StockpileResource GH { get; set; }

        [JsonProperty("go")]
        public StockpileResource GO { get; set; }

        [JsonProperty("uh2o")]
        public StockpileResource UH2O { get; set; }

        [JsonProperty("uho2")]
        public StockpileResource UHO2 { get; set; }

        [JsonProperty("kh2o")]
        public StockpileResource KH2O { get; set; }

        [JsonProperty("kho2")]
        public StockpileResource KHO2 { get; set; }

        [JsonProperty("lh2o")]
        public StockpileResource LH2O { get; set; }

        [JsonProperty("lho2")]
        public StockpileResource LHO2 { get; set; }

        [JsonProperty("zh2o")]
        public StockpileResource ZH2O { get; set; }

        [JsonProperty("zho2")]
        public StockpileResource ZHO2 { get; set; }

        [JsonProperty("gh2o")]
        public StockpileResource GH2O { get; set; }

        [JsonProperty("gho2")]
        public StockpileResource GHO2 { get; set; }

        [JsonProperty("xuh2o")]
        public StockpileResource XUH2O { get; set; }

        [JsonProperty("xuho2")]
        public StockpileResource XUHO2 { get; set; }

        [JsonProperty("xkh2o")]
        public StockpileResource XKH2O { get; set; }

        [JsonProperty("xkho2")]
        public StockpileResource XKHO2 { get; set; }

        [JsonProperty("xlh2o")]
        public StockpileResource XLH2O { get; set; }

        [JsonProperty("xlho2")]
        public StockpileResource XLHO2 { get; set; }

        [JsonProperty("xzh2o")]
        public StockpileResource XZH2O { get; set; }

        [JsonProperty("xzho2")]
        public StockpileResource XZHO2 { get; set; }

        [JsonProperty("xgh2o")]
        public StockpileResource XGH2O { get; set; }

        [JsonProperty("xgho2")]
        public StockpileResource XGHO2 { get; set; }

        [JsonProperty("ops")]
        public StockpileResource ops { get; set; }

        [JsonProperty("utrium_bar")]
        public StockpileResource utrium_bar { get; set; }

        [JsonProperty("lemergium_bar")]
        public StockpileResource lemergium_bar { get; set; }

        [JsonProperty("zynthium_bar")]
        public StockpileResource zynthium_bar { get; set; }

        [JsonProperty("keanium_bar")]
        public StockpileResource keanium_bar { get; set; }

        [JsonProperty("ghodium_melt")]
        public StockpileResource ghodium_melt { get; set; }

        [JsonProperty("oxidant")]
        public StockpileResource oxidant { get; set; }

        [JsonProperty("reductant")]
        public StockpileResource reductant { get; set; }

        [JsonProperty("purifier")]
        public StockpileResource purifier { get; set; }

        [JsonProperty("battery")]
        public StockpileResource battery { get; set; }

        [JsonProperty("composite")]
        public StockpileResource composite { get; set; }

        [JsonProperty("crystal")]
        public StockpileResource crystal { get; set; }

        [JsonProperty("liquid")]
        public StockpileResource liquid { get; set; }

        [JsonProperty("wire")]
        public StockpileResource wire { get; set; }

        [JsonProperty("switch")]
        public StockpileResource Switch { get; set; }

        [JsonProperty("transistor")]
        public StockpileResource transistor { get; set; }

        [JsonProperty("microchip")]
        public StockpileResource microchip { get; set; }

        [JsonProperty("circuit")]
        public StockpileResource circuit { get; set; }

        [JsonProperty("device")]
        public StockpileResource device { get; set; }

        [JsonProperty("cell")]
        public StockpileResource cell { get; set; }

        [JsonProperty("phlegm")]
        public StockpileResource phlegm { get; set; }

        [JsonProperty("tissue")]
        public StockpileResource tissue { get; set; }

        [JsonProperty("muscle")]
        public StockpileResource muscle { get; set; }

        [JsonProperty("organoid")]
        public StockpileResource organoid { get; set; }

        [JsonProperty("organism")]
        public StockpileResource organism { get; set; }

        [JsonProperty("alloy")]
        public StockpileResource alloy { get; set; }

        [JsonProperty("tube")]
        public StockpileResource tube { get; set; }

        [JsonProperty("fixtures")]
        public StockpileResource fixtures { get; set; }

        [JsonProperty("frame")]
        public StockpileResource frame { get; set; }

        [JsonProperty("hydraulics")]
        public StockpileResource hydraulics { get; set; }

        [JsonProperty("machine")]
        public StockpileResource machine { get; set; }

        [JsonProperty("condensate")]
        public StockpileResource condensate { get; set; }

        [JsonProperty("concentrate")]
        public StockpileResource concentrate { get; set; }

        [JsonProperty("extract")]
        public StockpileResource extract { get; set; }

        [JsonProperty("spirit")]
        public StockpileResource spirit { get; set; }

        [JsonProperty("emanation")]
        public StockpileResource emanation { get; set; }

        [JsonProperty("essence")]
        public StockpileResource essence { get; set; }

        public void Clear()
        {
            NextUpdate = 0;
            energy.Clear();
            power.Clear();
            H.Clear();
            O.Clear();
            U.Clear();
            L.Clear();
            K.Clear();
            Z.Clear();
            X.Clear();
            G.Clear();
            silicon.Clear();
            metal.Clear();
            biomass.Clear();
            mist.Clear();
            OH.Clear();
            ZK.Clear();
            UL.Clear();
            UH.Clear();
            UO.Clear();
            KH.Clear();
            KO.Clear();
            LH.Clear();
            LO.Clear();
            ZH.Clear();
            ZO.Clear();
            GH.Clear();
            GO.Clear();
            UH2O.Clear();
            UHO2.Clear();
            KH2O.Clear();
            KHO2.Clear();
            LH2O.Clear();
            LHO2.Clear();
            ZH2O.Clear();
            ZHO2.Clear();
            GH2O.Clear();
            GHO2.Clear();
            XUH2O.Clear();
            XUHO2.Clear();
            XKH2O.Clear();
            XKHO2.Clear();
            XLH2O.Clear();
            XLHO2.Clear();
            XZH2O.Clear();
            XZHO2.Clear();
            XGH2O.Clear();
            XGHO2.Clear();
            ops.Clear();
            utrium_bar.Clear();
            lemergium_bar.Clear();
            zynthium_bar.Clear();
            keanium_bar.Clear();
            ghodium_melt.Clear();
            oxidant.Clear();
            reductant.Clear();
            purifier.Clear();
            battery.Clear();
            composite.Clear();
            crystal.Clear();
            liquid.Clear();
            wire.Clear();
            Switch.Clear();
            transistor.Clear();
            microchip.Clear();
            circuit.Clear();
            device.Clear();
            cell.Clear();
            phlegm.Clear();
            tissue.Clear();
            muscle.Clear();
            organoid.Clear();
            organism.Clear();
            alloy.Clear();
            tube.Clear();
            fixtures.Clear();
            frame.Clear();
            hydraulics.Clear();
            machine.Clear();
            condensate.Clear();
            concentrate.Clear();
            extract.Clear();
            spirit.Clear();
            emanation.Clear();
            essence.Clear();
        }
    }

    public class Spawning
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("needTime")]
        public long NeedTime { get; set; }

        [JsonProperty("spawnTime")]
        public long SpawnTime { get; set; }

        [JsonProperty("directions")]
        public long[] Directions { get; set; } = [];
    }
    #endregion

    #region Parent Base
    public class BaseStructure
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("structureType")]
        public string StructureType { get; set; }
    }

    public class BaseCreep
    {
        [JsonProperty("body")]
        public BodyPart[] Body { get; set; } = [];

        [JsonProperty("store")]
        public Store Store { get; set; }

        [JsonProperty("actionLog")]
        public ActionLog ActionLog { get; set; }

        [JsonProperty("_oldFatigue")]
        public long? _oldFatigue { get; set; }
    }

    public class Creep : BaseCreep
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("storeCapacity")]
        public long? StoreCapacity { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("spawning")]
        public bool Spawning { get; set; }

        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }

        [JsonProperty("ageTime")]
        public long AgeTime { get; set; }

        [JsonProperty("interRoom")]
        public InterRoom InterRoom { get; set; }

        [JsonProperty("fatigue")]
        public long? fatigue { get; set; }

        [JsonProperty("_fatigue")]
        public long? _fatigue { get; set; }

        [JsonProperty("ticksToLive")]
        public long TicksToLive { get; set; }

        [JsonProperty("memory_move")]
        public MemoryMove Memory_move { get; set; }

        [JsonProperty("_attack")]
        public bool? _attack { get; set; }

        [JsonProperty("memory_sourceId")]
        public string Memory_sourceId { get; set; }

        [JsonProperty("strongholdId")]
        public string StrongholdId { get; set; }

        [JsonProperty("_pull")]
        public string _pull { get; set; }

        [JsonProperty("_pulled")]
        public string _pulled { get; set; }

        [JsonProperty("mission")]
        public string Mission { get; set; }

        [JsonProperty("tombstoneDecay")]
        public long TombstoneDecay { get; set; }

        [JsonProperty("noCapacityRecalc")]
        public bool NoCapacityRecalc { get; set; }

        [JsonProperty("noInterShard")]
        public bool noInterShard { get; set; }

        [JsonProperty("_healToApply")]
        public long? _healToApply { get; set; }

        [JsonProperty("_damageToApply")]
        public long? _damageToApply { get; set; }
    }

    public class PowerCreep : BaseCreep
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("className")]
        public string ClassName { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("level")]
        public long Level { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("storeCapacity")]
        public long? StoreCapacity { get; set; }

        [JsonProperty("spawnCooldownTime")]
        public long? SpawnCooldownTime { get; set; }

        [JsonProperty("powers")]
        public Dictionary<string, Power> Powers { get; set; } = new();

        [JsonProperty("shard")]
        public string Shard { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("ageTime")]
        public long AgeTime { get; set; }

        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }

        [JsonProperty("fatigue")]
        public long? Fatigue { get; set; }

        [JsonProperty("_fatigue")]
        public long? _fatigue { get; set; }

        [JsonProperty("deleteTime")]
        public long? DeleteTime { get; set; }

        [JsonProperty("interRoom")]
        public InterRoom InterRoom { get; set; }
    }

    public class GroundResource : Store
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("x")]
        public long x { get; set; }

        [JsonProperty("y")]
        public long y { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("resourceType")]
        public string ResourceType { get; set; }
    }
    #endregion

    #region Structures
    public class StructureConstructionSite : BaseStructure
    {
        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("progress")]
        public long Progress { get; set; }

        [JsonProperty("progressTotal")]
        public long ProgressTotal { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("nextDecayTime")]
        public long? NextDecayTime { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }

        [JsonProperty("store")]
        public Store Store { get; set; }

        [JsonProperty("storeCapacityResource")]
        public Store StoreCapacityResource { get; set; }
    }

    public class StructureTombstone : BaseStructure
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("deathTime")]
        public long DeathTime { get; set; }

        [JsonProperty("decayTime")]
        public long DecayTime { get; set; }

        [JsonProperty("store")]
        public Store Store { get; set; }

        [JsonProperty("tick")]
        public long Tick { get; set; }

        [JsonProperty("creepId")]
        public string CreepId { get; set; }

        [JsonProperty("creepName")]
        public string CreepName { get; set; }

        [JsonProperty("creepSaying")]
        public string CreepSaying { get; set; }

        [JsonProperty("creepTicksToLive")]
        public long? CreepTicksToLive { get; set; }

        [JsonProperty("creepBody")]
        public string[] CreepBody { get; set; } = [];

        [JsonProperty("powerCreepId")]
        public string PowerCreepId { get; set; }

        [JsonProperty("powerCreepName")]
        public string PowerCreepName { get; set; }

        [JsonProperty("powerCreepTicksToLive")]
        public long PowerCreepTicksToLive { get; set; }

        [JsonProperty("powerCreepClassName")]
        public string PowerCreepClassName { get; set; }

        [JsonProperty("powerCreepLevel")]
        public long PowerCreepLevel { get; set; }

        [JsonProperty("powerCreepPowers")]
        public Dictionary<string, Power> PowerCreepPowers { get; set; } = new();

        [JsonProperty("powerCreepSaying")]
        public string PowerCreepSaying { get; set; }
    }

    public class StructureRuin : BaseStructure
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("structure")]
        public RuinStructure Structure { get; set; }

        [JsonProperty("destroyTime")]
        public long DestroyTime { get; set; }

        [JsonProperty("decayTime")]
        public long DecayTime { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("store")]
        public Store Store { get; set; }
    }

    public class StructureDepsoit : BaseStructure
    {
        [JsonProperty("depositType")]
        public string DepositType { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("harvested")]
        public long Harvested { get; set; }

        [JsonProperty("decayTime")]
        public long DecayTime { get; set; }

        [JsonProperty("cooldownTime")]
        public long CooldownTime { get; set; }

        [JsonProperty("_cooldown")]
        public long? _cooldown { get; set; }
    }

    public class StructureMineral : BaseStructure
    {
        [JsonProperty("mineralType")]
        public string MineralType { get; set; }

        [JsonProperty("mineralAmount")]
        public long MineralAmount { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("_updated")]
        public long Updated { get; set; }

        [JsonProperty("density")]
        public long Density { get; set; }

        [JsonProperty("nextRegenerationTime")]
        public long? NextRegenerationTime { get; set; }

        [JsonProperty("effects")]
        public object Effects { get; set; } = new();
    }

    public class StructureSource : BaseStructure
    {
        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("energy")]
        public long Energy { get; set; }

        [JsonProperty("energyCapacity")]
        public long EnergyCapacity { get; set; }

        [JsonProperty("ticksToRegeneration")]
        public long TicksToRegeneration { get; set; }

        [JsonProperty("nextRegenerationTime")]
        public long? NextRegenerationTime { get; set; }

        [JsonProperty("invaderHarvested")]
        public long InvaderHarvested { get; set; }

        [JsonProperty("_updated")]
        public long Updated { get; set; }

        [JsonProperty("effects")]
        public object Effects { get; set; } = new();
    }

    public class StructureSpawn : BaseStructure
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("store")]
        public Store Store { get; set; }

        [JsonProperty("storeCapacityResource")]
        public Store StoreCapacityResource { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("off")]
        public bool Off { get; set; }

        [JsonProperty("_off")]
        public bool _off { get; set; }

        [JsonProperty("spawning")]
        public Spawning Spawning { get; set; }

        [JsonProperty("_updated")]
        public long Updated { get; set; }

        [JsonProperty("effects")]
        public object Effects { get; set; } = new();

        [JsonProperty("tick")]
        public long Tick { get; set; }
    }

    public class StructureExtension : BaseStructure
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("off")]
        public bool Off { get; set; }

        [JsonProperty("_off")]
        public bool _off { get; set; }

        [JsonProperty("_updated")]
        public long Updated { get; set; }

        [JsonProperty("store")]
        public Store Store { get; set; }

        [JsonProperty("storeCapacityResource")]
        public Store StoreCapacityResource { get; set; }
    }

    public class StructureRoad : BaseStructure
    {
        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("nextDecayTime")]
        public long NextDecayTime { get; set; }

        [JsonProperty("strongholdId")]
        public string StrongholdId { get; set; }

        [JsonProperty("decayTime")]
        public long DecayTime { get; set; }

        [JsonProperty("effects")]
        public object Effects { get; set; } = new();

        [JsonProperty("_updated")]
        public long Updated { get; set; }
    }

    public class StructureWall : BaseStructure
    {
        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("progress")]
        public long Progress { get; set; }

        [JsonProperty("progressTotal")]
        public long ProgressTotal { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("_updated")]
        public long Updated { get; set; }

        [JsonProperty("decayTime")]
        public DecayTime DecayTime { get; set; }

        [JsonProperty("effects")]
        public object Effects { get; set; } = new();
    }

    public class StructureRampart : BaseStructure
    {
        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("nextDecayTime")]
        public long NextDecayTime { get; set; }

        [JsonProperty("_updated")]
        public long Updated { get; set; }

        [JsonProperty("strongholdId")]
        public string StrongholdId { get; set; }

        [JsonProperty("decayTime")]
        public long DecayTime { get; set; }

        [JsonProperty("effects")]
        public object Effects { get; set; } = new();

        [JsonProperty("hitsTarget")]
        public long? HitsTarget { get; set; }

        [JsonProperty("isPublic")]
        public bool IsPublic { get; set; }

        [JsonProperty("progress")]
        public long Progress { get; set; }

        [JsonProperty("progressTotal")]
        public long ProgressTotal { get; set; }
    }

    public class StructureKeeperLair : BaseStructure
    {
        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("nextSpawnTime")]
        public long? NextSpawnTime { get; set; }

        [JsonProperty("_updated")]
        public long Updated { get; set; }
    }

    public class StructurePortal : BaseStructure
    {
        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("destination")]
        public Destination Destination { get; set; }

        [JsonProperty("unstableDate")]
        public long? UnstableDate { get; set; }

        [JsonProperty("decayTime")]
        public long DecayTime { get; set; }

        [JsonProperty("tick")]
        public long Tick { get; set; }

        [JsonProperty("disabled")]
        public bool Disabled { get; set; }
    }

    public class StructureController : BaseStructure
    {
        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("level")]
        public long Level { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("progress")]
        public long? Progress { get; set; }

        [JsonProperty("ticksToDowngrade")]
        public long? TicksToDowngrade { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("progressTotal")]
        public long ProgressTotal { get; set; }

        [JsonProperty("downgradeTime")]
        public long? DowngradeTime { get; set; }

        [JsonProperty("reservation")]
        public Reservation Reservation { get; set; }

        [JsonProperty("_updated")]
        public long Updated { get; set; }

        [JsonProperty("sign")]
        public Sign Sign { get; set; }

        [JsonProperty("datetime")]
        public Datetime Datetime { get; set; }

        [JsonProperty("safeModeAvailable")]
        public long? SafeModeAvailable { get; set; }

        [JsonProperty("_safeModeActivated")]
        public long? _safeModeActivated { get; set; }


        [JsonProperty("safeMode")]
        public long? SafeMode { get; set; }

        [JsonProperty("safeModeCooldown")]
        public long? SafeModeCooldown { get; set; }

        [JsonProperty("upgradeBlocked")]
        public long? UpgradeBlocked { get; set; }

        [JsonProperty("isPowerEnabled")]
        public bool? IsPowerEnabled { get; set; }

        [JsonProperty("effects")]
        public object Effects { get; set; } = new();

        [JsonProperty("_upgraded")]
        public long? _upgraded { get; set; }

        [JsonProperty("newField")]
        public long NewField { get; set; }

        [JsonProperty("hardSign")]
        public HardSign HardSign { get; set; }

        [JsonProperty("promoPeriodUntil")]
        public long PromoPeriodUntil { get; set; }

        [JsonProperty("new_field")]
        public long New_field { get; set; }

        [JsonProperty("store")]
        public Store Store { get; set; }

        [JsonProperty("storeCapacityResource")]
        public Store StoreCapacityResource { get; set; }

        [JsonProperty("autoSpawn")]
        public bool? AutoSpawn { get; set; }
    }

    public class StructureLink : BaseStructure
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("cooldown")]
        public long Cooldown { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("actionLog")]
        public ActionLog ActionLog { get; set; }

        [JsonProperty("_updated")]
        public long Updated { get; set; }

        [JsonProperty("store")]
        public Store Store { get; set; }

        [JsonProperty("storeCapacityResource")]
        public Store StoreCapacityResource { get; set; }

        [JsonProperty("_actionLog")]
        public ActionLog _actionLog { get; set; }
    }

    public class StructureStorage : BaseStructure
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("store")]
        public Store Store { get; set; }

        [JsonProperty("storeCapacity")]
        public long? StoreCapacity { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("_updated")]
        public long Updated { get; set; }

        [JsonProperty("effects")]
        public object Effects { get; set; } = new();
    }

    public class StructureTower : BaseStructure
    {
        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("actionLog")]
        public ActionLog ActionLog { get; set; }

        [JsonProperty("_updated")]
        public long Updated { get; set; }

        [JsonProperty("store")]
        public Store Store { get; set; }

        [JsonProperty("storeCapacityResource")]
        public Store StoreCapacityResource { get; set; }

        [JsonProperty("_actionLog")]
        public ActionLog _actionLog { get; set; }

        [JsonProperty("strongholdId")]
        public string StrongholdId { get; set; }

        [JsonProperty("decayTime")]
        public long DecayTime { get; set; }

        [JsonProperty("effects")]
        public object Effects { get; set; } = new();

        [JsonProperty("tick")]
        public long Tick { get; set; }
    }

    public class StructureObserver : BaseStructure
    {
        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("observeRoom")]
        public string ObserveRoom { get; set; }

        [JsonProperty("_updated")]
        public long Updated { get; set; }

        [JsonProperty("effects")]
        public object Effects { get; set; } = new();
    }

    public class StructurePowerBank : BaseStructure
    {
        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("store")]
        public Store Store { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("decayTime")]
        public long DecayTime { get; set; }
    }

    public class StructurePowerSpawn : BaseStructure
    {
        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("store")]
        public Store Store { get; set; }

        [JsonProperty("storeCapacityResource")]
        public Store StoreCapacityResource { get; set; }

        [JsonProperty("effects")]
        public object Effects { get; set; } = new();

        [JsonProperty("_updated")]
        public long Updated { get; set; }
    }

    public class StructureExtractor : BaseStructure
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("_cooldown")]
        public long? _cooldown { get; set; }

        [JsonProperty("cooldown")]
        public long Cooldown { get; set; }

        [JsonProperty("_updated")]
        public long Updated { get; set; }
    }

    public class StructureLab : BaseStructure
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("mineralAmount")]
        public long MineralAmount { get; set; }

        [JsonProperty("cooldown")]
        public long Cooldown { get; set; }

        [JsonProperty("store")]
        public Store Store { get; set; }

        [JsonProperty("storeCapacity")]
        public long? StoreCapacity { get; set; }

        [JsonProperty("storeCapacityResource")]
        public Store StoreCapacityResource { get; set; }

        [JsonProperty("actionLog")]
        public ActionLog ActionLog { get; set; }

        [JsonProperty("_actionLog")]
        public ActionLog _actionLog { get; set; }

        [JsonProperty("_updated")]
        public long Updated { get; set; }

        [JsonProperty("cooldownTime")]
        public long CooldownTime { get; set; }

        [JsonProperty("effects")]
        public object Effects { get; set; } = new();

        [JsonProperty("tick")]
        public string Tick { get; set; }
    }

    public class StructureTerminal : BaseStructure
    {
        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("store")]
        public Store Store { get; set; }

        [JsonProperty("storeCapacity")]
        public long? StoreCapacity { get; set; }

        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("cooldownTime")]
        public long CooldownTime { get; set; }

        [JsonProperty("send")]
        public Send Send { get; set; }

        [JsonProperty("_updated")]
        public long Updated { get; set; }

        [JsonProperty("effects")]
        public object Effects { get; set; } = new();

        [JsonProperty("npc")]
        public bool? Npc { get; set; }

        [JsonProperty("marketData")]
        public MarketData MarketData { get; set; }
    }

    public class StructureContainer : BaseStructure
    {
        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }

        [JsonProperty("store")]
        public Store Store { get; set; }

        [JsonProperty("storeCapacity")]
        public long? StoreCapacity { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("nextDecayTime")]
        public long NextDecayTime { get; set; }

        [JsonProperty("strongholdId")]
        public string StrongholdId { get; set; }

        [JsonProperty("decayTime")]
        public long DecayTime { get; set; }

        [JsonProperty("effects")]
        public object Effects { get; set; } = new();

        [JsonProperty("_updated")]
        public long Updated { get; set; }
    }

    public class StructureNuker : BaseStructure
    {
        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("store")]
        public Store Store { get; set; }

        [JsonProperty("storeCapacityResource")]
        public Store StoreCapacityResource { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("cooldownTime")]
        public long CooldownTime { get; set; }

        [JsonProperty("_updated")]
        public long Updated { get; set; }
    }

    public class StructureFactory : BaseStructure
    {
        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("store")]
        public Store Store { get; set; }

        [JsonProperty("storeCapacity")]
        public long? StoreCapacity { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("cooldown")]
        public long Cooldown { get; set; }

        [JsonProperty("actionLog")]
        public ActionLog ActionLog { get; set; }

        [JsonProperty("cooldownTime")]
        public long CooldownTime { get; set; }

        [JsonProperty("_actionLog")]
        public ActionLog _actionLog { get; set; }

        [JsonProperty("effects")]
        public object Effects { get; set; } = new();

        [JsonProperty("level")]
        public long Level { get; set; }
    }

    public class StructureInvaderCore : BaseStructure
    {
        [JsonProperty()]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("level")]
        public long Level { get; set; }

        [JsonProperty("strongholdBehavior")]
        public string StrongholdBehavior { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("templateName")]
        public string TemplateName { get; set; }

        [JsonProperty("hits")]
        public long? Hits { get; set; }

        [JsonProperty("hitsMax")]
        public long? HitsMax { get; set; }

        [JsonProperty("nextExpandTime")]
        public long NextExpandTime { get; set; }

        [JsonProperty("depositType")]
        public string DepositType { get; set; }

        [JsonProperty("deployTime")]
        public long? DeployTime { get; set; }

        [JsonProperty("strongholdId")]
        public string StrongholdId { get; set; }

        [JsonProperty("effects")]
        public object Effects { get; set; } = new();

        [JsonProperty("actionLog")]
        public ActionLog ActionLog { get; set; }

        [JsonProperty("decayTime")]
        public long DecayTime { get; set; }

        [JsonProperty("_actionLog")]
        public ActionLog _actionLog { get; set; }

        [JsonProperty("population")]
        public Dictionary<string, Population> Population { get; set; } = new();

        [JsonProperty("_spawning")]
        public bool _spawning { get; set; }

        [JsonProperty("spawning")]
        public Spawning Spawning { get; set; }

        [JsonProperty("tick")]
        public string Tick { get; set; }
    }

    public class StructureNuke : BaseStructure
    {
        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("landTime")]
        public long LandTime { get; set; }

        [JsonProperty("launchRoomName")]
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
        public Dictionary<string, StructureNuke> Nukes { get; set; } = new Dictionary<string, StructureNuke>();
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
    }

    #endregion

    public class ScreepsRoomHistory
    {
        public long TimeStamp { get; set; }
        public long Base { get; set; }
        public long Tick { get; set; }

        public Dictionary<string, GroundResource> GroundResources { get; set; } = new Dictionary<string, GroundResource>();
        public Creeps Creeps { get; set; } = new Creeps();
        public Structures Structures { get; set; } = new Structures();

        public Dictionary<string, string> TypeMap { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> UserMap { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, Dictionary<string, object>> PropertiesListDictionary { get; set; } = new Dictionary<string, Dictionary<string, object>>();
    }
}