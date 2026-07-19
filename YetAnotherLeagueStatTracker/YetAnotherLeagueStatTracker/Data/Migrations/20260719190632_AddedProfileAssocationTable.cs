using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YetAnotherLeagueStatTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedProfileAssocationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SummonerAssociations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    SummonerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SummonerAssociations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SummonerAssociations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SummonerAssociations_Summoners_SummonerId",
                        column: x => x.SummonerId,
                        principalTable: "Summoners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SummonerAssociations_SummonerId",
                table: "SummonerAssociations",
                column: "SummonerId");

            migrationBuilder.CreateIndex(
                name: "IX_SummonerAssociations_UserId",
                table: "SummonerAssociations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SummonerAssociations");
        }
    }
}
