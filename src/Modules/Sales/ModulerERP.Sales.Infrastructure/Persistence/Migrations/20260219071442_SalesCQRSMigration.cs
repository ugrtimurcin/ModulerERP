using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.Sales.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SalesCQRSMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PriceLists_TenantId_Code",
                schema: "sales",
                table: "PriceLists");

            migrationBuilder.DropIndex(
                name: "IX_PriceListItems_TenantId_PriceListId_ProductId_VariantId_Uni~",
                schema: "sales",
                table: "PriceListItems");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "sales",
                table: "PriceLists");

            migrationBuilder.DropColumn(
                name: "IsInternal",
                schema: "sales",
                table: "PriceLists");

            migrationBuilder.AddColumn<DateTime>(
                name: "DispatchDateTime",
                schema: "sales",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverName",
                schema: "sales",
                table: "Shipments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehiclePlate",
                schema: "sales",
                table: "Shipments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaybillNumber",
                schema: "sales",
                table: "Shipments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                schema: "sales",
                table: "Quotes",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AddColumn<decimal>(
                name: "DocumentDiscountAmount",
                schema: "sales",
                table: "Quotes",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DocumentDiscountRate",
                schema: "sales",
                table: "Quotes",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "LocalCurrencyId",
                schema: "sales",
                table: "Quotes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LocalExchangeRate",
                schema: "sales",
                table: "Quotes",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LocalSubTotal",
                schema: "sales",
                table: "Quotes",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LocalTaxAmount",
                schema: "sales",
                table: "Quotes",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LocalTotalAmount",
                schema: "sales",
                table: "Quotes",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WithholdingTaxAmount",
                schema: "sales",
                table: "Quotes",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WithholdingTaxRate",
                schema: "sales",
                table: "Quotes",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                schema: "sales",
                table: "QuoteLines",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "TaxRuleId",
                schema: "sales",
                table: "QuoteLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "sales",
                table: "PriceLists",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "sales",
                table: "PriceLists",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidFrom",
                schema: "sales",
                table: "PriceLists",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidTo",
                schema: "sales",
                table: "PriceLists",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinQuantity",
                schema: "sales",
                table: "PriceListItems",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                schema: "sales",
                table: "Orders",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AddColumn<decimal>(
                name: "DocumentDiscountAmount",
                schema: "sales",
                table: "Orders",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DocumentDiscountRate",
                schema: "sales",
                table: "Orders",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "LocalCurrencyId",
                schema: "sales",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LocalExchangeRate",
                schema: "sales",
                table: "Orders",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LocalSubTotal",
                schema: "sales",
                table: "Orders",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LocalTaxAmount",
                schema: "sales",
                table: "Orders",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LocalTotalAmount",
                schema: "sales",
                table: "Orders",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WithholdingTaxAmount",
                schema: "sales",
                table: "Orders",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WithholdingTaxRate",
                schema: "sales",
                table: "Orders",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "ShippedQuantity",
                schema: "sales",
                table: "OrderLines",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "InvoicedQuantity",
                schema: "sales",
                table: "OrderLines",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                schema: "sales",
                table: "OrderLines",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "TaxRuleId",
                schema: "sales",
                table: "OrderLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                schema: "sales",
                table: "Invoices",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AddColumn<decimal>(
                name: "DocumentDiscountAmount",
                schema: "sales",
                table: "Invoices",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DocumentDiscountRate",
                schema: "sales",
                table: "Invoices",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "LocalCurrencyId",
                schema: "sales",
                table: "Invoices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LocalExchangeRate",
                schema: "sales",
                table: "Invoices",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LocalSubTotal",
                schema: "sales",
                table: "Invoices",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LocalTaxAmount",
                schema: "sales",
                table: "Invoices",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LocalTotalAmount",
                schema: "sales",
                table: "Invoices",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WithholdingTaxAmount",
                schema: "sales",
                table: "Invoices",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WithholdingTaxRate",
                schema: "sales",
                table: "Invoices",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                schema: "sales",
                table: "InvoiceLines",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<Guid>(
                name: "TaxRuleId",
                schema: "sales",
                table: "InvoiceLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SalesPayments",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    AllocatedAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    AllocationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_SalesPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesPayments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "sales",
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesReturns",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReturnNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    LocalCurrencyId = table.Column<Guid>(type: "uuid", nullable: true),
                    LocalExchangeRate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    LocalTotalAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    LocalRefundAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RefundedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    RefundAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_SalesReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesReturns_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "sales",
                        principalTable: "Invoices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CreditNotes",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreditNoteNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesReturnId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    LocalCurrencyId = table.Column<Guid>(type: "uuid", nullable: true),
                    LocalExchangeRate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    LocalSubTotal = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    LocalTaxAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    LocalTotalAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreditNoteDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_CreditNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditNotes_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "sales",
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreditNotes_SalesReturns_SalesReturnId",
                        column: x => x.SalesReturnId,
                        principalSchema: "sales",
                        principalTable: "SalesReturns",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnLines",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesReturnId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReturnLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesReturnLines_SalesReturns_SalesReturnId",
                        column: x => x.SalesReturnId,
                        principalSchema: "sales",
                        principalTable: "SalesReturns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditNoteLines",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreditNoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditNoteLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditNoteLines_CreditNotes_CreditNoteId",
                        column: x => x.CreditNoteId,
                        principalSchema: "sales",
                        principalTable: "CreditNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreditNoteLines_CreditNoteId",
                schema: "sales",
                table: "CreditNoteLines",
                column: "CreditNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_CreditNoteNumber",
                schema: "sales",
                table: "CreditNotes",
                column: "CreditNoteNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_InvoiceId",
                schema: "sales",
                table: "CreditNotes",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_SalesReturnId",
                schema: "sales",
                table: "CreditNotes",
                column: "SalesReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesPayments_InvoiceId",
                schema: "sales",
                table: "SalesPayments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnLines_SalesReturnId",
                schema: "sales",
                table: "SalesReturnLines",
                column: "SalesReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_InvoiceId",
                schema: "sales",
                table: "SalesReturns",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_ReturnNumber",
                schema: "sales",
                table: "SalesReturns",
                column: "ReturnNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditNoteLines",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "SalesPayments",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "SalesReturnLines",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "CreditNotes",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "SalesReturns",
                schema: "sales");

            migrationBuilder.DropColumn(
                name: "DispatchDateTime",
                schema: "sales",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DriverName",
                schema: "sales",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "VehiclePlate",
                schema: "sales",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "WaybillNumber",
                schema: "sales",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DocumentDiscountAmount",
                schema: "sales",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "DocumentDiscountRate",
                schema: "sales",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "LocalCurrencyId",
                schema: "sales",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "LocalExchangeRate",
                schema: "sales",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "LocalSubTotal",
                schema: "sales",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "LocalTaxAmount",
                schema: "sales",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "LocalTotalAmount",
                schema: "sales",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "WithholdingTaxAmount",
                schema: "sales",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "WithholdingTaxRate",
                schema: "sales",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                schema: "sales",
                table: "QuoteLines");

            migrationBuilder.DropColumn(
                name: "TaxRuleId",
                schema: "sales",
                table: "QuoteLines");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "sales",
                table: "PriceLists");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "sales",
                table: "PriceLists");

            migrationBuilder.DropColumn(
                name: "ValidFrom",
                schema: "sales",
                table: "PriceLists");

            migrationBuilder.DropColumn(
                name: "ValidTo",
                schema: "sales",
                table: "PriceLists");

            migrationBuilder.DropColumn(
                name: "MinQuantity",
                schema: "sales",
                table: "PriceListItems");

            migrationBuilder.DropColumn(
                name: "DocumentDiscountAmount",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DocumentDiscountRate",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LocalCurrencyId",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LocalExchangeRate",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LocalSubTotal",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LocalTaxAmount",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LocalTotalAmount",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WithholdingTaxAmount",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WithholdingTaxRate",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                schema: "sales",
                table: "OrderLines");

            migrationBuilder.DropColumn(
                name: "TaxRuleId",
                schema: "sales",
                table: "OrderLines");

            migrationBuilder.DropColumn(
                name: "DocumentDiscountAmount",
                schema: "sales",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DocumentDiscountRate",
                schema: "sales",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "LocalCurrencyId",
                schema: "sales",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "LocalExchangeRate",
                schema: "sales",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "LocalSubTotal",
                schema: "sales",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "LocalTaxAmount",
                schema: "sales",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "LocalTotalAmount",
                schema: "sales",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "WithholdingTaxAmount",
                schema: "sales",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "WithholdingTaxRate",
                schema: "sales",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TaxRuleId",
                schema: "sales",
                table: "InvoiceLines");

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                schema: "sales",
                table: "Quotes",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "sales",
                table: "PriceLists",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsInternal",
                schema: "sales",
                table: "PriceLists",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                schema: "sales",
                table: "Orders",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "ShippedQuantity",
                schema: "sales",
                table: "OrderLines",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "InvoicedQuantity",
                schema: "sales",
                table: "OrderLines",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                schema: "sales",
                table: "Invoices",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                schema: "sales",
                table: "InvoiceLines",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.CreateIndex(
                name: "IX_PriceLists_TenantId_Code",
                schema: "sales",
                table: "PriceLists",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceListItems_TenantId_PriceListId_ProductId_VariantId_Uni~",
                schema: "sales",
                table: "PriceListItems",
                columns: new[] { "TenantId", "PriceListId", "ProductId", "VariantId", "UnitId" },
                unique: true);
        }
    }
}
