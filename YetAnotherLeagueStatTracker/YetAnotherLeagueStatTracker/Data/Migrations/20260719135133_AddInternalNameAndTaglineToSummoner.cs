using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YetAnotherLeagueStatTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInternalNameAndTaglineToSummoner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InternalName",
                table: "Summoners",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InternalTag",
                table: "Summoners",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InternalName",
                table: "Summoners");

            migrationBuilder.DropColumn(
                name: "InternalTag",
                table: "Summoners");
        }
    }
}
