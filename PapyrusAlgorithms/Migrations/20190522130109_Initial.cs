using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PapyrusCs.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Checksums",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LevelDbKey = table.Column<byte[]>(nullable: true),
                    Crc32 = table.Column<uint>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checksums", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Checksums_LevelDbKey",
                table: "Checksums",
                column: "LevelDbKey");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Checksums");
        }
    }
}
