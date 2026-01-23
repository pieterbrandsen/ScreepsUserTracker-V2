using System.Reflection;
using UserTrackerShared.Helpers;
using UserTrackerShared.States;

namespace UserTrackerShared.Models
{
    #region Base Dto
    public class DamageActionDto
    {
        public decimal Count { get; set; } = 0;
        public decimal Damage { get; set; } = 0;
        public void Clear()
        {
            Count = 0;
            Damage = 0;
        }
    }
    public class HealActionDto
    {
        public decimal Count { get; set; } = 0;
        public decimal Heal { get; set; } = 0;
        public void Clear()
        {
            Count = 0;
            Heal = 0;
        }
    }
    public class InflowActionDto
    {
        public decimal Count { get; set; } = 0;
        public decimal Inflow { get; set; } = 0;
        public void Clear()
        {
            Count = 0;
            Inflow = 0;
        }
    }
    public class OutlflowActionDto
    {
        public decimal Count { get; set; } = 0;
        public decimal Outflow { get; set; } = 0;
        public decimal Effect { get; set; } = 0;
        public void Clear()
        {
            Count = 0;
            Outflow = 0;
            Effect = 0;
        }
    }
    public class GenericActionDto
    {
        public decimal Count { get; set; } = 0;
        public void Clear()
        {
            Count = 0;
        }
    }
    public class ActionLogDto
    {
        public DamageActionDto Attacked { get; set; } = new DamageActionDto();
        public DamageActionDto Attack { get; set; } = new DamageActionDto();
        public DamageActionDto RangedAttack { get; set; } = new DamageActionDto();
        public DamageActionDto RangedMassAttack { get; set; } = new DamageActionDto();
        public HealActionDto RangedHeal { get; set; } = new HealActionDto();
        public HealActionDto Healed { get; set; } = new HealActionDto();
        public HealActionDto Heal { get; set; } = new HealActionDto();
        public InflowActionDto Harvest { get; set; } = new InflowActionDto();
        public OutlflowActionDto Repair { get; set; } = new OutlflowActionDto();
        public OutlflowActionDto Build { get; set; } = new OutlflowActionDto();
        public OutlflowActionDto UpgradeController { get; set; } = new OutlflowActionDto();
        public GenericActionDto Move { get; set; } = new GenericActionDto();
        public GenericActionDto Say { get; set; } = new GenericActionDto();
        public GenericActionDto ReserveController { get; set; } = new GenericActionDto();
        public GenericActionDto Produce { get; set; } = new GenericActionDto();
        public GenericActionDto TransferEnergy { get; set; } = new GenericActionDto();
        public GenericActionDto AttackController { get; set; } = new GenericActionDto();
        public GenericActionDto RunReaction { get; set; } = new GenericActionDto();
        public GenericActionDto ReverseReaction { get; set; } = new GenericActionDto();
        public GenericActionDto Spawned { get; set; } = new GenericActionDto();
        public GenericActionDto Power { get; set; } = new GenericActionDto();
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
    public class CountByPartDto
    {
        public decimal Move { get; set; } = 0;
        public decimal Work { get; set; } = 0;
        public decimal Carry { get; set; } = 0;
        public decimal Attack { get; set; } = 0;
        public decimal RangedAttack { get; set; } = 0;
        public decimal Tough { get; set; } = 0;
        public decimal Heal { get; set; } = 0;
        public decimal Claim { get; set; } = 0;
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

    public class CreepDto
    {
        public decimal Count { get; set; } = 0;
        public Store Store { get; set; } = new Store();
        public CountByPartDto BodyParts { get; set; } = new CountByPartDto();
        public ActionLogDto ActionLog { get; set; } = new ActionLogDto();

