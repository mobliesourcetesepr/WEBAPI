using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiTenantApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminAuditTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "AdminAudits");

            migrationBuilder.DropColumn(
                name: "ChangeDetails",
                table: "AdminAudits");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "AdminAudits",
                newName: "OldData");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "AdminAudits",
                newName: "NewData");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "AdminAudits",
                newName: "ChangeType");

            migrationBuilder.AddColumn<int>(
                name: "AdminId",
                table: "AdminAudits",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminId",
                table: "AdminAudits");

            migrationBuilder.RenameColumn(
                name: "OldData",
                table: "AdminAudits",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "NewData",
                table: "AdminAudits",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "ChangeType",
                table: "AdminAudits",
                newName: "Email");

            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "AdminAudits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ChangeDetails",
                table: "AdminAudits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
