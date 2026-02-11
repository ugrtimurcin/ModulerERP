using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.Procurement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                schema: "procurement",
                table: "SupplierBillLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectTaskId",
                schema: "procurement",
                table: "SupplierBillLines",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectId",
                schema: "procurement",
                table: "SupplierBillLines");

            migrationBuilder.DropColumn(
                name: "ProjectTaskId",
                schema: "procurement",
                table: "SupplierBillLines");

            migrationBuilder.EnsureSchema(
                name: "system_core");
        }
    }
}
