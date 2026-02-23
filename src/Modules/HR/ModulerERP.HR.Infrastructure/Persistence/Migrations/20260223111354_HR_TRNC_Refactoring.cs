using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.HR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HR_TRNC_Refactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bonus",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "TransportationAllowance",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "hr",
                table: "LeaveRequests");

            migrationBuilder.RenameColumn(
                name: "OvertimePay",
                schema: "hr",
                table: "PayrollEntries",
                newName: "TotalTaxableEarnings");

            migrationBuilder.RenameColumn(
                name: "CommissionPay",
                schema: "hr",
                table: "PayrollEntries",
                newName: "TotalSgkExemptEarnings");

            migrationBuilder.RenameColumn(
                name: "AdvanceDeduction",
                schema: "hr",
                table: "PayrollEntries",
                newName: "PersonalAllowanceDeduction");

            migrationBuilder.AddColumn<decimal>(
                name: "CumulativeTaxBaseBeforeThisPayroll",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StampTax",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "LeavePolicyId",
                schema: "hr",
                table: "LeaveRequests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "HealthReportExpDate",
                schema: "hr",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PassportExpDate",
                schema: "hr",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportNumber",
                schema: "hr",
                table: "Employees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SgkRiskProfileId",
                schema: "hr",
                table: "Employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentDepartmentId",
                schema: "hr",
                table: "Departments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SgkRiskProfileId",
                schema: "hr",
                table: "Departments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EarningDeductionTypes",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    IsTaxable = table.Column<bool>(type: "boolean", nullable: false),
                    IsSgkExempt = table.Column<bool>(type: "boolean", nullable: false),
                    MaxExemptAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_EarningDeductionTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeCumulatives",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    YtdTaxBase = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalSeveranceAccrual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PreviousEmployerNotation = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_EmployeeCumulatives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeCumulatives_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "hr",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrSettings",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_HrSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeavePolicies",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresSgkMissingDayCode = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultDays = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_LeavePolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SgkRiskProfiles",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmployerSgkMultiplier = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_SgkRiskProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollEntryDetails",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    EarningDeductionTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    PayrollEntryId1 = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_PayrollEntryDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollEntryDetails_EarningDeductionTypes_EarningDeductionT~",
                        column: x => x.EarningDeductionTypeId,
                        principalSchema: "hr",
                        principalTable: "EarningDeductionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollEntryDetails_PayrollEntries_PayrollEntryId",
                        column: x => x.PayrollEntryId,
                        principalSchema: "hr",
                        principalTable: "PayrollEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayrollEntryDetails_PayrollEntries_PayrollEntryId1",
                        column: x => x.PayrollEntryId1,
                        principalSchema: "hr",
                        principalTable: "PayrollEntries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LeaveAllocations",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeavePolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    TotalDaysAllocated = table.Column<int>(type: "integer", nullable: false),
                    DaysUsed = table.Column<int>(type: "integer", nullable: false),
                    AllocationReason = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_LeaveAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveAllocations_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "hr",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveAllocations_LeavePolicies_LeavePolicyId",
                        column: x => x.LeavePolicyId,
                        principalSchema: "hr",
                        principalTable: "LeavePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_LeavePolicyId",
                schema: "hr",
                table: "LeaveRequests",
                column: "LeavePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_SgkRiskProfileId",
                schema: "hr",
                table: "Employees",
                column: "SgkRiskProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ParentDepartmentId",
                schema: "hr",
                table: "Departments",
                column: "ParentDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_SgkRiskProfileId",
                schema: "hr",
                table: "Departments",
                column: "SgkRiskProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCumulatives_EmployeeId",
                schema: "hr",
                table: "EmployeeCumulatives",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrSettings_TenantId_Key",
                schema: "hr",
                table: "HrSettings",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveAllocations_EmployeeId_LeavePolicyId_Year",
                schema: "hr",
                table: "LeaveAllocations",
                columns: new[] { "EmployeeId", "LeavePolicyId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveAllocations_LeavePolicyId",
                schema: "hr",
                table: "LeaveAllocations",
                column: "LeavePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollEntryDetails_EarningDeductionTypeId",
                schema: "hr",
                table: "PayrollEntryDetails",
                column: "EarningDeductionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollEntryDetails_PayrollEntryId",
                schema: "hr",
                table: "PayrollEntryDetails",
                column: "PayrollEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollEntryDetails_PayrollEntryId1",
                schema: "hr",
                table: "PayrollEntryDetails",
                column: "PayrollEntryId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Departments_ParentDepartmentId",
                schema: "hr",
                table: "Departments",
                column: "ParentDepartmentId",
                principalSchema: "hr",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_SgkRiskProfiles_SgkRiskProfileId",
                schema: "hr",
                table: "Departments",
                column: "SgkRiskProfileId",
                principalSchema: "hr",
                principalTable: "SgkRiskProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_SgkRiskProfiles_SgkRiskProfileId",
                schema: "hr",
                table: "Employees",
                column: "SgkRiskProfileId",
                principalSchema: "hr",
                principalTable: "SgkRiskProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_LeavePolicies_LeavePolicyId",
                schema: "hr",
                table: "LeaveRequests",
                column: "LeavePolicyId",
                principalSchema: "hr",
                principalTable: "LeavePolicies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Departments_ParentDepartmentId",
                schema: "hr",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_SgkRiskProfiles_SgkRiskProfileId",
                schema: "hr",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_SgkRiskProfiles_SgkRiskProfileId",
                schema: "hr",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_LeavePolicies_LeavePolicyId",
                schema: "hr",
                table: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "EmployeeCumulatives",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "HrSettings",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "LeaveAllocations",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "PayrollEntryDetails",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "SgkRiskProfiles",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "LeavePolicies",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "EarningDeductionTypes",
                schema: "hr");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_LeavePolicyId",
                schema: "hr",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_Employees_SgkRiskProfileId",
                schema: "hr",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Departments_ParentDepartmentId",
                schema: "hr",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_SgkRiskProfileId",
                schema: "hr",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "CumulativeTaxBaseBeforeThisPayroll",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "StampTax",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "LeavePolicyId",
                schema: "hr",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "HealthReportExpDate",
                schema: "hr",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PassportExpDate",
                schema: "hr",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PassportNumber",
                schema: "hr",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "SgkRiskProfileId",
                schema: "hr",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ParentDepartmentId",
                schema: "hr",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "SgkRiskProfileId",
                schema: "hr",
                table: "Departments");

            migrationBuilder.RenameColumn(
                name: "TotalTaxableEarnings",
                schema: "hr",
                table: "PayrollEntries",
                newName: "OvertimePay");

            migrationBuilder.RenameColumn(
                name: "TotalSgkExemptEarnings",
                schema: "hr",
                table: "PayrollEntries",
                newName: "CommissionPay");

            migrationBuilder.RenameColumn(
                name: "PersonalAllowanceDeduction",
                schema: "hr",
                table: "PayrollEntries",
                newName: "AdvanceDeduction");

            migrationBuilder.AddColumn<decimal>(
                name: "Bonus",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportationAllowance",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                schema: "hr",
                table: "LeaveRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
