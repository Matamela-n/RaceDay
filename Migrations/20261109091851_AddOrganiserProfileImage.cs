using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RACEDAY_API_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganiserProfileImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "profileImagePath",
                table: "EventOrganisers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "profileImagePath",
                table: "EventOrganisers");
        }
    }
}
