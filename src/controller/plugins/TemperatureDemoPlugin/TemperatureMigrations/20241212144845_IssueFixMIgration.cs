using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TemperatureDemoPlugin.TemperatureMigrations
{
    /// <inheritdoc />
    public partial class IssueFixMIgration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Temperature",
                table: "DataEntries",
                newName: "TemperatureC");

            migrationBuilder.RenameColumn(
                name: "Humidity",
                table: "DataEntries",
                newName: "HumidityPercent");

            migrationBuilder.AddColumn<DateTime>(
                name: "CaptureTime",
                table: "DataEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaptureTime",
                table: "DataEntries");

            migrationBuilder.RenameColumn(
                name: "TemperatureC",
                table: "DataEntries",
                newName: "Temperature");

            migrationBuilder.RenameColumn(
                name: "HumidityPercent",
                table: "DataEntries",
                newName: "Humidity");
        }
    }
}
