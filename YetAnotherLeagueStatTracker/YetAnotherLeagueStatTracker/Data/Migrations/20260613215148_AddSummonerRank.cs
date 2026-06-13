using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YetAnotherLeagueStatTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddSummonerRank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SummonerRanks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Time = table.Column<long>(type: "INTEGER", nullable: false),
                    QueueType = table.Column<string>(type: "TEXT", nullable: false),
                    SummonerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tier = table.Column<string>(type: "TEXT", nullable: false),
                    Rank = table.Column<string>(type: "TEXT", nullable: true),
                    LeaguePoints = table.Column<int>(type: "INTEGER", nullable: false),
                    Wins = table.Column<int>(type: "INTEGER", nullable: false),
                    Losses = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SummonerRanks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SummonerRanks_Summoners_SummonerId",
                        column: x => x.SummonerId,
                        principalTable: "Summoners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SummonerRanks_SummonerId",
                table: "SummonerRanks",
                column: "SummonerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SummonerRanks");
        }
    }
}
