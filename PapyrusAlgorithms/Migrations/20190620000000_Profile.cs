using Microsoft.EntityFrameworkCore.Migrations;

namespace PapyrusAlgorithms.Migrations
{
    public partial class Profile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Profile",
                table: "Checksums",
                defaultValue: "");
            migrationBuilder.AddColumn<string>(
                name: "Profile",
                table: "Settings",
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Profile",
                table: "Checksums");
            migrationBuilder.DropColumn(
                name: "Profile",
                table: "Settings");
        }
    }
}
