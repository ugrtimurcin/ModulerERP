using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkShiftsAndAdvanceRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "hr",
                table: "WorkShifts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                schema: "hr",
                table: "AdvanceRequests",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "hr",
                table: "AdvanceRequests",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId1",
                schema: "hr",
                table: "AdvanceRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdvanceRequests_EmployeeId1",
                schema: "hr",
                table: "AdvanceRequests",
                column: "EmployeeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvanceRequests_Employees_EmployeeId1",
                schema: "hr",
                table: "AdvanceRequests",
                column: "EmployeeId1",
                principalSchema: "hr",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvanceRequests_Employees_EmployeeId1",
                schema: "hr",
                table: "AdvanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_AdvanceRequests_EmployeeId1",
                schema: "hr",
                table: "AdvanceRequests");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "hr",
                table: "AdvanceRequests");

            migrationBuilder.DropColumn(
                name: "EmployeeId1",
                schema: "hr",
                table: "AdvanceRequests");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "hr",
                table: "WorkShifts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                schema: "hr",
                table: "AdvanceRequests",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);
        }
    }
}
