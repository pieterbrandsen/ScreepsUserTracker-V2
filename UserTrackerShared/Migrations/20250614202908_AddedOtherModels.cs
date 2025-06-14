using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UserTrackerShared.Migrations
{
    /// <inheritdoc />
    public partial class AddedOtherModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ScreepsRoomHistory",
                table: "ScreepsRoomHistory");

            migrationBuilder.RenameTable(
                name: "ScreepsRoomHistory",
                newName: "TimeScaleScreepsRoomHistoryDto");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimeScaleScreepsRoomHistoryDto",
                table: "TimeScaleScreepsRoomHistoryDto",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AdminUtilsData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Server = table.Column<string>(type: "text", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ObjectsJson = table.Column<string>(type: "jsonb", nullable: false),
                    TicksJson = table.Column<string>(type: "jsonb", nullable: false),
                    UsersJson = table.Column<string>(type: "jsonb", nullable: false),
                    ActiveUsers = table.Column<int>(type: "integer", nullable: false),
                    ActiveRooms = table.Column<int>(type: "integer", nullable: false),
                    TotalRooms = table.Column<int>(type: "integer", nullable: false),
                    OwnedRooms = table.Column<int>(type: "integer", nullable: false),
                    GameTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUtilsData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeasonItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Server = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Season = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Server = table.Column<string>(type: "text", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    BadgeJson = table.Column<string>(type: "jsonb", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    GCL = table.Column<long>(type: "bigint", nullable: false),
                    Power = table.Column<long>(type: "bigint", nullable: false),
                    GCLRank = table.Column<int>(type: "integer", nullable: false),
                    PowerRank = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminUtilsData");

            migrationBuilder.DropTable(
                name: "SeasonItems");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimeScaleScreepsRoomHistoryDto",
                table: "TimeScaleScreepsRoomHistoryDto");

            migrationBuilder.RenameTable(
                name: "TimeScaleScreepsRoomHistoryDto",
                newName: "ScreepsRoomHistory");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScreepsRoomHistory",
                table: "ScreepsRoomHistory",
                column: "Id");
        }
    }
}
