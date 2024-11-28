using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.SystemMigrations
{
    /// <inheritdoc />
    public partial class LanguageUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "UserProfiles",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "UserProfiles");
        }
    }
}
