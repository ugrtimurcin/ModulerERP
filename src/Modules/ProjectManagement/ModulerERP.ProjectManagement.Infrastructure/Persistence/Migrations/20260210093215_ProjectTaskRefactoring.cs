using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.ProjectManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProjectTaskRefactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedEmployeeId",
                schema: "pm",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "AssignedSubcontractorId",
                schema: "pm",
                table: "ProjectTasks");

            migrationBuilder.CreateTable(
                name: "ProjectTaskResources",
                schema: "pm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocationPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTaskResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectTaskResources_ProjectResources_ProjectResourceId",
                        column: x => x.ProjectResourceId,
                        principalSchema: "pm",
                        principalTable: "ProjectResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectTaskResources_ProjectTasks_ProjectTaskId",
                        column: x => x.ProjectTaskId,
                        principalSchema: "pm",
                        principalTable: "ProjectTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTaskResources_ProjectResourceId",
                schema: "pm",
                table: "ProjectTaskResources",
                column: "ProjectResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTaskResources_ProjectTaskId_ProjectResourceId",
                schema: "pm",
                table: "ProjectTaskResources",
                columns: new[] { "ProjectTaskId", "ProjectResourceId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectTaskResources",
                schema: "pm");

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedEmployeeId",
                schema: "pm",
                table: "ProjectTasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedSubcontractorId",
                schema: "pm",
                table: "ProjectTasks",
                type: "uuid",
                nullable: true);
        }
    }
}
