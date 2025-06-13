using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UserTrackerShared.Migrations
{
    /// <inheritdoc />
    public partial class InitialTimescale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PerformanceStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Server = table.Column<string>(type: "text", nullable: false),
                    ResultCodesJson = table.Column<string>(type: "jsonb", nullable: false),
                    Shard = table.Column<string>(type: "text", nullable: false),
                    TicksBehind = table.Column<long>(type: "bigint", nullable: false),
                    TimeTakenMs = table.Column<long>(type: "bigint", nullable: false),
                    TotalRooms = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceStats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScreepsRoomHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Server = table.Column<string>(type: "text", nullable: false),
                    Shard = table.Column<string>(type: "text", nullable: false),
                    Room = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    GroundResourcesJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreepsJson = table.Column<string>(type: "jsonb", nullable: false),
                    StructuresJson = table.Column<string>(type: "jsonb", nullable: false),
                    TimeStamp = table.Column<long>(type: "bigint", nullable: false),
                    Base = table.Column<long>(type: "bigint", nullable: false),
                    Tick = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreepsRoomHistory", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerformanceStats");

            migrationBuilder.DropTable(
                name: "ScreepsRoomHistory");
        }
    }
}
