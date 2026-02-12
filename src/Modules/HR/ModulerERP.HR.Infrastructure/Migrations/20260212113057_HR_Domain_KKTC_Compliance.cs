using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HR_Domain_KKTC_Compliance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaxDeduction",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.RenameColumn(
                name: "OvertimeMins",
                schema: "hr",
                table: "DailyAttendances",
                newName: "Source");

            migrationBuilder.AddColumn<decimal>(
                name: "Bonus",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IncomeTax",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProvidentFundEmployee",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProvidentFundEmployer",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SocialSecurityEmployee",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SocialSecurityEmployer",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportationAllowance",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnemploymentInsuranceEmployer",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                schema: "hr",
                table: "Employees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Citizenship",
                schema: "hr",
                table: "Employees",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WorkPermitNumber",
                schema: "hr",
                table: "Employees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MatchedProjectId",
                schema: "hr",
                table: "DailyAttendances",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NormalMins",
                schema: "hr",
                table: "DailyAttendances",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Overtime1xMins",
                schema: "hr",
                table: "DailyAttendances",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Overtime2xMins",
                schema: "hr",
                table: "DailyAttendances",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bonus",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "IncomeTax",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "ProvidentFundEmployee",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "ProvidentFundEmployer",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "SocialSecurityEmployee",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "SocialSecurityEmployer",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "TransportationAllowance",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "UnemploymentInsuranceEmployer",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "BankName",
                schema: "hr",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Citizenship",
                schema: "hr",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "WorkPermitNumber",
                schema: "hr",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "MatchedProjectId",
                schema: "hr",
                table: "DailyAttendances");

            migrationBuilder.DropColumn(
                name: "NormalMins",
                schema: "hr",
                table: "DailyAttendances");

            migrationBuilder.DropColumn(
                name: "Overtime1xMins",
                schema: "hr",
                table: "DailyAttendances");

            migrationBuilder.DropColumn(
                name: "Overtime2xMins",
                schema: "hr",
                table: "DailyAttendances");

            migrationBuilder.RenameColumn(
                name: "Source",
                schema: "hr",
                table: "DailyAttendances",
                newName: "OvertimeMins");

            migrationBuilder.AddColumn<decimal>(
                name: "TaxDeduction",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
