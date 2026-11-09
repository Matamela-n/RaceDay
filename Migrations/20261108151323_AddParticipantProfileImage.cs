using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RACEDAY_API_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class AddParticipantProfileImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "profileImagePath",
                table: "Participants",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "profileImagePath",
                table: "Participants");
        }
    }
}
