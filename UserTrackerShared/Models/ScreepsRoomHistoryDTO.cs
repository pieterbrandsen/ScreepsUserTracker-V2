using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UserTrackerShared.Helpers;

namespace UserTrackerShared.Models
{
    #region Base DTO
    public class DamageActionDTO
    {
        public long Count { get; set; } = 0;
        public long Damage { get; set; } = 0;
    }
    public class HealActionDTO
    {
        public long Count { get; set; } = 0;
        public long Heal { get; set; } = 0;
    }
    public class InflowActionDTO
    {
        public long Count { get; set; } = 0;
        public long Inflow { get; set; } = 0;
    }
    public class OutlflowActionDTO
    {
        public long Count { get; set; } = 0;
        public long Outlflow { get; set; } = 0;
        public long Effect { get; set; } = 0;
    }
    public class GenericActionDTO
    {
        public long Count { get; set; } = 0;
    }
    public class ActionLogDTO
    {
        public DamageActionDTO Attacked { get; set; } = new DamageActionDTO();
        public DamageActionDTO Attack { get; set; } = new DamageActionDTO();
        public DamageActionDTO RangedAttack { get; set; } = new DamageActionDTO();
        public DamageActionDTO RangedMassAttack { get; set; } = new DamageActionDTO();
        public HealActionDTO RangedHeal { get; set; } = new HealActionDTO();
        public HealActionDTO Healed { get; set; } = new HealActionDTO();
        public HealActionDTO Heal { get; set; } = new HealActionDTO(); 
        public InflowActionDTO Harvest { get; set; } = new InflowActionDTO(); 
        public OutlflowActionDTO Repair { get; set; } = new OutlflowActionDTO(); 
        public OutlflowActionDTO Build { get; set; } = new OutlflowActionDTO();
        public OutlflowActionDTO UpgradeController { get; set; } = new OutlflowActionDTO();
        public GenericActionDTO Say { get; set; } = new GenericActionDTO();
        public GenericActionDTO ReserveController { get; set; } = new GenericActionDTO();
        public GenericActionDTO Produce { get; set; } = new GenericActionDTO();
        public GenericActionDTO TransferEnergy { get; set; } = new GenericActionDTO();
        public GenericActionDTO AttackController { get; set; } = new GenericActionDTO();
        public GenericActionDTO RunReaction { get; set; } = new GenericActionDTO();
        public GenericActionDTO ReverseReaction { get; set; } = new GenericActionDTO();
        public GenericActionDTO Spawned { get; set; } = new GenericActionDTO();
        public GenericActionDTO Power { get; set; } = new GenericActionDTO();
        public GenericActionDTO Move { get; set; } = new GenericActionDTO();
    }
    public class CreepDTO
    {
        public long Count { get; set; } = 0;
        public Dictionary<string,long> Store = new Dictionary<string,long>();
        public Dictionary<string, long> BodyParts = new Dictionary<string, long>();
        public ActionLogDTO ActionLog { get; set; } = new ActionLogDTO();
    }
    public class BaseStructureDTO
    {
        public long Count { get; set; } = 0;
        public Dictionary<string, long> Store = new Dictionary<string, long>();
        public ActionLogDTO ActionLog { get; set; } = new ActionLogDTO();
    }
    #endregion
    #region Structures DTO
    public class StructureControllerDTO : BaseStructureDTO
    {
        public long Level { get; set; } = 0;
        public long Progress { get; set; } = 0;
        public long ProgressTotal { get; set; } = 0;
    }
    public class StructureMineralDTO : BaseStructureDTO
    {

    }
    public class StructureDepositDTO : BaseStructureDTO
    {

    }
    public class StructureWallDTO : BaseStructureDTO
    {
        public long Hits { get; set; } = 0;
    }
    public class StructureConstructionSiteDTO : BaseStructureDTO
    {
        public long Progress { get; set; } = 0;
        public long ProgressTotal { get; set; } = 0;
        public Dictionary<string, long> TypesBuilding { get; set; } = new Dictionary<string, long>();
    }
    public class StructureContainerDTO : BaseStructureDTO
    {
        public Dictionary<string, long> Store = new Dictionary<string, long>();
    }
    public class StructureExtensionDTO : BaseStructureDTO
    {
        public long Energy { get; set; } = 0;
        public long EnergyCapacity { get; set; } = 0;
    }
    public class StructureExtractorDTO : BaseStructureDTO
    {

    }
    public class StructureFactoryDTO : BaseStructureDTO
    {

    }
    public class StructureInvaderCoreDTO : BaseStructureDTO
    {

    }
    public class StructureKeeperLairDTO : BaseStructureDTO
    {

    }
    public class StructureLabDTO : BaseStructureDTO
    {

    }
    public class StructureLinkDTO : BaseStructureDTO
    {
        public long Energy { get; set; } = 0;
        public long EnergyCapacity { get; set; } = 0;
    }
    public class StructureObserverDTO : BaseStructureDTO
    {

    }
    public class StructurePortalDTO : BaseStructureDTO
    {

    }
    public class StructurePowerBankDTO : BaseStructureDTO
    {

    }
    public class StructurePowerSpawnDTO : BaseStructureDTO
    {

    }
    public class StructureRampartDTO : BaseStructureDTO
    {
        public long Hits { get; set; } = 0;
    }
    public class StructureRoadDTO : BaseStructureDTO
    {

    }
    public class StructureRuinDTO : BaseStructureDTO
    {

    }
    public class StructureSourceDTO : BaseStructureDTO
    {
        public long Energy { get; set; } = 0;
        public long EnergyCapacity { get; set; } = 0;
    }
    public class StructureSpawnDTO : BaseStructureDTO
    {

    }
    public class StructureStorageDTO : BaseStructureDTO
    {
        public Dictionary<string, long> Store = new Dictionary<string, long>();

    }
    public class StructureTerminalDTO : BaseStructureDTO
    {
        public Dictionary<string, long> Store = new Dictionary<string, long>();

    }
    public class StructureTombstoneDTO : BaseStructureDTO
    {

    }
    public class StructureTowerDTO : BaseStructureDTO
    {
        public long Energy { get; set; } = 0;
    }
    public class StructureNukerDTO : BaseStructureDTO
    {

    }
    public class StructureNukeDTO : BaseStructureDTO
    {

    }
    #endregion
    #region Groups DTO
    public class CreepsDTO
    {
        public CreepDTO OwnedCreeps { get; set; } = new CreepDTO();
        public CreepDTO EnemyCreeps { get; set; } = new CreepDTO();
        public CreepDTO OtherCreeps { get; set; } = new CreepDTO();
        public CreepDTO PowerCreeps { get; set; } = new CreepDTO();
    }
    public class StructuresDTO
    {
        public StructureControllerDTO Controller { get; set; } = new StructureControllerDTO();
        public StructureMineralDTO Mineral { get; set; } = new StructureMineralDTO();
        public StructureDepositDTO Deposit { get; set; } = new StructureDepositDTO();

