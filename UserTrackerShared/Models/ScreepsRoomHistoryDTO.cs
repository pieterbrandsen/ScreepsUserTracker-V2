using System.Reflection;
using UserTrackerShared.Helpers;

namespace UserTrackerShared.Models
{
    #region Base DTO
    public class DamageActionDTO
    {
        public long Count { get; set; } = 0;
        public long Damage { get; set; } = 0;
        public void Clear()
        {
            Count = 0;
            Damage = 0;
        }
    }
    public class HealActionDTO
    {
        public long Count { get; set; } = 0;
        public long Heal { get; set; } = 0;
        public void Clear()
        {
            Count = 0;
            Heal = 0;
        }
    }
    public class InflowActionDTO
    {
        public long Count { get; set; } = 0;
        public long Inflow { get; set; } = 0;
        public void Clear()
        {
            Count = 0;
            Inflow = 0;
        }
    }
    public class OutlflowActionDTO
    {
        public long Count { get; set; } = 0;
        public long Outflow { get; set; } = 0;
        public long Effect { get; set; } = 0;
        public void Clear()
        {
            Count = 0;
            Outflow = 0;
            Effect = 0;
        }
    }
    public class GenericActionDTO
    {
        public long Count { get; set; } = 0;
        public void Clear()
        {
            Count = 0;
        }
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
        public GenericActionDTO Move { get; set; } = new GenericActionDTO();
        public GenericActionDTO Say { get; set; } = new GenericActionDTO();
        public GenericActionDTO ReserveController { get; set; } = new GenericActionDTO();
        public GenericActionDTO Produce { get; set; } = new GenericActionDTO();
        public GenericActionDTO TransferEnergy { get; set; } = new GenericActionDTO();
        public GenericActionDTO AttackController { get; set; } = new GenericActionDTO();
        public GenericActionDTO RunReaction { get; set; } = new GenericActionDTO();
        public GenericActionDTO ReverseReaction { get; set; } = new GenericActionDTO();
        public GenericActionDTO Spawned { get; set; } = new GenericActionDTO();
        public GenericActionDTO Power { get; set; } = new GenericActionDTO();
        public void Clear()
        {
            Attacked.Clear();
            Attack.Clear();
            RangedAttack.Clear();
            RangedMassAttack.Clear();
            RangedHeal.Clear();
            Healed.Clear();
            Heal.Clear();
            Harvest.Clear();
            Repair.Clear();
            Build.Clear();
            UpgradeController.Clear();
            Move.Clear();
            Say.Clear();
            ReserveController.Clear();
            Produce.Clear();
            TransferEnergy.Clear();
            AttackController.Clear();
            RunReaction.Clear();
            ReverseReaction.Clear();
            Spawned.Clear();
            Power.Clear();
        }
    }
    public class CountByPartDTO
    {
        public long Move { get; set; } = 0;
        public long Work { get; set; } = 0;
        public long Carry { get; set; } = 0;
        public long Attack { get; set; } = 0;
        public long RangedAttack { get; set; } = 0;
        public long Tough { get; set; } = 0;
        public long Heal { get; set; } = 0;
        public long Claim { get; set; } = 0;
        public void Clear()
        {
            Move = 0;
            Work = 0;
            Carry = 0;
            Attack = 0;
            RangedAttack = 0;
            Tough = 0;
            Heal = 0;
            Claim = 0;
        }
    }

    public class CreepDTO
    {
        public long Count { get; set; } = 0;
        public Store Store = new Store();
        public CountByPartDTO BodyParts = new CountByPartDTO();
        public ActionLogDTO ActionLog { get; set; } = new ActionLogDTO();

