using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.Finance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancePostingAndTaxProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyId",
                schema: "finance",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "OriginalAmount",
                schema: "finance",
                table: "JournalEntryLines");

            migrationBuilder.RenameColumn(
                name: "Debit",
                schema: "finance",
                table: "JournalEntryLines",
                newName: "TransactionDebit");

            migrationBuilder.RenameColumn(
                name: "Credit",
                schema: "finance",
                table: "JournalEntryLines",
                newName: "TransactionCredit");

            migrationBuilder.RenameColumn(
                name: "TotalDebit",
                schema: "finance",
                table: "JournalEntries",
                newName: "TotalTransactionDebit");

            migrationBuilder.RenameColumn(
                name: "TotalCredit",
                schema: "finance",
                table: "JournalEntries",
                newName: "TotalTransactionCredit");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                schema: "finance",
                table: "TaxRates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                schema: "finance",
                table: "JournalEntryLines",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseCredit",
                schema: "finance",
                table: "JournalEntryLines",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "BaseCurrencyId",
                schema: "finance",
                table: "JournalEntryLines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "BaseDebit",
                schema: "finance",
                table: "JournalEntryLines",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionCurrencyId",
                schema: "finance",
                table: "JournalEntryLines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ReversesJournalEntryId",
                schema: "finance",
                table: "JournalEntries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalBaseCredit",
                schema: "finance",
                table: "JournalEntries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalBaseDebit",
                schema: "finance",
                table: "JournalEntries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsTaxDeductible",
                schema: "finance",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresCostCenter",
                schema: "finance",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresPartner",
                schema: "finance",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PostingProfiles",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_PostingProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxProfiles",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_TaxProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PostingProfileLines",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostingProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostingProfileLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostingProfileLines_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "finance",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostingProfileLines_PostingProfiles_PostingProfileId",
                        column: x => x.PostingProfileId,
                        principalSchema: "finance",
                        principalTable: "PostingProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaxProfileLines",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaxProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaxRateId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsInclusive = table.Column<bool>(type: "boolean", nullable: false),
                    CalculationOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxProfileLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxProfileLines_TaxProfiles_TaxProfileId",
                        column: x => x.TaxProfileId,
                        principalSchema: "finance",
                        principalTable: "TaxProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaxProfileLines_TaxRates_TaxRateId",
                        column: x => x.TaxRateId,
                        principalSchema: "finance",
                        principalTable: "TaxRates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostingProfileLines_AccountId",
                schema: "finance",
                table: "PostingProfileLines",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PostingProfileLines_PostingProfileId",
                schema: "finance",
                table: "PostingProfileLines",
                column: "PostingProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxProfileLines_TaxProfileId",
                schema: "finance",
                table: "TaxProfileLines",
                column: "TaxProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxProfileLines_TaxRateId",
                schema: "finance",
                table: "TaxProfileLines",
                column: "TaxRateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostingProfileLines",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "TaxProfileLines",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "PostingProfiles",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "TaxProfiles",
                schema: "finance");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "finance",
                table: "TaxRates");

            migrationBuilder.DropColumn(
                name: "BaseCredit",
                schema: "finance",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "BaseCurrencyId",
                schema: "finance",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "BaseDebit",
                schema: "finance",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "TransactionCurrencyId",
                schema: "finance",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "ReversesJournalEntryId",
                schema: "finance",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "TotalBaseCredit",
                schema: "finance",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "TotalBaseDebit",
                schema: "finance",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "IsTaxDeductible",
                schema: "finance",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "RequiresCostCenter",
                schema: "finance",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "RequiresPartner",
                schema: "finance",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "TransactionDebit",
                schema: "finance",
                table: "JournalEntryLines",
                newName: "Debit");

            migrationBuilder.RenameColumn(
                name: "TransactionCredit",
                schema: "finance",
                table: "JournalEntryLines",
                newName: "Credit");

            migrationBuilder.RenameColumn(
                name: "TotalTransactionDebit",
                schema: "finance",
                table: "JournalEntries",
                newName: "TotalDebit");

            migrationBuilder.RenameColumn(
                name: "TotalTransactionCredit",
                schema: "finance",
                table: "JournalEntries",
                newName: "TotalCredit");

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                schema: "finance",
                table: "JournalEntryLines",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<Guid>(
                name: "CurrencyId",
                schema: "finance",
                table: "JournalEntryLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalAmount",
                schema: "finance",
                table: "JournalEntryLines",
                type: "numeric",
                nullable: true);
        }
    }
}
