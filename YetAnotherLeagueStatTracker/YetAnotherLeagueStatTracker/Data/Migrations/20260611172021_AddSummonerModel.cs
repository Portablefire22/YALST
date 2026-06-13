using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YetAnotherLeagueStatTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddSummonerModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Summoners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Puuid = table.Column<string>(type: "TEXT", nullable: false),
                    SummonerLevel = table.Column<long>(type: "INTEGER", nullable: false),
                    GameName = table.Column<string>(type: "TEXT", nullable: false),
                    TagLine = table.Column<string>(type: "TEXT", nullable: false),
                    Region = table.Column<string>(type: "TEXT", nullable: false),
                    ProfileIconId = table.Column<int>(type: "INTEGER", nullable: false),
                    RevisionDate = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Summoners", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Summoners");
        }
    }
}
