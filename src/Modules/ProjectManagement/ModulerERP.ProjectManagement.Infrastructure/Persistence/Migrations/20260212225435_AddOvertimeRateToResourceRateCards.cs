using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.ProjectManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOvertimeRateToResourceRateCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OvertimeRate",
                schema: "pm",
                table: "ResourceRateCards",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OvertimeRate",
                schema: "pm",
                table: "ResourceRateCards");
        }
    }
}
