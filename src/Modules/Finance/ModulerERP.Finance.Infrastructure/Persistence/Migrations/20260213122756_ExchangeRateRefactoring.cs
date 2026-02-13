using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.Finance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExchangeRateRefactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BuyingRate",
                schema: "finance",
                table: "ExchangeRates",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SellingRate",
                schema: "finance",
                table: "ExchangeRates",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyingRate",
                schema: "finance",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "SellingRate",
                schema: "finance",
                table: "ExchangeRates");
        }
    }
}
