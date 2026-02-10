using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.ProjectManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProgressPaymentDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProgressPaymentDetails",
                schema: "pm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgressPaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    BillOfQuantitiesItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousCumulativeQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CumulativeQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_ProgressPaymentDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressPaymentDetails_BillOfQuantitiesItems_BillOfQuantiti~",
                        column: x => x.BillOfQuantitiesItemId,
                        principalSchema: "pm",
                        principalTable: "BillOfQuantitiesItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgressPaymentDetails_ProgressPayments_ProgressPaymentId",
                        column: x => x.ProgressPaymentId,
                        principalSchema: "pm",
                        principalTable: "ProgressPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressPaymentDetails_BillOfQuantitiesItemId",
                schema: "pm",
                table: "ProgressPaymentDetails",
                column: "BillOfQuantitiesItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressPaymentDetails_ProgressPaymentId",
                schema: "pm",
                table: "ProgressPaymentDetails",
                column: "ProgressPaymentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgressPaymentDetails",
                schema: "pm");
        }
    }
}
