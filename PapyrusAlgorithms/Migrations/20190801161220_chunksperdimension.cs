using Microsoft.EntityFrameworkCore.Migrations;

namespace PapyrusCs.Migrations
{
    public partial class chunksperdimension : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          

            migrationBuilder.AddColumn<int>(
                name: "ChunksPerDimension",
                table: "Settings",
                nullable: false,
                defaultValue: 2);

          
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChunksPerDimension",
                table: "Settings");

         
        }
    }
}
