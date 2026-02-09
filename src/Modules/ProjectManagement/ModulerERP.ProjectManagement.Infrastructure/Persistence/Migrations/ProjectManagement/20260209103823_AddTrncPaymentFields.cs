using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.ProjectManagement.Infrastructure.Persistence.Migrations.ProjectManagement
{
    /// <inheritdoc />
    public partial class AddTrncPaymentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdvanceDeductionAmount",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialOnSiteAmount",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxWithholdingAmount",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdvanceDeductionAmount",
                schema: "pm",
                table: "ProgressPayments");

            migrationBuilder.DropColumn(
                name: "MaterialOnSiteAmount",
                schema: "pm",
                table: "ProgressPayments");

            migrationBuilder.DropColumn(
                name: "TaxWithholdingAmount",
                schema: "pm",
                table: "ProgressPayments");
        }
    }
}
