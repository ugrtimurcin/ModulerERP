using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixInventoryAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastUpdated",
                schema: "inventory",
                table: "StockLevels",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                schema: "inventory",
                table: "StockLevels",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "inventory",
                table: "StockLevels",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                schema: "inventory",
                table: "StockLevels",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "inventory",
                table: "StockLevels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "inventory",
                table: "StockLevels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "inventory",
                table: "StockLevels",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                schema: "inventory",
                table: "StockLevels",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "inventory",
                table: "StockLevels");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "inventory",
                table: "StockLevels");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "inventory",
                table: "StockLevels");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "inventory",
                table: "StockLevels");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "inventory",
                table: "StockLevels");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "inventory",
                table: "StockLevels");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "inventory",
                table: "StockLevels");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "inventory",
                table: "StockLevels",
                newName: "LastUpdated");
        }
    }
}
