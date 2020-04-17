using Microsoft.EntityFrameworkCore.Migrations;

namespace PapyrusAlgorithms.Migrations
{
    public partial class Settings2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Zoom",
                table: "Settings",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Zoom",
                table: "Settings");
        }
    }
}
