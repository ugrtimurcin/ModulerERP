using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactoingOfCRMV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessPartners_BusinessPartnerGroups_BusinessPartnerGroup~",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessPartners_BusinessPartnerGroups_GroupId",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessPartners_BusinessPartners_ParentPartnerId",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropIndex(
                name: "IX_Tags_TenantId_Name",
                schema: "crm",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_PartnerBalances_TenantId_PartnerId_CurrencyId",
                schema: "crm",
                table: "PartnerBalances");

            migrationBuilder.DropIndex(
                name: "IX_EntityTags_TenantId_TagId_EntityId_EntityType",
                schema: "crm",
                table: "EntityTags");

            migrationBuilder.DropIndex(
                name: "IX_BusinessPartners_BusinessPartnerGroupId",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropIndex(
                name: "IX_BusinessPartners_TenantId",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "BusinessPartnerGroupId",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.AlterColumn<decimal>(
                name: "CommissionRate",
                schema: "crm",
                table: "SaleAgents",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "crm",
                table: "Opportunities",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedValue",
                schema: "crm",
                table: "Opportunities",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<string>(
                name: "EstimatedValueCurrency",
                schema: "crm",
                table: "Opportunities",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                schema: "crm",
                table: "Contacts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                schema: "crm",
                table: "Contacts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Block",
                schema: "crm",
                table: "Contacts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "crm",
                table: "Contacts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                schema: "crm",
                table: "Contacts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                schema: "crm",
                table: "Contacts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Parcel",
                schema: "crm",
                table: "Contacts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                schema: "crm",
                table: "Contacts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                schema: "crm",
                table: "Contacts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "crm",
                table: "BusinessPartners",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "crm",
                table: "BusinessPartners",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "BillingBlock",
                schema: "crm",
                table: "BusinessPartners",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingCity",
                schema: "crm",
                table: "BusinessPartners",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingCountry",
                schema: "crm",
                table: "BusinessPartners",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingDistrict",
                schema: "crm",
                table: "BusinessPartners",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingParcel",
                schema: "crm",
                table: "BusinessPartners",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingStreet",
                schema: "crm",
                table: "BusinessPartners",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingZipCode",
                schema: "crm",
                table: "BusinessPartners",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingBlock",
                schema: "crm",
                table: "BusinessPartners",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingCity",
                schema: "crm",
                table: "BusinessPartners",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingCountry",
                schema: "crm",
                table: "BusinessPartners",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingDistrict",
                schema: "crm",
                table: "BusinessPartners",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingParcel",
                schema: "crm",
                table: "BusinessPartners",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingStreet",
                schema: "crm",
                table: "BusinessPartners",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingZipCode",
                schema: "crm",
                table: "BusinessPartners",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CommissionRules",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartnerGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Basis = table.Column<int>(type: "integer", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_CommissionRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissionRules_BusinessPartnerGroups_PartnerGroupId",
                        column: x => x.PartnerGroupId,
                        principalSchema: "crm",
                        principalTable: "BusinessPartnerGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaleAgents_TenantId_EmployeeId",
                schema: "crm",
                table: "SaleAgents",
                columns: new[] { "TenantId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommissionRules_PartnerGroupId",
                schema: "crm",
                table: "CommissionRules",
                column: "PartnerGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessPartners_BusinessPartnerGroups_GroupId",
                schema: "crm",
                table: "BusinessPartners",
                column: "GroupId",
                principalSchema: "crm",
                principalTable: "BusinessPartnerGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessPartners_BusinessPartners_ParentPartnerId",
                schema: "crm",
                table: "BusinessPartners",
                column: "ParentPartnerId",
                principalSchema: "crm",
                principalTable: "BusinessPartners",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessPartners_BusinessPartnerGroups_GroupId",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessPartners_BusinessPartners_ParentPartnerId",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropTable(
                name: "CommissionRules",
                schema: "crm");

            migrationBuilder.DropIndex(
                name: "IX_SaleAgents_TenantId_EmployeeId",
                schema: "crm",
                table: "SaleAgents");

            migrationBuilder.DropColumn(
                name: "EstimatedValueCurrency",
                schema: "crm",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Block",
                schema: "crm",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "City",
                schema: "crm",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Country",
                schema: "crm",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "District",
                schema: "crm",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Parcel",
                schema: "crm",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Street",
                schema: "crm",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                schema: "crm",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "BillingBlock",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "BillingCity",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "BillingCountry",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "BillingDistrict",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "BillingParcel",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "BillingStreet",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "BillingZipCode",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "ShippingBlock",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "ShippingCity",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "ShippingCountry",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "ShippingDistrict",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "ShippingParcel",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "ShippingStreet",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "ShippingZipCode",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.AlterColumn<decimal>(
                name: "CommissionRate",
                schema: "crm",
                table: "SaleAgents",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "crm",
                table: "Opportunities",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedValue",
                schema: "crm",
                table: "Opportunities",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                schema: "crm",
                table: "Contacts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                schema: "crm",
                table: "Contacts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "crm",
                table: "BusinessPartners",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "crm",
                table: "BusinessPartners",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<Guid>(
                name: "BusinessPartnerGroupId",
                schema: "crm",
                table: "BusinessPartners",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TenantId_Name",
                schema: "crm",
                table: "Tags",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartnerBalances_TenantId_PartnerId_CurrencyId",
                schema: "crm",
                table: "PartnerBalances",
                columns: new[] { "TenantId", "PartnerId", "CurrencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntityTags_TenantId_TagId_EntityId_EntityType",
                schema: "crm",
                table: "EntityTags",
                columns: new[] { "TenantId", "TagId", "EntityId", "EntityType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPartners_BusinessPartnerGroupId",
                schema: "crm",
                table: "BusinessPartners",
                column: "BusinessPartnerGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPartners_TenantId",
                schema: "crm",
                table: "BusinessPartners",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessPartners_BusinessPartnerGroups_BusinessPartnerGroup~",
                schema: "crm",
                table: "BusinessPartners",
                column: "BusinessPartnerGroupId",
                principalSchema: "crm",
                principalTable: "BusinessPartnerGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessPartners_BusinessPartnerGroups_GroupId",
                schema: "crm",
                table: "BusinessPartners",
                column: "GroupId",
                principalSchema: "crm",
                principalTable: "BusinessPartnerGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessPartners_BusinessPartners_ParentPartnerId",
                schema: "crm",
                table: "BusinessPartners",
                column: "ParentPartnerId",
                principalSchema: "crm",
                principalTable: "BusinessPartners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
