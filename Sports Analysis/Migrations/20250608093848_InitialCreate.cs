using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sports_Analysis.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FootballMatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Season = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HomeTeam = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AwayTeam = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeClearances = table.Column<int>(type: "int", nullable: false),
                    HomeCorners = table.Column<int>(type: "int", nullable: false),
                    HomeFoulsConceded = table.Column<int>(type: "int", nullable: false),
                    HomeOffsides = table.Column<int>(type: "int", nullable: false),
                    HomePasses = table.Column<int>(type: "int", nullable: false),
                    HomePossession = table.Column<double>(type: "float", nullable: false),
                    HomeRedCards = table.Column<int>(type: "int", nullable: false),
                    HomeShots = table.Column<int>(type: "int", nullable: false),
                    HomeShotsOnTarget = table.Column<int>(type: "int", nullable: false),
                    HomeTackles = table.Column<int>(type: "int", nullable: false),
                    HomeTouches = table.Column<int>(type: "int", nullable: false),
                    HomeYellowCards = table.Column<int>(type: "int", nullable: false),
                    AwayClearances = table.Column<int>(type: "int", nullable: false),
                    AwayCorners = table.Column<int>(type: "int", nullable: false),
                    AwayFoulsConceded = table.Column<int>(type: "int", nullable: false),
                    AwayOffsides = table.Column<int>(type: "int", nullable: false),
                    AwayPasses = table.Column<int>(type: "int", nullable: false),
                    AwayPossession = table.Column<double>(type: "float", nullable: false),
                    AwayRedCards = table.Column<int>(type: "int", nullable: false),
                    AwayShots = table.Column<int>(type: "int", nullable: false),
                    AwayShotsOnTarget = table.Column<int>(type: "int", nullable: false),
                    AwayTackles = table.Column<int>(type: "int", nullable: false),
                    AwayTouches = table.Column<int>(type: "int", nullable: false),
                    AwayYellowCards = table.Column<int>(type: "int", nullable: false),
                    HomeGoals = table.Column<int>(type: "int", nullable: false),
                    AwayGoals = table.Column<int>(type: "int", nullable: false),
                    GoalDifference = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballMatches", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FootballMatches");
        }
    }
}
