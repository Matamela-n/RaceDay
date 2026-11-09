using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RACEDAY_API_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class AddEventBannerImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "bannerImagePath",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bannerImagePath",
                table: "Events");
        }
    }
}
