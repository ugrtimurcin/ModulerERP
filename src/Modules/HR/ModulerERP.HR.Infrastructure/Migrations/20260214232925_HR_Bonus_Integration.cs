using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HR_Bonus_Integration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SocialSecurityType",
                schema: "hr",
                table: "SocialSecurityRules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "UnemploymentInsuranceEmployeeRate",
                schema: "hr",
                table: "SocialSecurityRules",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnemploymentInsuranceEmployerRate",
                schema: "hr",
                table: "SocialSecurityRules",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnemploymentInsuranceEmployee",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SocialSecurityType",
                schema: "hr",
                table: "Employees",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeducted",
                schema: "hr",
                table: "AdvanceRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                schema: "hr",
                table: "AdvanceRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RepaymentDate",
                schema: "hr",
                table: "AdvanceRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Bonuses",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Period = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_Bonuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bonuses_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "hr",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bonuses_EmployeeId",
                schema: "hr",
                table: "Bonuses",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bonuses",
                schema: "hr");

            migrationBuilder.DropColumn(
                name: "SocialSecurityType",
                schema: "hr",
                table: "SocialSecurityRules");

            migrationBuilder.DropColumn(
                name: "UnemploymentInsuranceEmployeeRate",
                schema: "hr",
                table: "SocialSecurityRules");

            migrationBuilder.DropColumn(
                name: "UnemploymentInsuranceEmployerRate",
                schema: "hr",
                table: "SocialSecurityRules");

            migrationBuilder.DropColumn(
                name: "UnemploymentInsuranceEmployee",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "SocialSecurityType",
                schema: "hr",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IsDeducted",
                schema: "hr",
                table: "AdvanceRequests");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                schema: "hr",
                table: "AdvanceRequests");

            migrationBuilder.DropColumn(
                name: "RepaymentDate",
                schema: "hr",
                table: "AdvanceRequests");
        }
    }
}
