using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class PluginEntityTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PluginInfoTag_Plugins_PluginInfoId",
                table: "PluginInfoTag");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Plugins");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Plugins");

            migrationBuilder.RenameColumn(
                name: "PluginInfoId",
                table: "PluginInfoTag",
                newName: "PluginEntryId");

            migrationBuilder.AddColumn<string>(
                name: "RegistryName",
                table: "Plugins",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_PluginInfoTag_Plugins_PluginEntryId",
                table: "PluginInfoTag",
                column: "PluginEntryId",
                principalTable: "Plugins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PluginInfoTag_Plugins_PluginEntryId",
                table: "PluginInfoTag");

            migrationBuilder.DropColumn(
                name: "RegistryName",
                table: "Plugins");

            migrationBuilder.RenameColumn(
                name: "PluginEntryId",
                table: "PluginInfoTag",
                newName: "PluginInfoId");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Plugins",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "Plugins",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_PluginInfoTag_Plugins_PluginInfoId",
                table: "PluginInfoTag",
                column: "PluginInfoId",
                principalTable: "Plugins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