        public void Clear()
        {
            Count = 0;
            Store.Clear();
            BodyParts.Clear();
            ActionLog.Clear();
        }
    }
    public class BaseStructureDto
    {
        public decimal Count { get; set; } = 0;
        public void Clear()
        {
            Count = 0;
        }
    }
    #endregion
    #region Structures Dto
    public class StructureControllerDto : BaseStructureDto
    {
        public decimal Level { get; set; } = 0;
        public decimal Progress { get; set; } = 0;
        public decimal ProgressTotal { get; set; } = 0;
        public string UserId { get; set; } = "";
        public string? ReservationUserId { get; set; }
        public decimal Upgraded { get; set; } = 0;
        public new void Clear()
        {
            Level = 0;
            Progress = 0;
            ProgressTotal = 0;
            UserId = "";
            Upgraded = 0;
            base.Clear();
        }
    }
    public class StructureMineralDto : BaseStructureDto
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureDepositDto : BaseStructureDto
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureWallDto : BaseStructureDto
    {
        public decimal Hits { get; set; } = 0;
        public new void Clear()
        {
            Hits = 0;
            base.Clear();
        }
    }
    public class StructureConstructionSiteDto : BaseStructureDto
    {
        public decimal Progress { get; set; } = 0;
        public decimal ProgressTotal { get; set; } = 0;
        public Dictionary<string, decimal> TypesBuilding { get; set; } = new Dictionary<string, decimal>();
        public new void Clear()
        {
            Progress = 0;
            ProgressTotal = 0;
            TypesBuilding.Clear();
            base.Clear();
        }
    }
    public class StructureContainerDto : BaseStructureDto
    {
        public Store Store { get; set; } = new Store();
        public new void Clear()
        {
            Store.Clear();
            base.Clear();
        }
    }
    public class StructureExtensionDto : BaseStructureDto
    {
        public decimal Energy { get; set; } = 0;
        public decimal EnergyCapacity { get; set; } = 0;
        public new void Clear()
        {
            Energy = 0;
            EnergyCapacity = 0;
            base.Clear();
        }
    }
    public class StructureExtractorDto : BaseStructureDto
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureFactoryDto : BaseStructureDto
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureInvaderCoreDto : BaseStructureDto
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureKeeperLairDto : BaseStructureDto
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureLabDto : BaseStructureDto
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureLinkDto : BaseStructureDto
    {
        public decimal Energy { get; set; } = 0;
        public decimal EnergyCapacity { get; set; } = 0;
        public new void Clear()
        {
            Energy = 0;
            EnergyCapacity = 0;
            base.Clear();
        }
    }
    public class StructureObserverDto : BaseStructureDto
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructurePortalDto : BaseStructureDto
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructurePowerBankDto : BaseStructureDto
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructurePowerSpawnDto : BaseStructureDto
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureRampartDto : BaseStructureDto
    {
        public decimal Hits { get; set; } = 0;
        public new void Clear()
        {
            Hits = 0;
            base.Clear();
        }
    }
    public class StructureRoadDto : BaseStructureDto
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureRuinDto : BaseStructureDto
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureSourceDto : BaseStructureDto
    {
        public decimal Energy { get; set; } = 0;
        public decimal EnergyCapacity { get; set; } = 0;
        public new void Clear()
        {
            Energy = 0;
            EnergyCapacity = 0;
            base.Clear();
        }
    }
    public class StructureSpawnDto : BaseStructureDto
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureStorageDto : BaseStructureDto
    {
        public Store Store { get; set; } = new Store();
        public new void Clear()
        {
            base.Clear();
        }

    }
    public class StructureTerminalDto : BaseStructureDto
    {
        public Store Store { get; set; } = new Store();
        public new void Clear()
        {
            base.Clear();
        }

    }
    public class StructureTombstoneDto : BaseStructureDto
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureTowerDto : BaseStructureDto
    {
        public decimal Energy { get; set; } = 0;
        public new void Clear()
        {
            Energy = 0;
            base.Clear();
        }
    }
    public class StructureNukerDto : BaseStructureDto
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    public class StructureNukeDto : BaseStructureDto
    {
        public new void Clear()
        {
            base.Clear();
        }
    }
    #endregion
    #region Groups Dto
    public class CreepsDto
    {
        public CreepDto OwnedCreeps { get; set; } = new CreepDto();
        public CreepDto EnemyCreeps { get; set; } = new CreepDto();
        public CreepDto OtherCreeps { get; set; } = new CreepDto();
        public CreepDto PowerCreeps { get; set; } = new CreepDto();

