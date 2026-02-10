using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.ProjectManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnterpriseV2_Domain_Refined : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectBudgetLines",
                schema: "pm");

            migrationBuilder.DropColumn(
                name: "CurrentAmount",
                schema: "pm",
                table: "ProgressPayments");

            migrationBuilder.DropColumn(
                name: "TaxWithholdingAmount",
                schema: "pm",
                table: "ProgressPayments");

            migrationBuilder.AlterColumn<decimal>(
                name: "ContractAmount",
                schema: "pm",
                table: "Projects",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultRetentionRate",
                schema: "pm",
                table: "Projects",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultWithholdingTaxRate",
                schema: "pm",
                table: "Projects",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SiteAddress_Block",
                schema: "pm",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteAddress_City",
                schema: "pm",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteAddress_Country",
                schema: "pm",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteAddress_District",
                schema: "pm",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteAddress_Parcel",
                schema: "pm",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteAddress_Street",
                schema: "pm",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteAddress_ZipCode",
                schema: "pm",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VirtualWarehouseId",
                schema: "pm",
                table: "Projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "RetentionRate",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "RetentionAmount",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "PreviousCumulativeAmount",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "NetPayableAmount",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "MaterialOnSiteAmount",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "AdvanceDeductionAmount",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<decimal>(
                name: "CumulativeTotalAmount",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossWorkAmount",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsExpense",
                schema: "pm",
                table: "ProgressPayments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PeriodDeltaAmount",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodEnd",
                schema: "pm",
                table: "ProgressPayments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodStart",
                schema: "pm",
                table: "ProgressPayments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "WithholdingTaxAmount",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WithholdingTaxRate",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "BillOfQuantitiesItems",
                schema: "pm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ItemCode = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractUnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EstimatedUnitCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_BillOfQuantitiesItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillOfQuantitiesItems_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "pm",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyLogs",
                schema: "pm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WeatherCondition = table.Column<string>(type: "text", nullable: false),
                    SiteManagerNote = table.Column<string>(type: "text", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_DailyLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaterialRequests",
                schema: "pm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestNo = table.Column<int>(type: "integer", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MaterialRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectResources",
                schema: "pm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: true),
                    Role = table.Column<string>(type: "text", nullable: false),
                    HourlyCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_ProjectResources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubcontractorContracts",
                schema: "pm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubcontractorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceNo = table.Column<string>(type: "text", nullable: false),
                    ScopeOfWork = table.Column<string>(type: "text", nullable: false),
                    ContractAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_SubcontractorContracts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyLogMaterialUsages",
                schema: "pm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DailyLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uuid", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_DailyLogMaterialUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyLogMaterialUsages_DailyLogs_DailyLogId",
                        column: x => x.DailyLogId,
                        principalSchema: "pm",
                        principalTable: "DailyLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyLogResourceUsages",
                schema: "pm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DailyLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectTaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    HoursWorked = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_DailyLogResourceUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyLogResourceUsages_DailyLogs_DailyLogId",
                        column: x => x.DailyLogId,
                        principalSchema: "pm",
                        principalTable: "DailyLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaterialRequestItems",
                schema: "pm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequiredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_MaterialRequestItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialRequestItems_MaterialRequests_MaterialRequestId",
                        column: x => x.MaterialRequestId,
                        principalSchema: "pm",
                        principalTable: "MaterialRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillOfQuantitiesItems_ProjectId",
                schema: "pm",
                table: "BillOfQuantitiesItems",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyLogMaterialUsages_DailyLogId",
                schema: "pm",
                table: "DailyLogMaterialUsages",
                column: "DailyLogId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyLogResourceUsages_DailyLogId",
                schema: "pm",
                table: "DailyLogResourceUsages",
                column: "DailyLogId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequestItems_MaterialRequestId",
                schema: "pm",
                table: "MaterialRequestItems",
                column: "MaterialRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillOfQuantitiesItems",
                schema: "pm");

            migrationBuilder.DropTable(
                name: "DailyLogMaterialUsages",
                schema: "pm");

            migrationBuilder.DropTable(
                name: "DailyLogResourceUsages",
                schema: "pm");

            migrationBuilder.DropTable(
                name: "MaterialRequestItems",
                schema: "pm");

            migrationBuilder.DropTable(
                name: "ProjectResources",
                schema: "pm");

            migrationBuilder.DropTable(
                name: "SubcontractorContracts",
                schema: "pm");

            migrationBuilder.DropTable(
                name: "DailyLogs",
                schema: "pm");

            migrationBuilder.DropTable(
                name: "MaterialRequests",
                schema: "pm");

            migrationBuilder.DropColumn(
                name: "DefaultRetentionRate",
                schema: "pm",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DefaultWithholdingTaxRate",
                schema: "pm",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SiteAddress_Block",
                schema: "pm",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SiteAddress_City",
                schema: "pm",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SiteAddress_Country",
                schema: "pm",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SiteAddress_District",
                schema: "pm",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SiteAddress_Parcel",
                schema: "pm",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SiteAddress_Street",
                schema: "pm",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SiteAddress_ZipCode",
                schema: "pm",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "VirtualWarehouseId",
                schema: "pm",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CumulativeTotalAmount",
                schema: "pm",
                table: "ProgressPayments");

            migrationBuilder.DropColumn(
                name: "GrossWorkAmount",
                schema: "pm",
                table: "ProgressPayments");

            migrationBuilder.DropColumn(
                name: "IsExpense",
                schema: "pm",
                table: "ProgressPayments");

            migrationBuilder.DropColumn(
                name: "PeriodDeltaAmount",
                schema: "pm",
                table: "ProgressPayments");

            migrationBuilder.DropColumn(
                name: "PeriodEnd",
                schema: "pm",
                table: "ProgressPayments");

            migrationBuilder.DropColumn(
                name: "PeriodStart",
                schema: "pm",
                table: "ProgressPayments");

            migrationBuilder.DropColumn(
                name: "WithholdingTaxAmount",
                schema: "pm",
                table: "ProgressPayments");

            migrationBuilder.DropColumn(
                name: "WithholdingTaxRate",
                schema: "pm",
                table: "ProgressPayments");

            migrationBuilder.AlterColumn<decimal>(
                name: "ContractAmount",
                schema: "pm",
                table: "Projects",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "RetentionRate",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,4)",
                oldPrecision: 5,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "RetentionAmount",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "PreviousCumulativeAmount",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "NetPayableAmount",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaterialOnSiteAmount",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "AdvanceDeductionAmount",
                schema: "pm",
                table: "ProgressPayments",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentAmount",
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

            migrationBuilder.CreateTable(
                name: "ProjectBudgetLines",
                schema: "pm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    CostCode = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectBudgetLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectBudgetLines_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "pm",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBudgetLines_ProjectId",
                schema: "pm",
                table: "ProjectBudgetLines",
                column: "ProjectId");
        }
    }
}
