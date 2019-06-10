using Microsoft.EntityFrameworkCore.Migrations;

namespace PapyrusCs.Migrations
{
    public partial class Settings3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinZoom",
                table: "Settings",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinZoom",
                table: "Settings");
        }
    }
}