        public void Clear()
        {
            OwnedCreeps.Clear();
            EnemyCreeps.Clear();
            OtherCreeps.Clear();
            PowerCreeps.Clear();
        }
    }
    public class StructuresDto
    {
        public StructureControllerDto Controller { get; set; } = new StructureControllerDto();
        public StructureMineralDto Mineral { get; set; } = new StructureMineralDto();
        public StructureDepositDto Deposit { get; set; } = new StructureDepositDto();

        public StructureWallDto Wall { get; set; } = new StructureWallDto();
        public StructureConstructionSiteDto ConstructionSite { get; set; } = new StructureConstructionSiteDto();
        public StructureContainerDto Container { get; set; } = new StructureContainerDto();
        public StructureExtensionDto Extension { get; set; } = new StructureExtensionDto();
        public StructureExtractorDto Extractor { get; set; } = new StructureExtractorDto();
        public StructureFactoryDto Factory { get; set; } = new StructureFactoryDto();
        public StructureInvaderCoreDto InvaderCore { get; set; } = new StructureInvaderCoreDto();
        public StructureKeeperLairDto KeeperLair { get; set; } = new StructureKeeperLairDto();
        public StructureLabDto Lab { get; set; } = new StructureLabDto();
        public StructureLinkDto Link { get; set; } = new StructureLinkDto();
        public StructureObserverDto Observer { get; set; } = new StructureObserverDto();
        public StructurePortalDto Portal { get; set; } = new StructurePortalDto();
        public StructurePowerBankDto PowerBank { get; set; } = new StructurePowerBankDto();
        public StructurePowerSpawnDto PowerSpawn { get; set; } = new StructurePowerSpawnDto();
        public StructureRampartDto Rampart { get; set; } = new StructureRampartDto();
        public StructureRoadDto Road { get; set; } = new StructureRoadDto();
        public StructureRuinDto Ruin { get; set; } = new StructureRuinDto();
        public StructureSourceDto Source { get; set; } = new StructureSourceDto();
        public StructureSpawnDto Spawn { get; set; } = new StructureSpawnDto();
        public StructureStorageDto Storage { get; set; } = new StructureStorageDto();
        public StructureTerminalDto Terminal { get; set; } = new StructureTerminalDto();
        public StructureTombstoneDto Tombstone { get; set; } = new StructureTombstoneDto();
        public StructureTowerDto Tower { get; set; } = new StructureTowerDto();
        public StructureNukerDto Nuker { get; set; } = new StructureNukerDto();
        public StructureNukeDto Nuke { get; set; } = new StructureNukeDto();
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
    public class ScreepsRoomHistoryDto
    {
        public void ProcessGroundResources(ScreepsRoomHistory his)
        {
            var groundResourceKeys = his.GroundResources.Keys.ToList();
            for (int gr = 0; gr < groundResourceKeys.Count; gr++)
            {
                var groundResourceObj = his.GroundResources[groundResourceKeys[gr]];
                if (groundResourceObj == null) continue;

                var resourceType = groundResourceObj.ResourceType ?? "";
                PropertyInfo? property = groundResourceObj.GetType()
                    .GetProperty(resourceType) ?? groundResourceObj.GetType().GetProperty(char.ToUpperInvariant(resourceType[0]) + resourceType.Substring(1));
                if (property == null) continue;

                long toBeAddedAmount = Convert.ToInt64(property.GetValue(groundResourceObj));
                decimal currentAmount = 0;

                if (GroundResources.TryGetValue(resourceType, out var existingGroundResource))
                {
                    currentAmount = existingGroundResource;
                }

                GroundResources[resourceType] = currentAmount + toBeAddedAmount / ConfigSettingsState.TicksInFile;
            }
        }
        public void CombineGroundResources(ScreepsRoomHistoryDto hisDto)
        {
            foreach (var grKvp in hisDto.GroundResources)
            {
                if (GroundResources.TryGetValue(grKvp.Key, out var existingValue))
                {
                    GroundResources[grKvp.Key] = existingValue + grKvp.Value;
                }
                else
                {
                    GroundResources[grKvp.Key] = grKvp.Value;
                }
            }
        }

