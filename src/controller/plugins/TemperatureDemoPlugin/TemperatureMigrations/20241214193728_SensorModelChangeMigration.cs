using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TemperatureDemoPlugin.TemperatureMigrations
{
    /// <inheritdoc />
    public partial class SensorModelChangeMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Sensors",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Sensors");
        }
    }
}
