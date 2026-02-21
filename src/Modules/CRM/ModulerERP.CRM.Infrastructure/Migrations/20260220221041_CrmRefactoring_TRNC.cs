using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP.CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CrmRefactoring_TRNC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommissionRules",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "PartnerBalances",
                schema: "crm");

            migrationBuilder.DropColumn(
                name: "CreditLimit",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "PaymentTermDays",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "EntityId",
                schema: "crm",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "EntityType",
                schema: "crm",
                table: "Activities");

            migrationBuilder.AddColumn<Guid>(
                name: "CompetitorId",
                schema: "crm",
                table: "Opportunities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LossReasonId",
                schema: "crm",
                table: "Opportunities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TerritoryId",
                schema: "crm",
                table: "Opportunities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsentDate",
                schema: "crm",
                table: "Leads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsentSource",
                schema: "crm",
                table: "Leads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMarketingConsentGiven",
                schema: "crm",
                table: "Leads",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RejectionReasonId",
                schema: "crm",
                table: "Leads",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TerritoryId",
                schema: "crm",
                table: "Leads",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsentDate",
                schema: "crm",
                table: "Contacts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsentSource",
                schema: "crm",
                table: "Contacts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMarketingConsentGiven",
                schema: "crm",
                table: "Contacts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TerritoryId",
                schema: "crm",
                table: "BusinessPartners",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PartnerId",
                schema: "crm",
                table: "Activities",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Competitors",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Website = table.Column<string>(type: "text", nullable: true),
                    Strengths = table.Column<string>(type: "text", nullable: true),
                    Weaknesses = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Competitors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RejectionReasons",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_RejectionReasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Territories",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    RegionalManagerId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_Territories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_CompetitorId",
                schema: "crm",
                table: "Opportunities",
                column: "CompetitorId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_LossReasonId",
                schema: "crm",
                table: "Opportunities",
                column: "LossReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_TerritoryId",
                schema: "crm",
                table: "Opportunities",
                column: "TerritoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_RejectionReasonId",
                schema: "crm",
                table: "Leads",
                column: "RejectionReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_TerritoryId",
                schema: "crm",
                table: "Leads",
                column: "TerritoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPartners_TerritoryId",
                schema: "crm",
                table: "BusinessPartners",
                column: "TerritoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_PartnerId",
                schema: "crm",
                table: "Activities",
                column: "PartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_BusinessPartners_PartnerId",
                schema: "crm",
                table: "Activities",
                column: "PartnerId",
                principalSchema: "crm",
                principalTable: "BusinessPartners",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessPartners_Territories_TerritoryId",
                schema: "crm",
                table: "BusinessPartners",
                column: "TerritoryId",
                principalSchema: "crm",
                principalTable: "Territories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_RejectionReasons_RejectionReasonId",
                schema: "crm",
                table: "Leads",
                column: "RejectionReasonId",
                principalSchema: "crm",
                principalTable: "RejectionReasons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Territories_TerritoryId",
                schema: "crm",
                table: "Leads",
                column: "TerritoryId",
                principalSchema: "crm",
                principalTable: "Territories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Competitors_CompetitorId",
                schema: "crm",
                table: "Opportunities",
                column: "CompetitorId",
                principalSchema: "crm",
                principalTable: "Competitors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_RejectionReasons_LossReasonId",
                schema: "crm",
                table: "Opportunities",
                column: "LossReasonId",
                principalSchema: "crm",
                principalTable: "RejectionReasons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Territories_TerritoryId",
                schema: "crm",
                table: "Opportunities",
                column: "TerritoryId",
                principalSchema: "crm",
                principalTable: "Territories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_BusinessPartners_PartnerId",
                schema: "crm",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessPartners_Territories_TerritoryId",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_RejectionReasons_RejectionReasonId",
                schema: "crm",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Territories_TerritoryId",
                schema: "crm",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Competitors_CompetitorId",
                schema: "crm",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_RejectionReasons_LossReasonId",
                schema: "crm",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Territories_TerritoryId",
                schema: "crm",
                table: "Opportunities");

            migrationBuilder.DropTable(
                name: "Competitors",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "RejectionReasons",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "Territories",
                schema: "crm");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_CompetitorId",
                schema: "crm",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_LossReasonId",
                schema: "crm",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_TerritoryId",
                schema: "crm",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Leads_RejectionReasonId",
                schema: "crm",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_TerritoryId",
                schema: "crm",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_BusinessPartners_TerritoryId",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropIndex(
                name: "IX_Activities_PartnerId",
                schema: "crm",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CompetitorId",
                schema: "crm",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "LossReasonId",
                schema: "crm",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "TerritoryId",
                schema: "crm",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ConsentDate",
                schema: "crm",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ConsentSource",
                schema: "crm",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "IsMarketingConsentGiven",
                schema: "crm",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "RejectionReasonId",
                schema: "crm",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "TerritoryId",
                schema: "crm",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ConsentDate",
                schema: "crm",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "ConsentSource",
                schema: "crm",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "IsMarketingConsentGiven",
                schema: "crm",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "TerritoryId",
                schema: "crm",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                schema: "crm",
                table: "Activities");

            migrationBuilder.AddColumn<decimal>(
                name: "CreditLimit",
                schema: "crm",
                table: "BusinessPartners",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PaymentTermDays",
                schema: "crm",
                table: "BusinessPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "EntityId",
                schema: "crm",
                table: "Activities",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                schema: "crm",
                table: "Activities",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CommissionRules",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    Basis = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ProductCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Rate = table.Column<decimal>(type: "numeric", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "PartnerBalances",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false),
                    CurrencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartnerBalances_BusinessPartners_PartnerId",
                        column: x => x.PartnerId,
                        principalSchema: "crm",
                        principalTable: "BusinessPartners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommissionRules_PartnerGroupId",
                schema: "crm",
                table: "CommissionRules",
                column: "PartnerGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerBalances_PartnerId",
                schema: "crm",
                table: "PartnerBalances",
                column: "PartnerId");
        }
    }
}