        public void ProcessCreeps(ScreepsRoomHistory his)
        {
            Creeps.OwnedCreeps = ScreepsRoomHistoryDtoHelper.ConvertCreeps(his.Creeps.OwnedCreeps.Values.ToList<BaseCreep>(), Creeps.OwnedCreeps);
            Creeps.EnemyCreeps = ScreepsRoomHistoryDtoHelper.ConvertCreeps(his.Creeps.EnemyCreeps.Values.ToList<BaseCreep>(), Creeps.EnemyCreeps);
            Creeps.OtherCreeps = ScreepsRoomHistoryDtoHelper.ConvertCreeps(his.Creeps.OtherCreeps.Values.ToList<BaseCreep>(), Creeps.OtherCreeps);

            Creeps.PowerCreeps = ScreepsRoomHistoryDtoHelper.ConvertCreeps(his.Creeps.PowerCreeps.Values.ToList<BaseCreep>(), Creeps.PowerCreeps);
        }
        public void CombineCreeps(ScreepsRoomHistoryDto hisDto)
        {
            Creeps.OwnedCreeps = ScreepsRoomHistoryDtoHelper.CombineCreeps(hisDto.Creeps.OwnedCreeps, Creeps.OwnedCreeps);
            Creeps.EnemyCreeps = ScreepsRoomHistoryDtoHelper.CombineCreeps(hisDto.Creeps.EnemyCreeps, Creeps.EnemyCreeps);
            Creeps.OtherCreeps = ScreepsRoomHistoryDtoHelper.CombineCreeps(hisDto.Creeps.OtherCreeps, Creeps.OtherCreeps);

            Creeps.PowerCreeps = ScreepsRoomHistoryDtoHelper.CombineCreeps(hisDto.Creeps.PowerCreeps, Creeps.PowerCreeps);
        }

        public void ProcessStructures(ScreepsRoomHistory his)
        {
            Structures = ScreepsRoomHistoryDtoHelper.ConvertStructures(his.Structures, Structures);
        }

        public void CombineStructures(ScreepsRoomHistoryDto his)
        {
            Structures = ScreepsRoomHistoryDtoHelper.CombineStructures(his.Structures, Structures);
        }

        public void Update(ScreepsRoomHistory his)
        {
            TimeStamp = his.TimeStamp;
            Base = his.Base;
            Tick = his.Tick;

            if (his.ObjectUserMap.Any())
            {
                var userId = his.ObjectUserMap.Count == 1 ? his.ObjectUserMap.First().Value : his.ObjectUserMap.Values.GroupBy(v => v).OrderByDescending(g => g.Count()).First().Key;
                UserId = userId;
            }

            ProcessGroundResources(his);
            ProcessCreeps(his);
            ProcessStructures(his);
        }
        public void Combine(ScreepsRoomHistoryDto hisDto)
        {
            TimeStamp = hisDto.TimeStamp;
            Tick = hisDto.Tick;

            CombineGroundResources(hisDto);
            CombineCreeps(hisDto);
            CombineStructures(hisDto);
        }
        public void ClearAll()
        {
            GroundResources.Clear();
            Creeps.Clear();
            Structures.Clear();
        }

        public long TimeStamp { get; set; }
        public long Base { get; set; }
        public long Tick { get; set; }
        public string UserId { get; set; }
        public Dictionary<string, decimal> GroundResources { get; set; } = new Dictionary<string, decimal>();
        public CreepsDto Creeps { get; set; } = new CreepsDto();
        public StructuresDto Structures { get; set; } = new StructuresDto();
    }
}
