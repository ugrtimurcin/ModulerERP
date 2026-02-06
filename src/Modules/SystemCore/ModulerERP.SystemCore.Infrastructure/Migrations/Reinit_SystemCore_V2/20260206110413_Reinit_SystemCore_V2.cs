using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.SystemCore.Infrastructure.Migrations.Reinit_SystemCore_V2
{
    /// <inheritdoc />
    public partial class Reinit_SystemCore_V2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolePermissions",
                schema: "system_core");

            migrationBuilder.DropTable(
                name: "Permissions",
                schema: "system_core");

            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.RenameTable(
                name: "Webhooks",
                schema: "system_core",
                newName: "Webhooks",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "UserSessions",
                schema: "system_core",
                newName: "UserSessions",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "system_core",
                newName: "Users",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                schema: "system_core",
                newName: "UserRoles",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "UserLoginHistory",
                schema: "system_core",
                newName: "UserLoginHistory",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "Translations",
                schema: "system_core",
                newName: "Translations",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "TenantSettings",
                schema: "system_core",
                newName: "TenantSettings",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "Tenants",
                schema: "system_core",
                newName: "Tenants",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "TenantFeatures",
                schema: "system_core",
                newName: "TenantFeatures",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "Roles",
                schema: "system_core",
                newName: "Roles",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "QueuedJobs",
                schema: "system_core",
                newName: "QueuedJobs",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "NotificationTemplates",
                schema: "system_core",
                newName: "NotificationTemplates",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "Notifications",
                schema: "system_core",
                newName: "Notifications",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "MediaFiles",
                schema: "system_core",
                newName: "MediaFiles",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "Languages",
                schema: "system_core",
                newName: "Languages",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "Currencies",
                schema: "system_core",
                newName: "Currencies",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "AuditLogs",
                schema: "system_core",
                newName: "AuditLogs",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "ApiKeys",
                schema: "system_core",
                newName: "ApiKeys",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "Addresses",
                schema: "system_core",
                newName: "Addresses",
                newSchema: "core");

            migrationBuilder.AddColumn<string>(
                name: "Permissions",
                schema: "core",
                table: "Roles",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Permissions",
                schema: "core",
                table: "Roles");

            migrationBuilder.EnsureSchema(
                name: "system_core");

            migrationBuilder.RenameTable(
                name: "Webhooks",
                schema: "core",
                newName: "Webhooks",
                newSchema: "system_core");

            migrationBuilder.RenameTable(
                name: "UserSessions",
                schema: "core",
                newName: "UserSessions",
                newSchema: "system_core");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "core",
                newName: "Users",
                newSchema: "system_core");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                schema: "core",
                newName: "UserRoles",
                newSchema: "system_core");

            migrationBuilder.RenameTable(
                name: "UserLoginHistory",
                schema: "core",
                newName: "UserLoginHistory",
                newSchema: "system_core");

            migrationBuilder.RenameTable(
                name: "Translations",
                schema: "core",
                newName: "Translations",
                newSchema: "system_core");

            migrationBuilder.RenameTable(
                name: "TenantSettings",
                schema: "core",
                newName: "TenantSettings",
                newSchema: "system_core");

            migrationBuilder.RenameTable(
                name: "Tenants",
                schema: "core",
                newName: "Tenants",
                newSchema: "system_core");

            migrationBuilder.RenameTable(
                name: "TenantFeatures",
                schema: "core",
                newName: "TenantFeatures",
                newSchema: "system_core");

            migrationBuilder.RenameTable(
                name: "Roles",
                schema: "core",
                newName: "Roles",
                newSchema: "system_core");

            migrationBuilder.RenameTable(
                name: "QueuedJobs",
                schema: "core",
                newName: "QueuedJobs",
                newSchema: "system_core");

            migrationBuilder.RenameTable(
                name: "NotificationTemplates",
                schema: "core",
                newName: "NotificationTemplates",
                newSchema: "system_core");

            migrationBuilder.RenameTable(
                name: "Notifications",
                schema: "core",
                newName: "Notifications",
                newSchema: "system_core");

            migrationBuilder.RenameTable(
                name: "MediaFiles",
                schema: "core",
                newName: "MediaFiles",
                newSchema: "system_core");

            migrationBuilder.RenameTable(
                name: "Languages",
                schema: "core",
                newName: "Languages",
                newSchema: "system_core");

            migrationBuilder.RenameTable(
                name: "Currencies",
                schema: "core",
                newName: "Currencies",
                newSchema: "system_core");

            migrationBuilder.RenameTable(
                name: "AuditLogs",
                schema: "core",
                newName: "AuditLogs",
                newSchema: "system_core");

            migrationBuilder.RenameTable(
                name: "ApiKeys",
                schema: "core",
                newName: "ApiKeys",
                newSchema: "system_core");

            migrationBuilder.RenameTable(
                name: "Addresses",
                schema: "core",
                newName: "Addresses",
                newSchema: "system_core");

            migrationBuilder.CreateTable(
                name: "Permissions",
                schema: "system_core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsScopeable = table.Column<bool>(type: "boolean", nullable: false),
                    ModuleName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                schema: "system_core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Scope = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "system_core",
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "system_core",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                schema: "system_core",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                schema: "system_core",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                schema: "system_core",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);
        }
    }
}
