using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialHRSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayrollEntries_Employees_EmployeeId",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.AlterColumn<string>(
                name: "Period",
                schema: "hr",
                table: "Payrolls",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "hr",
                table: "Payrolls",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxDeduction",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "OvertimePay",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "NetPayable",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "CommissionPay",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "BaseSalary",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "AdvanceDeduction",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId1",
                schema: "hr",
                table: "PayrollEntries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PayrollId1",
                schema: "hr",
                table: "PayrollEntries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                schema: "hr",
                table: "LeaveRequests",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId1",
                schema: "hr",
                table: "LeaveRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "hr",
                table: "Departments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                schema: "hr",
                table: "DailyAttendances",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId1",
                schema: "hr",
                table: "DailyAttendances",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollEntries_EmployeeId1",
                schema: "hr",
                table: "PayrollEntries",
                column: "EmployeeId1");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollEntries_PayrollId1",
                schema: "hr",
                table: "PayrollEntries",
                column: "PayrollId1");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_EmployeeId1",
                schema: "hr",
                table: "LeaveRequests",
                column: "EmployeeId1");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ManagerId",
                schema: "hr",
                table: "Departments",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyAttendances_EmployeeId1",
                schema: "hr",
                table: "DailyAttendances",
                column: "EmployeeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyAttendances_Employees_EmployeeId1",
                schema: "hr",
                table: "DailyAttendances",
                column: "EmployeeId1",
                principalSchema: "hr",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Employees_ManagerId",
                schema: "hr",
                table: "Departments",
                column: "ManagerId",
                principalSchema: "hr",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_Employees_EmployeeId1",
                schema: "hr",
                table: "LeaveRequests",
                column: "EmployeeId1",
                principalSchema: "hr",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollEntries_Employees_EmployeeId",
                schema: "hr",
                table: "PayrollEntries",
                column: "EmployeeId",
                principalSchema: "hr",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollEntries_Employees_EmployeeId1",
                schema: "hr",
                table: "PayrollEntries",
                column: "EmployeeId1",
                principalSchema: "hr",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollEntries_Payrolls_PayrollId1",
                schema: "hr",
                table: "PayrollEntries",
                column: "PayrollId1",
                principalSchema: "hr",
                principalTable: "Payrolls",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyAttendances_Employees_EmployeeId1",
                schema: "hr",
                table: "DailyAttendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Employees_ManagerId",
                schema: "hr",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_Employees_EmployeeId1",
                schema: "hr",
                table: "LeaveRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_PayrollEntries_Employees_EmployeeId",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_PayrollEntries_Employees_EmployeeId1",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_PayrollEntries_Payrolls_PayrollId1",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.DropIndex(
                name: "IX_PayrollEntries_EmployeeId1",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.DropIndex(
                name: "IX_PayrollEntries_PayrollId1",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_EmployeeId1",
                schema: "hr",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_Departments_ManagerId",
                schema: "hr",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_DailyAttendances_EmployeeId1",
                schema: "hr",
                table: "DailyAttendances");

            migrationBuilder.DropColumn(
                name: "EmployeeId1",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "PayrollId1",
                schema: "hr",
                table: "PayrollEntries");

            migrationBuilder.DropColumn(
                name: "EmployeeId1",
                schema: "hr",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "EmployeeId1",
                schema: "hr",
                table: "DailyAttendances");

            migrationBuilder.AlterColumn<string>(
                name: "Period",
                schema: "hr",
                table: "Payrolls",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(7)",
                oldMaxLength: 7);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "hr",
                table: "Payrolls",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxDeduction",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "OvertimePay",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "NetPayable",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "CommissionPay",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "BaseSalary",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "AdvanceDeduction",
                schema: "hr",
                table: "PayrollEntries",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                schema: "hr",
                table: "LeaveRequests",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "hr",
                table: "Departments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                schema: "hr",
                table: "DailyAttendances",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollEntries_Employees_EmployeeId",
                schema: "hr",
                table: "PayrollEntries",
                column: "EmployeeId",
                principalSchema: "hr",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
