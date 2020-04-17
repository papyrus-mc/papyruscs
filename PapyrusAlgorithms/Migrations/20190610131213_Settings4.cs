using Microsoft.EntityFrameworkCore.Migrations;

namespace PapyrusCs.Migrations
{
    public partial class Settings4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Zoom",
                table: "Settings",
                newName: "MaxZoom");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaxZoom",
                table: "Settings",
                newName: "Zoom");
        }
    }
}
