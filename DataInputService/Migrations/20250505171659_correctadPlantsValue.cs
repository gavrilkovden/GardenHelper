using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataInputService.Migrations
{
    /// <inheritdoc />
    public partial class correctadPlantsValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoilHumidity",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "SoilPh",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "SoilTemperature",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Plants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "SoilHumidity",
                table: "Plants",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "SoilPh",
                table: "Plants",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "SoilTemperature",
                table: "Plants",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Timestamp",
                table: "Plants",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
