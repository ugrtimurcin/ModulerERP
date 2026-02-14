using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConfiguredMoneyValueObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalCost",
                schema: "inventory",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                schema: "inventory",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "CostPrice",
                schema: "inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PurchasePrice",
                schema: "inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SalesPrice",
                schema: "inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TrackBatches",
                schema: "inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TrackInventory",
                schema: "inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TrackSerials",
                schema: "inventory",
                table: "Products");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                schema: "inventory",
                table: "Warehouses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                schema: "inventory",
                table: "Warehouses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "inventory",
                table: "StockTransferLines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                schema: "inventory",
                table: "StockTransferLines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "inventory",
                table: "StockTransferLines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                schema: "inventory",
                table: "StockTransferLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "inventory",
                table: "StockTransferLines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "inventory",
                table: "StockTransferLines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "inventory",
                table: "StockTransferLines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "inventory",
                table: "StockTransferLines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                schema: "inventory",
                table: "StockTransferLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCostAmount",
                schema: "inventory",
                table: "StockMovements",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TotalCostCurrency",
                schema: "inventory",
                table: "StockMovements",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCostAmount",
                schema: "inventory",
                table: "StockMovements",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitCostCurrency",
                schema: "inventory",
                table: "StockMovements",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CostPriceAmount",
                schema: "inventory",
                table: "Products",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CostPriceCurrency",
                schema: "inventory",
                table: "Products",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PurchasePriceAmount",
                schema: "inventory",
                table: "Products",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PurchasePriceCurrency",
                schema: "inventory",
                table: "Products",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SalesPriceAmount",
                schema: "inventory",
                table: "Products",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SalesPriceCurrency",
                schema: "inventory",
                table: "Products",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TrackingMethod",
                schema: "inventory",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectId",
                schema: "inventory",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "inventory",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "inventory",
                table: "StockTransferLines");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "inventory",
                table: "StockTransferLines");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "inventory",
                table: "StockTransferLines");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "inventory",
                table: "StockTransferLines");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "inventory",
                table: "StockTransferLines");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "inventory",
                table: "StockTransferLines");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "inventory",
                table: "StockTransferLines");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "inventory",
                table: "StockTransferLines");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "inventory",
                table: "StockTransferLines");

            migrationBuilder.DropColumn(
                name: "TotalCostAmount",
                schema: "inventory",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "TotalCostCurrency",
                schema: "inventory",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "UnitCostAmount",
                schema: "inventory",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "UnitCostCurrency",
                schema: "inventory",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "CostPriceAmount",
                schema: "inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CostPriceCurrency",
                schema: "inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PurchasePriceAmount",
                schema: "inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PurchasePriceCurrency",
                schema: "inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SalesPriceAmount",
                schema: "inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SalesPriceCurrency",
                schema: "inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TrackingMethod",
                schema: "inventory",
                table: "Products");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                schema: "inventory",
                table: "StockMovements",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                schema: "inventory",
                table: "StockMovements",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                schema: "inventory",
                table: "Products",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PurchasePrice",
                schema: "inventory",
                table: "Products",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SalesPrice",
                schema: "inventory",
                table: "Products",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "TrackBatches",
                schema: "inventory",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrackInventory",
                schema: "inventory",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrackSerials",
                schema: "inventory",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
