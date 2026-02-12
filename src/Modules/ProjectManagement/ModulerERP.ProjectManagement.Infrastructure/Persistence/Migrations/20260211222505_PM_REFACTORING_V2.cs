using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.ProjectManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PM_REFACTORING_V2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LocalCurrencyAmount",
                schema: "pm",
                table: "ProjectTransactions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProjectCurrencyAmount",
                schema: "pm",
                table: "ProjectTransactions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SourceType",
                schema: "pm",
                table: "ProjectTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "BudgetCurrencyId",
                schema: "pm",
                table: "Projects",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "LocalCurrencyId",
                schema: "pm",
                table: "Projects",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "AdvanceRepaymentAmount",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SecurityDepositAmount",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SecurityDepositRate",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ApprovedHours",
                schema: "pm",
                table: "DailyLogResourceUsages",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RawHours",
                schema: "pm",
                table: "DailyLogResourceUsages",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ValidationStatus",
                schema: "pm",
                table: "DailyLogResourceUsages",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalCurrencyAmount",
                schema: "pm",
                table: "ProjectTransactions");

            migrationBuilder.DropColumn(
                name: "ProjectCurrencyAmount",
                schema: "pm",
                table: "ProjectTransactions");

            migrationBuilder.DropColumn(
                name: "SourceType",
                schema: "pm",
                table: "ProjectTransactions");

            migrationBuilder.DropColumn(
                name: "BudgetCurrencyId",
                schema: "pm",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "LocalCurrencyId",
                schema: "pm",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "AdvanceRepaymentAmount",
                schema: "pm",
                table: "ProgressPayments");

            migrationBuilder.DropColumn(
                name: "SecurityDepositAmount",
                schema: "pm",
                table: "ProgressPayments");

            migrationBuilder.DropColumn(
                name: "SecurityDepositRate",
                schema: "pm",
                table: "ProgressPayments");

            migrationBuilder.DropColumn(
                name: "ApprovedHours",
                schema: "pm",
                table: "DailyLogResourceUsages");

            migrationBuilder.DropColumn(
                name: "RawHours",
                schema: "pm",
                table: "DailyLogResourceUsages");

            migrationBuilder.DropColumn(
                name: "ValidationStatus",
                schema: "pm",
                table: "DailyLogResourceUsages");
        }
    }
}
