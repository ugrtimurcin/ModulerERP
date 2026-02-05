using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.Finance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveFinanceToSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "finance");

            migrationBuilder.RenameTable(
                name: "TaxRates",
                newName: "TaxRates",
                newSchema: "finance");

            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "Payments",
                newSchema: "finance");

            migrationBuilder.RenameTable(
                name: "PaymentAllocation",
                newName: "PaymentAllocation",
                newSchema: "finance");

            migrationBuilder.RenameTable(
                name: "JournalEntryLines",
                newName: "JournalEntryLines",
                newSchema: "finance");

            migrationBuilder.RenameTable(
                name: "JournalEntries",
                newName: "JournalEntries",
                newSchema: "finance");

            migrationBuilder.RenameTable(
                name: "FiscalPeriods",
                newName: "FiscalPeriods",
                newSchema: "finance");

            migrationBuilder.RenameTable(
                name: "ExchangeRates",
                newName: "ExchangeRates",
                newSchema: "finance");

            migrationBuilder.RenameTable(
                name: "CostCenters",
                newName: "CostCenters",
                newSchema: "finance");

            migrationBuilder.RenameTable(
                name: "Accounts",
                newName: "Accounts",
                newSchema: "finance");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "TaxRates",
                schema: "finance",
                newName: "TaxRates");

            migrationBuilder.RenameTable(
                name: "Payments",
                schema: "finance",
                newName: "Payments");

            migrationBuilder.RenameTable(
                name: "PaymentAllocation",
                schema: "finance",
                newName: "PaymentAllocation");

            migrationBuilder.RenameTable(
                name: "JournalEntryLines",
                schema: "finance",
                newName: "JournalEntryLines");

            migrationBuilder.RenameTable(
                name: "JournalEntries",
                schema: "finance",
                newName: "JournalEntries");

            migrationBuilder.RenameTable(
                name: "FiscalPeriods",
                schema: "finance",
                newName: "FiscalPeriods");

            migrationBuilder.RenameTable(
                name: "ExchangeRates",
                schema: "finance",
                newName: "ExchangeRates");

            migrationBuilder.RenameTable(
                name: "CostCenters",
                schema: "finance",
                newName: "CostCenters");

            migrationBuilder.RenameTable(
                name: "Accounts",
                schema: "finance",
                newName: "Accounts");
        }
    }
}