        public void Clear()
        {
            Count = 0;
            Store.Clear();
            BodyParts.Clear();
            ActionLog.Clear();
        }
    }
    public class BaseStructureDTO
    {
        public long Count { get; set; } = 0;
        public ActionLogDTO ActionLog { get; set; } = new ActionLogDTO();
        public void Clear()
        {
            Count = 0;
            ActionLog.Clear();
        }
    }
    #endregion
    #region Structures DTO
    public class StructureControllerDTO : BaseStructureDTO
    {
        public long Level { get; set; } = 0;
        public long Progress { get; set; } = 0;
        public long ProgressTotal { get; set; } = 0;
        public string UserId { get; set; } = "";
        public new void Clear()
        {
            Level = 0;
            Progress = 0;
            ProgressTotal = 0;
            UserId = "";
            base.Clear();
        }
    }
    public class StructureMineralDTO : BaseStructureDTO
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureDepositDTO : BaseStructureDTO
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureWallDTO : BaseStructureDTO
    {
        public long Hits { get; set; } = 0;
        public new void Clear()
        {
            Hits = 0;
            base.Clear();
        }
    }
    public class StructureConstructionSiteDTO : BaseStructureDTO
    {
        public long Progress { get; set; } = 0;
        public long ProgressTotal { get; set; } = 0;
        public Dictionary<string, long> TypesBuilding { get; set; } = new Dictionary<string, long>();
        public new void Clear()
        {
            Progress = 0;
            ProgressTotal = 0;
            TypesBuilding.Clear();
            base.Clear();
        }
    }
    public class StructureContainerDTO : BaseStructureDTO
    {
        public Store Store = new Store();
        public new void Clear()
        {
            Store.Clear();
            base.Clear();
        }
    }
    public class StructureExtensionDTO : BaseStructureDTO
    {
        public long Energy { get; set; } = 0;
        public long EnergyCapacity { get; set; } = 0;
        public new void Clear()
        {
            Energy = 0;
            EnergyCapacity = 0;
            base.Clear();
        }
    }
    public class StructureExtractorDTO : BaseStructureDTO
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureFactoryDTO : BaseStructureDTO
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureInvaderCoreDTO : BaseStructureDTO
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureKeeperLairDTO : BaseStructureDTO
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureLabDTO : BaseStructureDTO
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureLinkDTO : BaseStructureDTO
    {
        public long Energy { get; set; } = 0;
        public long EnergyCapacity { get; set; } = 0;
        public new void Clear()
        {
            Energy = 0;
            EnergyCapacity = 0;
            base.Clear();
        }
    }
    public class StructureObserverDTO : BaseStructureDTO
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructurePortalDTO : BaseStructureDTO
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructurePowerBankDTO : BaseStructureDTO
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructurePowerSpawnDTO : BaseStructureDTO
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureRampartDTO : BaseStructureDTO
    {
        public long Hits { get; set; } = 0;
        public new void Clear()
        {
            Hits = 0;
            base.Clear();
        }
    }
    public class StructureRoadDTO : BaseStructureDTO
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureRuinDTO : BaseStructureDTO
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureSourceDTO : BaseStructureDTO
    {
        public long Energy { get; set; } = 0;
        public long EnergyCapacity { get; set; } = 0;
        public new void Clear()
        {
            Energy = 0;
            EnergyCapacity = 0;
            base.Clear();
        }
    }
    public class StructureSpawnDTO : BaseStructureDTO
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureStorageDTO : BaseStructureDTO
    {
        public Store Store = new Store();
        public new void Clear()
        {
            base.Clear();
        }

    }
    public class StructureTerminalDTO : BaseStructureDTO
    {
        public Store Store = new Store();
        public new void Clear()
        {
            base.Clear();
        }

    }
    public class StructureTombstoneDTO : BaseStructureDTO
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureTowerDTO : BaseStructureDTO
    {
        public long Energy { get; set; } = 0;
        public void Clear()
        {
            Energy = 0;
            base.Clear();
        }
    }
    public class StructureNukerDTO : BaseStructureDTO
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureNukeDTO : BaseStructureDTO
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    #endregion
    #region Groups DTO
    public class CreepsDTO
    {
        public CreepDTO OwnedCreeps { get; set; } = new CreepDTO();
        public CreepDTO EnemyCreeps { get; set; } = new CreepDTO();
        public CreepDTO OtherCreeps { get; set; } = new CreepDTO();
        public CreepDTO PowerCreeps { get; set; } = new CreepDTO();

        public void Clear()
        {
            OwnedCreeps.Clear();
            EnemyCreeps.Clear();
            OtherCreeps.Clear();
            PowerCreeps.Clear();
        }
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
        public void Clear()
        {
            Controller.Clear();
            Mineral.Clear();
            Deposit.Clear();
            Wall.Clear();
            ConstructionSite.Clear();
            Container.Clear();
            Extension.Clear();
            Extractor.Clear();
            Factory.Clear();
            InvaderCore.Clear();
            KeeperLair.Clear();
            Lab.Clear();
            Link.Clear();
            Observer.Clear();
            Portal.Clear();
            PowerBank.Clear();
            PowerSpawn.Clear();
            Rampart.Clear();
            Road.Clear();
            Ruin.Clear();
            Source.Clear();
            Spawn.Clear();
            Storage.Clear();
            Terminal.Clear();
            Tombstone.Clear();
            Tower.Clear();
            Nuker.Clear();
            Nuke.Clear();
        }
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
            Creeps.OwnedCreeps = ScreepsRoomHistoryDTOHelper.ConvertCreeps(his.Creeps.OwnedCreeps.Values.ToList<BaseCreep>(), Creeps.OwnedCreeps);
            Creeps.EnemyCreeps = ScreepsRoomHistoryDTOHelper.ConvertCreeps(his.Creeps.EnemyCreeps.Values.ToList<BaseCreep>(), Creeps.EnemyCreeps);
            Creeps.OtherCreeps = ScreepsRoomHistoryDTOHelper.ConvertCreeps(his.Creeps.OtherCreeps.Values.ToList<BaseCreep>(), Creeps.OtherCreeps);

            Creeps.PowerCreeps = ScreepsRoomHistoryDTOHelper.ConvertCreeps(his.Creeps.PowerCreeps.Values.ToList<BaseCreep>(), Creeps.PowerCreeps);
        }

        private void ProcessStructures(ScreepsRoomHistory his)
        {
            Structures = ScreepsRoomHistoryDTOHelper.ConvertStructures(his.Structures, Structures);
        }

        public void Update(ScreepsRoomHistory his)
        {
            TimeStamp = his.TimeStamp;
            Base = his.Base;
            Tick = his.Tick;

            ClearAll(); ;
            ProcessGroundResources(his);
            ProcessCreeps(his);
            ProcessStructures(his);
        }
        private void ClearAll()
        {
            GroundResources.Clear();
            Creeps.Clear();
            Structures.Clear();
        }

        public long TimeStamp { get; set; }
        public long Base { get; set; }
        public long Tick { get; set; }
        public Dictionary<string, long> GroundResources { get; set; } = new Dictionary<string, long>();
        public CreepsDTO Creeps { get; set; } = new CreepsDTO();
        public StructuresDTO Structures { get; set; } = new StructuresDTO();
    }
}