        public StructureWallDTO Wall { get; set; } = new StructureWallDTO();
        public StructureConstructionSiteDTO ConstructionSite { get; set; } = new StructureConstructionSiteDTO();
        public StructureContainerDTO Container { get; set; } = new StructureContainerDTO();
        public StructureExtensionDTO Extension { get; set; } = new StructureExtensionDTO();
        public StructureExtractorDTO Extractor { get; set; } = new StructureExtractorDTO();
        public StructureFactoryDTO Factory { get; set; } = new StructureFactoryDTO();
        public StructureInvaderCoreDTO InvaderCore { get; set; } = new StructureInvaderCoreDTO();
        public StructureKeeperLairDTO KeeperLair { get; set; } = new StructureKeeperLairDTO();
        public StructureLabDTO Lab { get; set; } = new StructureLabDTO();
        public StructureLinkDTO Link { get; set; } = new StructureLinkDTO();
        public StructureObserverDTO Observer { get; set; } = new StructureObserverDTO();
        public StructurePortalDTO Portal { get; set; } = new StructurePortalDTO();
        public StructurePowerBankDTO PowerBank { get; set; } = new StructurePowerBankDTO();
        public StructurePowerSpawnDTO PowerSpawn { get; set; } = new StructurePowerSpawnDTO();
        public StructureRampartDTO Rampart { get; set; } = new StructureRampartDTO();
        public StructureRoadDTO Road { get; set; } = new StructureRoadDTO();
        public StructureRuinDTO Ruin { get; set; } = new StructureRuinDTO();
        public StructureSourceDTO Source { get; set; } = new StructureSourceDTO();
        public StructureSpawnDTO Spawn { get; set; } = new StructureSpawnDTO();
        public StructureStorageDTO Storage { get; set; } = new StructureStorageDTO();
        public StructureTerminalDTO Terminal { get; set; } = new StructureTerminalDTO();
        public StructureTombstoneDTO Tombstone { get; set; } = new StructureTombstoneDTO();
        public StructureTowerDTO Tower { get; set; } = new StructureTowerDTO();
        public StructureNukerDTO Nuker { get; set; } = new StructureNukerDTO();
        public StructureNukeDTO Nuke { get; set; } = new StructureNukeDTO();
    }
    #endregion
    public class ScreepsRoomHistoryDTO
    {
        private void ProcessGroundResources(ScreepsRoomHistory his)
        {
            var groundResourceKeys = his.GroundResources.Keys.ToList();
            for (int gr = 0; gr < groundResourceKeys.Count; gr++)
            {
                var groundResourceObj = his.GroundResources[groundResourceKeys[gr]];
                if (groundResourceObj == null) continue;

                PropertyInfo property = groundResourceObj.GetType().GetProperty(groundResourceObj.ResourceType);
                long toBeAddedAmount = Convert.ToInt64(property.GetValue(groundResourceObj));
                long currentAmount = 0;

                if (GroundResources.TryGetValue(groundResourceObj.ResourceType, out var existingGroundResource))
                {
                    currentAmount = existingGroundResource;
                }

                GroundResources[groundResourceObj.ResourceType] = currentAmount + toBeAddedAmount;
            }
        }
        private void ProcessCreeps(ScreepsRoomHistory his)
        {
            Creeps.OwnedCreeps = ScreepsRoomHistoryDTOHelper.ConvertCreeps(his.Creeps.OwnedCreeps.Values.ToList<BaseCreep>());
            Creeps.EnemyCreeps = ScreepsRoomHistoryDTOHelper.ConvertCreeps(his.Creeps.EnemyCreeps.Values.ToList<BaseCreep>());
            Creeps.OtherCreeps = ScreepsRoomHistoryDTOHelper.ConvertCreeps(his.Creeps.OtherCreeps.Values.ToList<BaseCreep>());
           
            Creeps.PowerCreeps = ScreepsRoomHistoryDTOHelper.ConvertCreeps(his.Creeps.PowerCreeps.Values.ToList<BaseCreep>());
        }

        private void ProcessStructures(ScreepsRoomHistory his)
        {
            Structures = ScreepsRoomHistoryDTOHelper.ConvertStructures(his.Structures);
        }

        public ScreepsRoomHistoryDTO(ScreepsRoomHistory his)
        {
            ProcessGroundResources(his);
            ProcessCreeps(his);
            ProcessStructures(his);
        }

        public long TimeStamp { get; set; }
        public long Base { get; set; }
        public long Tick { get; set; }
        public Dictionary<string, long> GroundResources { get; set; } = new Dictionary<string, long>();
        public CreepsDTO Creeps { get; set; } = new CreepsDTO();
        public StructuresDTO Structures { get; set; } = new StructuresDTO();
    }
}
