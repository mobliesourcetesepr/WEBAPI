using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiTenantApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCanLoginCanUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanLogin",
                table: "SubAgents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanUpdate",
                table: "SubAgents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanLogin",
                table: "AppUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanUpdate",
                table: "AppUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanLogin",
                table: "SubAgents");

            migrationBuilder.DropColumn(
                name: "CanUpdate",
                table: "SubAgents");

            migrationBuilder.DropColumn(
                name: "CanLogin",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "CanUpdate",
                table: "AppUsers");
        }
    }
}
