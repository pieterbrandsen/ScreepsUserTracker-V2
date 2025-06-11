using Newtonsoft.Json;
using System.Collections.Generic;

namespace UserTrackerShared.Models.ScreepsAPI
{
    public class AdminUtilsRcl
    {
        [JsonProperty("1")]
        public int LevelOne { get; set; }
        [JsonProperty("2")]
        public int LevelTwo { get; set; }
        [JsonProperty("3")]
        public int LevelThree { get; set; }
        [JsonProperty("4")]
        public int LevelFour { get; set; }
        [JsonProperty("5")]
        public int LevelFive { get; set; }
        [JsonProperty("6")]
        public int LevelSix { get; set; }
        [JsonProperty("7")]
        public int LevelSeven { get; set; }
        [JsonProperty("8")]
        public int LevelEight { get; set; }
    }

    public class AdminUtilsUser
    {
        [JsonProperty("id")]
        public required string Id { get; set; }
        [JsonProperty("username")]
        public required string Username { get; set; }
        [JsonProperty("gcl")]
        public int Gcl { get; set; }
        [JsonProperty("gclLevel")]
        public int GclLevel { get; set; }
        [JsonProperty("power")]
        public int Power { get; set; }
        [JsonProperty("powerLevel")]
        public int PowerLevel { get; set; }
        [JsonProperty("rank")]
        public int Rank { get; set; }
        [JsonProperty("creeps")]
        public int Creeps { get; set; }
        [JsonProperty("rooms")]
        public int Rooms { get; set; }
        [JsonProperty("combinedRCL")]
        public int CombinedRcl { get; set; }
        [JsonProperty("rcl")]
        public required AdminUtilsRcl Rcl { get; set; }
        [JsonProperty("cpu")]
        public int Cpu { get; set; }
        [JsonProperty("cpuAvailable")]
        public int CpuAvailable { get; set; }
        [JsonProperty("lastUsedCpu")]
        public int LastUsedCpu { get; set; }
        [JsonProperty("lastUsedDirtyTime")]
        public int LastUsedDirtyTime { get; set; }
    }

    public class AdminUtilsStages
    {
        [JsonProperty("start")]
        public double Start { get; set; }
        [JsonProperty("getUsers")]
        public double GetUsers { get; set; }
        [JsonProperty("addUsersToQueue")]
        public double AddUsersToQueue { get; set; }
        [JsonProperty("waitForUsers")]
        public double WaitForUsers { get; set; }
        [JsonProperty("getRooms")]
        public double GetRooms { get; set; }
        [JsonProperty("addRoomsToQueue")]
        public double AddRoomsToQueue { get; set; }
        [JsonProperty("waitForRooms")]
        public double WaitForRooms { get; set; }
        [JsonProperty("commit1")]
        public double CommitUser { get; set; }
        [JsonProperty("global")]
        public double Global { get; set; }
        [JsonProperty("commit2")]
        public double Commit2 { get; set; }
        [JsonProperty("incrementGameTime")]
        public double IncrementGameTime { get; set; }
        [JsonProperty("notifyRoomsDone")]
        public double NotifyRoomsDone { get; set; }
        [JsonProperty("custom")]
        public double Custom { get; set; }
    }

    public class AdminUtilsTicks
    {
        [JsonProperty("avg")]
        public double Average { get; set; }
        [JsonProperty("min")]
        public int Minimum { get; set; }
        [JsonProperty("max")]
        public int Maximum { get; set; }
        [JsonProperty("maxDeviation")]
        public int MaxDeviation { get; set; }
        [JsonProperty("stages")]
        public required AdminUtilsStages Stages { get; set; }
    }

    public class AdminUtilsObjects
    {
        [JsonProperty("all")]
        public int Total { get; set; }
        [JsonProperty("creeps")]
        public int Creeps { get; set; }
    }

    public class AdminUtilsResponse
    {
        [JsonProperty("activeUsers")]
        public int ActiveUsers { get; set; }
        [JsonProperty("activeRooms")]
        public int ActiveRooms { get; set; }
        [JsonProperty("objects")]
        public required AdminUtilsObjects Objects { get; set; }
        [JsonProperty("totalRooms")]
        public int TotalRooms { get; set; }
        [JsonProperty("ownedRooms")]
        public int OwnedRooms { get; set; }
        [JsonProperty("gametime")]
        public long GameTime { get; set; }
        [JsonProperty("ticks")]
        public required AdminUtilsTicks Ticks { get; set; }
        [JsonProperty("users")]
        public required List<AdminUtilsUser> Users { get; set; }
    }
    public class AdminUtilsDto
    {
        public AdminUtilsDto(AdminUtilsResponse response)
        {
            ActiveUsers = response.ActiveUsers;
            ActiveRooms = response.ActiveRooms;
            Objects = response.Objects;
            TotalRooms = response.TotalRooms;
            OwnedRooms = response.OwnedRooms;
            GameTime = response.GameTime;
            Ticks = response.Ticks;
            Users = new Dictionary<string, AdminUtilsUser>();
            foreach (var user in response.Users)
            {
                Users.Add(user.Username, user);
            }
        }
        [JsonProperty("activeUsers")]
        public int ActiveUsers { get; set; }
        [JsonProperty("activeRooms")]
        public int ActiveRooms { get; set; }
        [JsonProperty("objects")]
        public AdminUtilsObjects Objects { get; set; }
        [JsonProperty("totalRooms")]
        public int TotalRooms { get; set; }
        [JsonProperty("ownedRooms")]
        public int OwnedRooms { get; set; }
        [JsonProperty("gametime")]
        public long GameTime { get; set; }
        [JsonProperty("ticks")]
        public AdminUtilsTicks Ticks { get; set; }
        [JsonProperty("users")]
        public Dictionary<string, AdminUtilsUser> Users { get; set; }
    }
}
