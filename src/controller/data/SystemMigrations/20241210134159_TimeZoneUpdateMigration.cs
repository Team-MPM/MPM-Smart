using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.SystemMigrations
{
    /// <inheritdoc />
    public partial class TimeZoneUpdateMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimeZone",
                table: "SystemConfiguration",
                newName: "TimeZoneCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimeZoneCode",
                table: "SystemConfiguration",
                newName: "TimeZone");
        }
    }
}
