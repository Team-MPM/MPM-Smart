using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.SystemMigrations
{
    /// <inheritdoc />
    public partial class SystemSettingsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SystemName = table.Column<string>(nullable: false, defaultValue: "Controller"),
                    TimeZone = table.Column<string>(nullable: false, defaultValue: "UTC"),
                    TimeBetweenDataUpdates = table.Column<int>(nullable: false, defaultValue: 5)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfiguration", x => x.Id);
                });
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemConfiguration");
        }
    }
}
