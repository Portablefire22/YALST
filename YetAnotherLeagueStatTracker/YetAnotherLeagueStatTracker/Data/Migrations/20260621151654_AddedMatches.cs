using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YetAnotherLeagueStatTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedMatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DataVersion = table.Column<string>(type: "TEXT", nullable: false),
                    GameVersion = table.Column<string>(type: "TEXT", nullable: false),
                    MatchId = table.Column<string>(type: "TEXT", nullable: false),
                    EndOfGameResult = table.Column<string>(type: "TEXT", nullable: false),
                    GameCreation = table.Column<long>(type: "INTEGER", nullable: false),
                    GameDuration = table.Column<long>(type: "INTEGER", nullable: false),
                    GameEndTimestamp = table.Column<long>(type: "INTEGER", nullable: false),
                    GameId = table.Column<long>(type: "INTEGER", nullable: false),
                    GameMode = table.Column<string>(type: "TEXT", nullable: false),
                    GameName = table.Column<string>(type: "TEXT", nullable: false),
                    GameStartTimestamp = table.Column<long>(type: "INTEGER", nullable: false),
                    GameType = table.Column<string>(type: "TEXT", nullable: false),
                    MapId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlatformId = table.Column<string>(type: "TEXT", nullable: false),
                    QueueId = table.Column<int>(type: "INTEGER", nullable: false),
                    TournamentCode = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MatchParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MatchId = table.Column<int>(type: "INTEGER", nullable: false),
                    SummonerId = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamId = table.Column<int>(type: "INTEGER", nullable: false),
                    Assists = table.Column<int>(type: "INTEGER", nullable: false),
                    ChampionLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    ChampionName = table.Column<string>(type: "TEXT", nullable: false),
                    ChampionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChampionTransform = table.Column<int>(type: "INTEGER", nullable: false),
                    DamageDealtToBuildings = table.Column<int>(type: "INTEGER", nullable: false),
                    DamageDealtToObjectives = table.Column<int>(type: "INTEGER", nullable: false),
                    DamageSelfMitigated = table.Column<int>(type: "INTEGER", nullable: false),
                    Deaths = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstBlood = table.Column<bool>(type: "INTEGER", nullable: false),
                    FirstTowerKill = table.Column<bool>(type: "INTEGER", nullable: false),
                    GoldEarned = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamPosition = table.Column<string>(type: "TEXT", nullable: false),
                    Item0 = table.Column<int>(type: "INTEGER", nullable: false),
                    Item1 = table.Column<int>(type: "INTEGER", nullable: false),
                    Item2 = table.Column<int>(type: "INTEGER", nullable: false),
                    Item3 = table.Column<int>(type: "INTEGER", nullable: false),
                    Item4 = table.Column<int>(type: "INTEGER", nullable: false),
                    Item5 = table.Column<int>(type: "INTEGER", nullable: false),
                    Item6 = table.Column<int>(type: "INTEGER", nullable: false),
                    Kills = table.Column<int>(type: "INTEGER", nullable: false),
                    LargestMultiKill = table.Column<int>(type: "INTEGER", nullable: false),
                    MagicDamageDealtToChampions = table.Column<int>(type: "INTEGER", nullable: false),
                    PhysicalDamageDealtToChampions = table.Column<int>(type: "INTEGER", nullable: false),
                    TrueDamageDealtToChampions = table.Column<int>(type: "INTEGER", nullable: false),
                    Placement = table.Column<int>(type: "INTEGER", nullable: false),
                    SubteamPlacement = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerAugment1 = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerAugment2 = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerAugment3 = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerAugment4 = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerSubteamId = table.Column<int>(type: "INTEGER", nullable: false),
                    Summoner1Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Summoner2Id = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalDamageTaken = table.Column<int>(type: "INTEGER", nullable: false),
                    VisionScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Win = table.Column<bool>(type: "INTEGER", nullable: false),
                    MainRune = table.Column<int>(type: "INTEGER", nullable: false),
                    SubRune = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchParticipants_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchParticipants_Summoners_SummonerId",
                        column: x => x.SummonerId,
                        principalTable: "Summoners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchParticipants_MatchId",
                table: "MatchParticipants",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchParticipants_SummonerId",
                table: "MatchParticipants",
                column: "SummonerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchParticipants");

            migrationBuilder.DropTable(
                name: "Matches");
        }
    }
}
