using Newtonsoft.Json;

namespace UserTrackerShared.Models.Db
{
    public class QuestDBHistoryDTO
    {
        public decimal StructureCount { get; set; }
        public decimal PlacedStructureCount { get; set; }
        public Dictionary<string, decimal> StructureCounts { get; set; } = [];


        public decimal CreepCount { get; set; }
        public decimal OwnedCreepCount { get; set; }
        public decimal EnemyCreepCount { get; set; }
        public decimal OtherCreepCount { get; set; }
        public decimal PowerCreepCount { get; set; }

        public decimal OwnedCreepPartsCount { get; set; }
        public Dictionary<string, decimal> OwnedCreepPartsCounts { get; set; } = [];

        public decimal CreepIntentCount { get; set; }
        public Dictionary<string, decimal> CreepIntentCounts { get; set; } = [];
        public decimal CreepEnergyInflow { get; set; }
        public decimal CreepEnergyOutflow { get; set; }

        public decimal OwnedRoomCount { get; set; }
        public decimal ReservedRoomCount { get; set; }

        public decimal? ControllerLevel { get; set; }
        public decimal? ControllerProgress { get; set; }
        public decimal? ControllerProgressTotal { get; set; }
        public decimal? ControllerPointsPerTick { get; set; }
        public decimal? ControllerScorePerTick { get; set; }

        // Null when detailed output is disabled, preserving the compact schema.
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public StructuresDto? Structures { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CreepsDto? Creeps { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, decimal>? GroundResources { get; set; }

        public decimal StoreTotal { get; set; }
        public Dictionary<string, decimal> StoreTotals { get; set; } = [];
    }
}
