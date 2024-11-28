using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.SystemMigrations
{
    /// <inheritdoc />
    public partial class SystemConfTypoFixMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimeBetweenDataUpdates",
                table: "SystemConfiguration",
                newName: "TimeBetweenDataUpdatesSeconds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimeBetweenDataUpdatesSeconds",
                table: "SystemConfiguration",
                newName: "TimeBetweenDataUpdates");
        }
    }
}
