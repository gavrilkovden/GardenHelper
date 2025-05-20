using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataInputService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Plants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlantType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoilTemperature = table.Column<float>(type: "real", nullable: true),
                    SoilHumidity = table.Column<float>(type: "real", nullable: true),
                    SoilPh = table.Column<float>(type: "real", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plants", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Plants");
        }
    }
}
