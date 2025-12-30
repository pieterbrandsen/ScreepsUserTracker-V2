namespace UserTrackerShared.Models.Db
{
    public class QuestDBHistoryDTO
    {
        public int StructureCount { get; set; }
        public int PlacedStructureCount { get; set; }
        public Dictionary<string, int> StructureCounts { get; set; } = [];


        public int CreepCount { get; set; }
        public int OwnedCreepCount { get; set; }
        public int EnemyCreepCount { get; set; }
        public int OtherCreepCount { get; set; }
        public int PowerCreepCount { get; set; }

        public int OwnedCreepPartsCount { get; set; }
        public Dictionary<string, int> OwnedCreepPartsCounts { get; set; } = [];
        public int CreepIntentCount { get; set; }
        public Dictionary<string, int> CreepIntentCounts { get; set; } = [];

        public int OwnedRoomCount { get; set; }
        public int ReservedRoomCount { get; set; }
        public int OtherRoomCount { get; set; }

        public int? ControllerLevel { get; set; }
        public int? ControllerProgress { get; set; }
        public int? ControllerProgressTotal { get; set; }
        public int? ControllerPointsPerTick { get; set; }

        public int StoreTotal { get; set; }
        public Dictionary<string, int> StoreTotals { get; set; } = [];
    }
}