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
        [JsonProperty("effect")]
        public decimal EffectType { get; set; }
        [JsonProperty("level")]
        public decimal Level { get; set; }
        [JsonProperty("power")]
        public decimal Power { get; set; }
        [JsonProperty("endTime")]
        public decimal EndTime { get; set; }
        [JsonProperty("duration")]
        public decimal Duration { get; set; }
    }
    public class Store
    {
        [JsonProperty(nameof(energy))]
        public decimal? energy { get; set; }
        [JsonProperty(nameof(power))]
        public decimal? power { get; set; }
        [JsonProperty("h")]
        public decimal? H { get; set; }
        [JsonProperty("o")]
        public decimal? O { get; set; }
        [JsonProperty("u")]
        public decimal? U { get; set; }
        [JsonProperty("l")]
        public decimal? L { get; set; }
        [JsonProperty("k")]
        public decimal? K { get; set; }
        [JsonProperty("z")]
        public decimal? Z { get; set; }
        [JsonProperty("x")]
        public decimal? X { get; set; }
        [JsonProperty("g")]
        public decimal? G { get; set; }
        [JsonProperty(nameof(silicon))]
        public decimal? silicon { get; set; }
        [JsonProperty(nameof(metal))]
        public decimal? metal { get; set; }
        [JsonProperty(nameof(biomass))]
        public decimal? biomass { get; set; }
        [JsonProperty(nameof(mist))]
        public decimal? mist { get; set; }
        [JsonProperty("oh")]
        public decimal? OH { get; set; }
        [JsonProperty("zk")]
        public decimal? ZK { get; set; }
        [JsonProperty("ul")]
        public decimal? UL { get; set; }
        [JsonProperty("uh")]
        public decimal? UH { get; set; }
        [JsonProperty("uo")]
        public decimal? UO { get; set; }
        [JsonProperty("kh")]
        public decimal? KH { get; set; }
        [JsonProperty("ko")]
        public decimal? KO { get; set; }
        [JsonProperty("lh")]
        public decimal? LH { get; set; }
        [JsonProperty("lo")]
        public decimal? LO { get; set; }
        [JsonProperty("zh")]
        public decimal? ZH { get; set; }
        [JsonProperty("zo")]
        public decimal? ZO { get; set; }
        [JsonProperty("gh")]
        public decimal? GH { get; set; }
        [JsonProperty("go")]
        public decimal? GO { get; set; }
        [JsonProperty("uh2o")]
        public decimal? UH2O { get; set; }
        [JsonProperty("uho2")]
        public decimal? UHO2 { get; set; }
        [JsonProperty("kh2o")]
        public decimal? KH2O { get; set; }
        [JsonProperty("kho2")]
        public decimal? KHO2 { get; set; }
        [JsonProperty("lh2o")]
        public decimal? LH2O { get; set; }
        [JsonProperty("lho2")]
        public decimal? LHO2 { get; set; }
        [JsonProperty("zh2o")]
        public decimal? ZH2O { get; set; }
        [JsonProperty("zho2")]
        public decimal? ZHO2 { get; set; }
        [JsonProperty("gh2o")]
        public decimal? GH2O { get; set; }
        [JsonProperty("gho2")]
        public decimal? GHO2 { get; set; }
        [JsonProperty("xuh2o")]
        public decimal? XUH2O { get; set; }
        [JsonProperty("xuho2")]
        public decimal? XUHO2 { get; set; }
        [JsonProperty("xkh2o")]
        public decimal? XKH2O { get; set; }
        [JsonProperty("xkho2")]
        public decimal? XKHO2 { get; set; }
        [JsonProperty("xlh2o")]
        public decimal? XLH2O { get; set; }
        [JsonProperty("xlho2")]
        public decimal? XLHO2 { get; set; }
        [JsonProperty("xzh2o")]
        public decimal? XZH2O { get; set; }
        [JsonProperty("xzho2")]
        public decimal? XZHO2 { get; set; }
        [JsonProperty("xgh2o")]
        public decimal? XGH2O { get; set; }
        [JsonProperty("xgho2")]
        public decimal? XGHO2 { get; set; }
        [JsonProperty(nameof(ops))]
        public decimal? ops { get; set; }
        [JsonProperty(nameof(utrium_bar))]
        public decimal? utrium_bar { get; set; }
        [JsonProperty(nameof(lemergium_bar))]
        public decimal? lemergium_bar { get; set; }
        [JsonProperty(nameof(zynthium_bar))]
        public decimal? zynthium_bar { get; set; }
        [JsonProperty(nameof(keanium_bar))]
        public decimal? keanium_bar { get; set; }
        [JsonProperty(nameof(ghodium_melt))]
        public decimal? ghodium_melt { get; set; }
        [JsonProperty(nameof(oxidant))]
        public decimal? oxidant { get; set; }
        [JsonProperty(nameof(reductant))]
        public decimal? reductant { get; set; }
        [JsonProperty(nameof(purifier))]
        public decimal? purifier { get; set; }
        [JsonProperty(nameof(battery))]
        public decimal? battery { get; set; }
        [JsonProperty(nameof(composite))]
        public decimal? composite { get; set; }
        [JsonProperty(nameof(crystal))]
        public decimal? crystal { get; set; }
        [JsonProperty(nameof(liquid))]
        public decimal? liquid { get; set; }
        [JsonProperty(nameof(wire))]
        public decimal? wire { get; set; }
        [JsonProperty("switch")]
        public decimal? Switch { get; set; }
        [JsonProperty(nameof(transistor))]
        public decimal? transistor { get; set; }
        [JsonProperty(nameof(microchip))]
        public decimal? microchip { get; set; }
        [JsonProperty(nameof(circuit))]
        public decimal? circuit { get; set; }
        [JsonProperty(nameof(device))]
        public decimal? device { get; set; }
        [JsonProperty(nameof(cell))]
        public decimal? cell { get; set; }
        [JsonProperty(nameof(phlegm))]
        public decimal? phlegm { get; set; }
        [JsonProperty(nameof(tissue))]
        public decimal? tissue { get; set; }
        [JsonProperty(nameof(muscle))]
        public decimal? muscle { get; set; }
        [JsonProperty(nameof(organoid))]
        public decimal? organoid { get; set; }
        [JsonProperty(nameof(organism))]
        public decimal? organism { get; set; }
        [JsonProperty(nameof(alloy))]
        public decimal? alloy { get; set; }
        [JsonProperty(nameof(tube))]
        public decimal? tube { get; set; }
        [JsonProperty(nameof(fixtures))]
        public decimal? fixtures { get; set; }
        [JsonProperty(nameof(frame))]
        public decimal? frame { get; set; }
        [JsonProperty(nameof(hydraulics))]
        public decimal? hydraulics { get; set; }
        [JsonProperty(nameof(machine))]
        public decimal? machine { get; set; }
        [JsonProperty(nameof(condensate))]
        public decimal? condensate { get; set; }
        [JsonProperty(nameof(concentrate))]
        public decimal? concentrate { get; set; }
        [JsonProperty(nameof(extract))]
        public decimal? extract { get; set; }
        [JsonProperty(nameof(spirit))]
        public decimal? spirit { get; set; }
        [JsonProperty(nameof(emanation))]
        public decimal? emanation { get; set; }
        [JsonProperty(nameof(essence))]
        public decimal? essence { get; set; }
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
        public decimal EndTime { get; set; }
    }
    public class Datetime
    {
        [JsonProperty("sign")]
        public decimal Sign { get; set; }
    }
    public class Sign
    {
        [JsonProperty("user")]
        public string User { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("time")]
        public decimal Time { get; set; }
        [JsonProperty("datetime")]
        public decimal Datetime { get; set; }
    }
    public class HardSign
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("time")]
        public decimal Time { get; set; }
        [JsonProperty("datetime")]
        public decimal Datetime { get; set; }
        [JsonProperty("endDatetime")]
        public decimal EndDatetime { get; set; }
    }
    public class BodyPart
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("hits")]
        public decimal Hits { get; set; }
        [JsonProperty("boost")]
        public string Boost { get; set; }
    }
    public class ReactionBase
    {
        [JsonProperty("x1")]
        public decimal X1 { get; set; }
        [JsonProperty("y1")]
        public decimal Y1 { get; set; }
        [JsonProperty("x2")]
        public decimal X2 { get; set; }
        [JsonProperty("y2")]
        public decimal Y2 { get; set; }
    }
    public class RunReaction : ReactionBase { }
    public class ReverseReaction : ReactionBase { }
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
        public decimal Id { get; set; }
        [JsonProperty("x")]
        public decimal X { get; set; }
        [JsonProperty("y")]
        public decimal Y { get; set; }
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
        public decimal X { get; set; }
        [JsonProperty("y")]
        public decimal Y { get; set; }
    }
    public class MemoryMove
    {
        [JsonProperty("dest")]
        public string Dest { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("time")]
        public decimal Time { get; set; }
        [JsonProperty("lastMove")]
        public decimal LastMove { get; set; }
    }
    public class InterRoom
    {
        [JsonProperty("room")]
        public string Room { get; set; }
        [JsonProperty("x")]
        public decimal X { get; set; }
        [JsonProperty("y")]
        public decimal Y { get; set; }
    }
    public class Destination
    {
        [JsonProperty("room")]
        public string Room { get; set; }
        [JsonProperty("shard")]
        public string Shard { get; set; }
        [JsonProperty("x")]
        public decimal X { get; set; }
        [JsonProperty("y")]
        public decimal Y { get; set; }
    }
    public class Power
    {
        [JsonProperty("level")]
        public decimal Level { get; set; }
        [JsonProperty("cooldownTime")]
        public decimal CooldownTime { get; set; }
    }
    public class RuinStructure
    {
        [JsonProperty("_id")]
        public string Id { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("hits")]
        public decimal Hits { get; set; }
        [JsonProperty("hitsMax")]
        public decimal HitsMax { get; set; }
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
        public decimal Amount { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }
    public class StockpileResource
    {
        [JsonProperty("stockpiled")]
        public decimal Stockpiled { get; set; }
        [JsonProperty("stockpileUseTick")]
        public decimal StockpileUseTick { get; set; }
        public void Clear()
        {
            Stockpiled = 0;
            StockpileUseTick = 0;
        }
    }
    public class MarketData
    {
        [JsonProperty("nextUpdate")]
        public decimal NextUpdate { get; set; }
        [JsonProperty(nameof(energy))]
        public StockpileResource energy { get; set; }
        [JsonProperty(nameof(power))]
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
        [JsonProperty(nameof(silicon))]
        public StockpileResource silicon { get; set; }
        [JsonProperty(nameof(metal))]
        public StockpileResource metal { get; set; }
        [JsonProperty(nameof(biomass))]
        public StockpileResource biomass { get; set; }
        [JsonProperty(nameof(mist))]
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
        [JsonProperty(nameof(ops))]
        public StockpileResource ops { get; set; }
        [JsonProperty(nameof(utrium_bar))]
        public StockpileResource utrium_bar { get; set; }
        [JsonProperty(nameof(lemergium_bar))]
        public StockpileResource lemergium_bar { get; set; }
        [JsonProperty(nameof(zynthium_bar))]
        public StockpileResource zynthium_bar { get; set; }
        [JsonProperty(nameof(keanium_bar))]
        public StockpileResource keanium_bar { get; set; }
        [JsonProperty(nameof(ghodium_melt))]
        public StockpileResource ghodium_melt { get; set; }
        [JsonProperty(nameof(oxidant))]
        public StockpileResource oxidant { get; set; }
        [JsonProperty(nameof(reductant))]
        public StockpileResource reductant { get; set; }
        [JsonProperty(nameof(purifier))]
        public StockpileResource purifier { get; set; }
        [JsonProperty(nameof(battery))]
        public StockpileResource battery { get; set; }
        [JsonProperty(nameof(composite))]
        public StockpileResource composite { get; set; }
        [JsonProperty(nameof(crystal))]
        public StockpileResource crystal { get; set; }
        [JsonProperty(nameof(liquid))]
        public StockpileResource liquid { get; set; }
        [JsonProperty(nameof(wire))]
        public StockpileResource wire { get; set; }
        [JsonProperty("switch")]
        public StockpileResource Switch { get; set; }
        [JsonProperty(nameof(transistor))]
        public StockpileResource transistor { get; set; }
        [JsonProperty(nameof(microchip))]
        public StockpileResource microchip { get; set; }
        [JsonProperty(nameof(circuit))]
        public StockpileResource circuit { get; set; }
        [JsonProperty(nameof(device))]
        public StockpileResource device { get; set; }
        [JsonProperty(nameof(cell))]
        public StockpileResource cell { get; set; }
        [JsonProperty(nameof(phlegm))]
        public StockpileResource phlegm { get; set; }
        [JsonProperty(nameof(tissue))]
        public StockpileResource tissue { get; set; }
        [JsonProperty(nameof(muscle))]
        public StockpileResource muscle { get; set; }
        [JsonProperty(nameof(organoid))]
        public StockpileResource organoid { get; set; }
        [JsonProperty(nameof(organism))]
        public StockpileResource organism { get; set; }
        [JsonProperty(nameof(alloy))]
        public StockpileResource alloy { get; set; }
        [JsonProperty(nameof(tube))]
        public StockpileResource tube { get; set; }
        [JsonProperty(nameof(fixtures))]
        public StockpileResource fixtures { get; set; }
        [JsonProperty(nameof(frame))]
        public StockpileResource frame { get; set; }
        [JsonProperty(nameof(hydraulics))]
        public StockpileResource hydraulics { get; set; }
        [JsonProperty(nameof(machine))]
        public StockpileResource machine { get; set; }
        [JsonProperty(nameof(condensate))]
        public StockpileResource condensate { get; set; }
        [JsonProperty(nameof(concentrate))]
        public StockpileResource concentrate { get; set; }
        [JsonProperty(nameof(extract))]
        public StockpileResource extract { get; set; }
        [JsonProperty(nameof(spirit))]
        public StockpileResource spirit { get; set; }
        [JsonProperty(nameof(emanation))]
        public StockpileResource emanation { get; set; }
        [JsonProperty(nameof(essence))]
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
        public decimal NeedTime { get; set; }
        [JsonProperty("spawnTime")]
        public decimal SpawnTime { get; set; }
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
        [JsonProperty("progress")]
        public decimal Progress { get; set; }
        [JsonProperty("progressTotal")]
        public decimal ProgressTotal { get; set; }
    }

    public class PositionedStructure : BaseStructure
    {
        [JsonProperty("x")]
        public decimal X { get; set; }

        [JsonProperty("y")]
        public decimal Y { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }
    }

    public class OwnedStructure : PositionedStructure
    {
        [JsonProperty("user")]
        public string User { get; set; }
    }

    public class DurableStructure : OwnedStructure
    {
        [JsonProperty("hits")]
        public decimal Hits { get; set; }

        [JsonProperty("hitsMax")]
        public decimal HitsMax { get; set; }

        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }
    }

    public class StoredStructure : DurableStructure
    {
        [JsonProperty("store")]
        public Store Store { get; set; } = new();

        [JsonProperty("storeCapacityResource")]
        public Store StoreCapacityResource { get; set; } = new();
    }

    public class BaseCreep
    {
        [JsonProperty("_id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("user")]
        public string User { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("room")]
        public string Room { get; set; }
        [JsonProperty("hits")]
        public decimal Hits { get; set; }
        [JsonProperty("hitsMax")]
        public decimal HitsMax { get; set; }
        [JsonProperty("storeCapacity")]
        public decimal StoreCapacity { get; set; }
        [JsonProperty("ageTime")]
        public decimal AgeTime { get; set; }
        [JsonProperty("notifyWhenAttacked")]
        public bool NotifyWhenAttacked { get; set; }
        [JsonProperty("fatigue")]
        public decimal Fatigue { get; set; }
        [JsonProperty(nameof(_fatigue))]
        public decimal _fatigue { get; set; }
        [JsonProperty("interRoom")]
        public InterRoom InterRoom { get; set; }

        [JsonProperty("body")]
        public BodyPart[] Body { get; set; } = [];
        [JsonProperty("store")]
        public Store Store { get; set; }
        [JsonProperty("actionLog")]
        public ActionLog ActionLog { get; set; }
        [JsonProperty("x")]
        public decimal X { get; set; }
        [JsonProperty("y")]
        public decimal Y { get; set; }
        [JsonProperty(nameof(_oldFatigue))]
        public decimal _oldFatigue { get; set; }
    }

    public class Creep : BaseCreep
    {
        [JsonProperty("spawning")]
        public bool Spawning { get; set; }
        [JsonProperty("memory_move")]
        public MemoryMove Memory_move { get; set; }
        [JsonProperty(nameof(_attack))]
        public bool _attack { get; set; }
        [JsonProperty("memory_sourceId")]
        public string Memory_sourceId { get; set; }
        [JsonProperty("strongholdId")]
        public string StrongholdId { get; set; }
        [JsonProperty(nameof(_pull))]
        public string _pull { get; set; }
        [JsonProperty(nameof(_pulled))]
        public string _pulled { get; set; }
        [JsonProperty("mission")]
        public string Mission { get; set; }
        [JsonProperty("tombstoneDecay")]
        public decimal TombstoneDecay { get; set; }
        [JsonProperty("noCapacityRecalc")]
        public bool NoCapacityRecalc { get; set; }
        [JsonProperty("noInterShard")]
        public bool NoInterShard { get; set; }
        [JsonProperty(nameof(_healToApply))]
        public decimal _healToApply { get; set; }
        [JsonProperty(nameof(_damageToApply))]
        public decimal _damageToApply { get; set; }
        [JsonProperty("ticksToLive")]
        public decimal TicksToLive { get; set; }
        [JsonProperty("userSummoned")]
        public string? UserSummoned { get; set; }
    }

    public class PowerCreep : BaseCreep
    {
        [JsonProperty("className")]
        public string ClassName { get; set; }
        [JsonProperty("level")]
        public decimal Level { get; set; }
        [JsonProperty("spawnCooldownTime")]
        public decimal SpawnCooldownTime { get; set; }
        [JsonProperty("powers")]
        public Dictionary<string, Power> Powers { get; set; }
        [JsonProperty("shard")]
        public string Shard { get; set; }
        [JsonProperty("deleteTime")]
        public decimal DeleteTime { get; set; }
    }

    public class GroundResource : Store
    {
        [JsonProperty("_id")]
        public string Id { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty(nameof(x))]
        public decimal x { get; set; }
        [JsonProperty(nameof(y))]
        public decimal y { get; set; }
        [JsonProperty("room")]
        public string Room { get; set; }
        [JsonProperty("resourceType")]
        public string ResourceType { get; set; }
    }
    #endregion
    #region Structures
    public class StructureConstructionSite : DurableStructure
    {
        [JsonProperty("nextDecayTime")]
        public decimal NextDecayTime { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class StructureTombstone : PositionedStructure
    {
        [JsonProperty("user")]
        public string User { get; set; }
        [JsonProperty("deathTime")]
        public decimal DeathTime { get; set; }
        [JsonProperty("decayTime")]
        public decimal DecayTime { get; set; }
        [JsonProperty("store")]
        public Store Store { get; set; }
        [JsonProperty("tick")]
        public decimal Tick { get; set; }
        [JsonProperty("creepId")]
        public string CreepId { get; set; }
        [JsonProperty("creepName")]
        public string CreepName { get; set; }
        [JsonProperty("creepSaying")]
        public string CreepSaying { get; set; }
        [JsonProperty("creepTicksToLive")]
        public decimal CreepTicksToLive { get; set; }
        [JsonProperty("creepBody")]
        public string[] CreepBody { get; set; } = [];
        [JsonProperty("powerCreepId")]
        public string PowerCreepId { get; set; }
        [JsonProperty("powerCreepName")]
        public string PowerCreepName { get; set; }
        [JsonProperty("powerCreepTicksToLive")]
        public decimal PowerCreepTicksToLive { get; set; }
        [JsonProperty("powerCreepClassName")]
        public string PowerCreepClassName { get; set; }
        [JsonProperty("powerCreepLevel")]
        public decimal PowerCreepLevel { get; set; }
        [JsonProperty("powerCreepPowers")]
        public Dictionary<string, Power> PowerCreepPowers { get; set; }
        [JsonProperty("powerCreepSaying")]
        public string PowerCreepSaying { get; set; }
    }

    public class StructureRuin : PositionedStructure
    {
        [JsonProperty("structure")]
        public RuinStructure Structure { get; set; }
        [JsonProperty("destroyTime")]
        public decimal DestroyTime { get; set; }
        [JsonProperty("decayTime")]
        public decimal DecayTime { get; set; }
        [JsonProperty("user")]
        public string User { get; set; }
        [JsonProperty("store")]
        public Store Store { get; set; }
    }

    public class StructureExtension : StoredStructure
    {
        [JsonProperty("off")]
        public bool Off { get; set; }
        [JsonProperty(nameof(_off))]
        public bool _off { get; set; }
        [JsonProperty("_updated")]
        public decimal Updated { get; set; }
    }

    public class StructureSpawn : StoredStructure
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("off")]
        public bool Off { get; set; }
        [JsonProperty(nameof(_off))]
        public bool _off { get; set; }
        [JsonProperty("spawning")]
        public Spawning Spawning { get; set; }
        [JsonProperty("_updated")]
        public decimal Updated { get; set; }
        [JsonProperty("effects")]
        public Dictionary<string, Effect> Effects { get; set; }
        [JsonProperty("tick")]
        public decimal Tick { get; set; }
    }

    public class StructureTower : StoredStructure
    {
        [JsonProperty("actionLog")]
        public ActionLog ActionLog { get; set; }
        [JsonProperty("_updated")]
        public decimal Updated { get; set; }
        [JsonProperty(nameof(_actionLog))]
        public ActionLog _actionLog { get; set; }
        [JsonProperty("strongholdId")]
        public string StrongholdId { get; set; }
        [JsonProperty("decayTime")]
        public decimal DecayTime { get; set; }
        [JsonProperty("effects")]
        public Dictionary<string, Effect> Effects { get; set; }
        [JsonProperty("tick")]
        public decimal Tick { get; set; }
    }

    public class StructureLink : StoredStructure
    {
        [JsonProperty("cooldown")]
        public decimal Cooldown { get; set; }
        [JsonProperty("actionLog")]
        public ActionLog ActionLog { get; set; }
        [JsonProperty(nameof(_actionLog))]
        public ActionLog _actionLog { get; set; }
        [JsonProperty("_updated")]
        public decimal Updated { get; set; }
    }

    public class StructureStorage : StoredStructure
    {
        [JsonProperty("storeCapacity")]
        public decimal StoreCapacity { get; set; }
        [JsonProperty("_updated")]
        public decimal Updated { get; set; }
        [JsonProperty("effects")]
        public Dictionary<string, Effect> Effects { get; set; }
    }

    public class StructureContainer : StoredStructure
    {
        [JsonProperty("storeCapacity")]
        public decimal StoreCapacity { get; set; }
        [JsonProperty("nextDecayTime")]
        public decimal NextDecayTime { get; set; }
        [JsonProperty("strongholdId")]
        public string StrongholdId { get; set; }
        [JsonProperty("decayTime")]
        public decimal DecayTime { get; set; }
        [JsonProperty("_updated")]
        public decimal Updated { get; set; }
        [JsonProperty("effects")]
        public Dictionary<string, Effect> Effects { get; set; }
    }

    public class StructureRoad : DurableStructure
    {
        [JsonProperty("nextDecayTime")]
        public decimal NextDecayTime { get; set; }
        [JsonProperty("strongholdId")]
        public string StrongholdId { get; set; }
        [JsonProperty("decayTime")]
        public decimal DecayTime { get; set; }
        [JsonProperty("_updated")]
        public decimal Updated { get; set; }
        [JsonProperty("effects")]
        public Dictionary<string, Effect> Effects { get; set; }
    }

    public class StructureRampart : DurableStructure
    {
        [JsonProperty("nextDecayTime")]
        public decimal NextDecayTime { get; set; }
        [JsonProperty("strongholdId")]
        public string StrongholdId { get; set; }
        [JsonProperty("decayTime")]
        public decimal DecayTime { get; set; }
        [JsonProperty("_updated")]
        public decimal Updated { get; set; }
        [JsonProperty("effects")]
        public Dictionary<string, Effect> Effects { get; set; }
        [JsonProperty("hitsTarget")]
        public decimal HitsTarget { get; set; }
        [JsonProperty("isPublic")]
        public bool IsPublic { get; set; }
        [JsonProperty("progress")]
        public decimal Progress { get; set; }
        [JsonProperty("progressTotal")]
        public decimal ProgressTotal { get; set; }
    }

    public class StructureObserver : DurableStructure
    {
        [JsonProperty("observeRoom")]
        public string ObserveRoom { get; set; }
        [JsonProperty("_updated")]
        public decimal Updated { get; set; }
        [JsonProperty("effects")]
        public Dictionary<string, Effect> Effects { get; set; }
    }

    public class StructureController : DurableStructure
    {
        [JsonProperty("level")]
        public decimal Level { get; set; }
        [JsonProperty("progress")]
        public decimal Progress { get; set; }
        [JsonProperty("progressTotal")]
        public decimal ProgressTotal { get; set; }
        [JsonProperty("ticksToDowngrade")]
        public decimal TicksToDowngrade { get; set; }
        [JsonProperty("downgradeTime")]
        public decimal DowngradeTime { get; set; }
        [JsonProperty("reservation")]
        public Reservation Reservation { get; set; }
        [JsonProperty("sign")]
        public Sign Sign { get; set; }
        [JsonProperty("datetime")]
        public Datetime Datetime { get; set; }
        [JsonProperty("safeModeAvailable")]
        public decimal SafeModeAvailable { get; set; }
        [JsonProperty(nameof(_safeModeActivated))]
        public decimal _safeModeActivated { get; set; }
        [JsonProperty("safeMode")]
        public decimal SafeMode { get; set; }
        [JsonProperty("safeModeCooldown")]
        public decimal SafeModeCooldown { get; set; }
        [JsonProperty("upgradeBlocked")]
        public decimal UpgradeBlocked { get; set; }
        [JsonProperty("isPowerEnabled")]
        public bool IsPowerEnabled { get; set; }
        [JsonProperty("effects")]
        public Dictionary<string, Effect> Effects { get; set; }
        [JsonProperty(nameof(_upgraded))]
        public decimal _upgraded { get; set; }
        [JsonProperty("newField")]
        public decimal NewField { get; set; }
        [JsonProperty("hardSign")]
        public HardSign HardSign { get; set; }
        [JsonProperty("promoPeriodUntil")]
        public decimal PromoPeriodUntil { get; set; }
        [JsonProperty("new_field")]
        public decimal New_field { get; set; }
        [JsonProperty("store")]
        public Store Store { get; set; }
        [JsonProperty("storeCapacityResource")]
        public Store StoreCapacityResource { get; set; }
        [JsonProperty("autoSpawn")]
        public bool AutoSpawn { get; set; }
        [JsonProperty("_updated")]
        public decimal Updated { get; set; }
    }

    public class StructureLab : StoredStructure
    {
        [JsonProperty("mineralAmount")]
        public decimal MineralAmount { get; set; }
        [JsonProperty("cooldown")]
        public decimal Cooldown { get; set; }
        [JsonProperty("storeCapacity")]
        public decimal StoreCapacity { get; set; }
        [JsonProperty("actionLog")]
        public ActionLog ActionLog { get; set; }
        [JsonProperty(nameof(_actionLog))]
        public ActionLog _actionLog { get; set; }
        [JsonProperty("_updated")]
        public decimal Updated { get; set; }
        [JsonProperty("cooldownTime")]
        public decimal CooldownTime { get; set; }
        [JsonProperty("effects")]
        public Dictionary<string, Effect> Effects { get; set; }
        [JsonProperty("tick")]
        public string Tick { get; set; }
    }

    public class StructureNuke : PositionedStructure
    {
        [JsonProperty("landTime")]
        public decimal LandTime { get; set; }
        [JsonProperty("launchRoomName")]
        public string LaunchRoomName { get; set; }
    }

    public class StructureFactory : StoredStructure
    {
        [JsonProperty("storeCapacity")]
        public decimal StoreCapacity { get; set; }
        [JsonProperty("cooldown")]
        public decimal Cooldown { get; set; }
        [JsonProperty("actionLog")]
        public ActionLog ActionLog { get; set; }
        [JsonProperty("cooldownTime")]
        public decimal CooldownTime { get; set; }
        [JsonProperty(nameof(_actionLog))]
        public ActionLog _actionLog { get; set; }
        [JsonProperty("effects")]
        public Dictionary<string, Effect> Effects { get; set; }
        [JsonProperty("level")]
        public decimal Level { get; set; }
    }

    public class StructurePowerBank : PositionedStructure
    {
        [JsonProperty("store")]
        public Store Store { get; set; }
        [JsonProperty("hits")]
        public decimal Hits { get; set; }
        [JsonProperty("hitsMax")]
        public decimal HitsMax { get; set; }
        [JsonProperty("decayTime")]
        public decimal DecayTime { get; set; }
    }

    public class StructurePowerSpawn : StoredStructure
    {
        [JsonProperty("effects")]
        public Dictionary<string, Effect> Effects { get; set; }
        [JsonProperty("_updated")]
        public decimal Updated { get; set; }
    }

    public class StructureTerminal : StoredStructure
    {
        [JsonProperty("storeCapacity")]
        public decimal StoreCapacity { get; set; }
        [JsonProperty("cooldownTime")]
        public decimal CooldownTime { get; set; }
        [JsonProperty("send")]
        public Send Send { get; set; }
        [JsonProperty("_updated")]
        public decimal Updated { get; set; }
        [JsonProperty("effects")]
        public Dictionary<string, Effect> Effects { get; set; }
        [JsonProperty("npc")]
        public bool Npc { get; set; }
        [JsonProperty("marketData")]
        public MarketData MarketData { get; set; }
    }

    public class StructureInvaderCore : StoredStructure
    {
        [JsonProperty("level")]
        public decimal Level { get; set; }
        [JsonProperty("strongholdBehavior")]
        public string StrongholdBehavior { get; set; }
        [JsonProperty("templateName")]
        public string TemplateName { get; set; }
        [JsonProperty("nextExpandTime")]
        public decimal NextExpandTime { get; set; }
        [JsonProperty("depositType")]
        public string DepositType { get; set; }
        [JsonProperty("deployTime")]
        public decimal DeployTime { get; set; }
        [JsonProperty("strongholdId")]
        public string StrongholdId { get; set; }
        [JsonProperty("effects")]
        public Dictionary<string, Effect> Effects { get; set; }
        [JsonProperty("actionLog")]
        public ActionLog ActionLog { get; set; }
        [JsonProperty("decayTime")]
        public decimal DecayTime { get; set; }
        [JsonProperty(nameof(_actionLog))]
        public ActionLog _actionLog { get; set; }
        [JsonProperty("population")]
        public Dictionary<string, Population> Population { get; set; }
        [JsonProperty(nameof(_spawning))]
        public bool _spawning { get; set; }
        [JsonProperty("spawning")]
        public Spawning Spawning { get; set; }
        [JsonProperty("tick")]
        public string Tick { get; set; }
    }
    public class StructureWall : DurableStructure
    {
        [JsonProperty("progress")]
        public decimal Progress { get; set; }
        [JsonProperty("progressTotal")]
        public decimal ProgressTotal { get; set; }
        [JsonProperty("_updated")]
        public decimal Updated { get; set; }
        [JsonProperty("decayTime")]
        public DecayTime DecayTime { get; set; }
        [JsonProperty("effects")]
        public Dictionary<string, Effect> Effects { get; set; }
    }

    public class StructureSource : PositionedStructure
    {
        [JsonProperty("energy")]
        public decimal Energy { get; set; }
        [JsonProperty("energyCapacity")]
        public decimal EnergyCapacity { get; set; }
        [JsonProperty("ticksToRegeneration")]
        public decimal TicksToRegeneration { get; set; }
        [JsonProperty("nextRegenerationTime")]
        public decimal NextRegenerationTime { get; set; }
        [JsonProperty("invaderHarvested")]
        public decimal InvaderHarvested { get; set; }
        [JsonProperty("_updated")]
        public decimal Updated { get; set; }
        [JsonProperty("effects")]
        public Dictionary<string, Effect> Effects { get; set; }
    }

    public class StructurePortal : PositionedStructure
    {
        [JsonProperty("destination")]
        public Destination Destination { get; set; }
        [JsonProperty("unstableDate")]
        public decimal UnstableDate { get; set; }
        [JsonProperty("decayTime")]
        public decimal DecayTime { get; set; }
        [JsonProperty("tick")]
        public decimal Tick { get; set; }
        [JsonProperty("disabled")]
        public bool Disabled { get; set; }
    }

    public class StructureMineral : PositionedStructure
    {
        [JsonProperty("mineralType")]
        public string MineralType { get; set; }
        [JsonProperty("mineralAmount")]
        public decimal MineralAmount { get; set; }
        [JsonProperty("_updated")]
        public decimal Updated { get; set; }
        [JsonProperty("density")]
        public decimal Density { get; set; }
        [JsonProperty("nextRegenerationTime")]
        public decimal NextRegenerationTime { get; set; }
        [JsonProperty("effects")]
        public Dictionary<string, Effect> Effects { get; set; }
    }

    public class StructureExtractor : DurableStructure
    {
        [JsonProperty(nameof(_cooldown))]
        public decimal _cooldown { get; set; }
        [JsonProperty("cooldown")]
        public decimal Cooldown { get; set; }
        [JsonProperty("_updated")]
        public decimal Updated { get; set; }
    }

    public class StructureKeeperLair : PositionedStructure
    {
        [JsonProperty("nextSpawnTime")]
        public decimal NextSpawnTime { get; set; }
        [JsonProperty("_updated")]
        public decimal Updated { get; set; }
    }
    public class StructureNuker : StoredStructure
    {
        [JsonProperty("cooldownTime")]
        public decimal CooldownTime { get; set; }
        [JsonProperty("_updated")]
        public decimal Updated { get; set; }
    }
    public class StructureDeposit : PositionedStructure
    {
        [JsonProperty("depositType")]
        public string DepositType { get; set; }
        [JsonProperty("harvested")]
        public decimal Harvested { get; set; }
        [JsonProperty("decayTime")]
        public decimal DecayTime { get; set; }
        [JsonProperty("cooldownTime")]
        public decimal CooldownTime { get; set; }
        [JsonProperty(nameof(_cooldown))]
        public decimal _cooldown { get; set; }
        [JsonProperty("_updated")]
        public decimal Updated { get; set; }
    }
    #endregion
    #region Groups
    public class Creeps
    {
        public Dictionary<string, Creep> OwnedCreeps { get; set; } =
            new Dictionary<string, Creep>();
        public Dictionary<string, Creep> EnemyCreeps { get; set; } =
            new Dictionary<string, Creep>();
        public Dictionary<string, Creep> OtherCreeps { get; set; } =
            new Dictionary<string, Creep>();
        public Dictionary<string, PowerCreep> PowerCreeps { get; set; } =
            new Dictionary<string, PowerCreep>();
    }
    public class Structures
    {
        public StructureController? Controller { get; set; }
        public StructureMineral? Mineral { get; set; }
        public Dictionary<string, StructureDeposit> Deposits { get; set; } =
            new Dictionary<string, StructureDeposit>();
        public Dictionary<string, StructureWall> Walls { get; set; } =
            new Dictionary<string, StructureWall>();
        public Dictionary<string, StructureConstructionSite> ConstructionSites { get; set; } =
            new Dictionary<string, StructureConstructionSite>();
        public Dictionary<string, StructureContainer> Containers { get; set; } =
            new Dictionary<string, StructureContainer>();
        public Dictionary<string, StructureExtension> Extensions { get; set; } =
            new Dictionary<string, StructureExtension>();
        public Dictionary<string, StructureExtractor> Extractors { get; set; } =
            new Dictionary<string, StructureExtractor>();
        public Dictionary<string, StructureFactory> Factories { get; set; } =
            new Dictionary<string, StructureFactory>();
        public Dictionary<string, StructureInvaderCore> InvaderCores { get; set; } =
            new Dictionary<string, StructureInvaderCore>();
        public Dictionary<string, StructureKeeperLair> KeeperLairs { get; set; } =
            new Dictionary<string, StructureKeeperLair>();
        public Dictionary<string, StructureLab> Labs { get; set; } =
            new Dictionary<string, StructureLab>();
        public Dictionary<string, StructureLink> Links { get; set; } =
            new Dictionary<string, StructureLink>();
        public Dictionary<string, StructureNuke> Nukes { get; set; } =
            new Dictionary<string, StructureNuke>();
        public Dictionary<string, StructureNuker> Nukers { get; set; } =
            new Dictionary<string, StructureNuker>();
        public Dictionary<string, StructureObserver> Observers { get; set; } =
            new Dictionary<string, StructureObserver>();
        public Dictionary<string, StructurePortal> Portals { get; set; } =
            new Dictionary<string, StructurePortal>();
        public Dictionary<string, StructurePowerBank> PowerBanks { get; set; } =
            new Dictionary<string, StructurePowerBank>();
        public Dictionary<string, StructurePowerSpawn> PowerSpawns { get; set; } =
            new Dictionary<string, StructurePowerSpawn>();
        public Dictionary<string, StructureRampart> Ramparts { get; set; } =
            new Dictionary<string, StructureRampart>();
        public Dictionary<string, StructureRoad> Roads { get; set; } =
            new Dictionary<string, StructureRoad>();
        public Dictionary<string, StructureRuin> Ruins { get; set; } =
            new Dictionary<string, StructureRuin>();
        public Dictionary<string, StructureSource> Sources { get; set; } =
            new Dictionary<string, StructureSource>();
        public Dictionary<string, StructureSpawn> Spawns { get; set; } =
            new Dictionary<string, StructureSpawn>();
        public Dictionary<string, StructureStorage> Storages { get; set; } =
            new Dictionary<string, StructureStorage>();
        public Dictionary<string, StructureTerminal> Terminals { get; set; } =
            new Dictionary<string, StructureTerminal>();
        public Dictionary<string, StructureTombstone> Tombstones { get; set; } =
            new Dictionary<string, StructureTombstone>();
        public Dictionary<string, StructureTower> Towers { get; set; } =
            new Dictionary<string, StructureTower>();
    }
    #endregion
    public class ScreepsRoomHistory
    {
        public long TimeStamp { get; set; }
        public long Base { get; set; }
        public long Tick { get; set; }
        public Dictionary<string, GroundResource> GroundResources { get; set; } =
            new Dictionary<string, GroundResource>();
        public Creeps Creeps { get; set; } = new Creeps();
        public Structures Structures { get; set; } = new Structures();
        public Dictionary<string, string> TypeMap { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> UserMap { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, Dictionary<string, object?>>? HistoryChangesDictionary { get; set; }
    }
}
