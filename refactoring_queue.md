# ERP Refactoring Queue (Hierarchical Dependency Order)

This queue defines the optimal sequence for refactoring the remaining ERP modules, ordered precisely by how their domain entities and bounded contexts influence downstream processes.

> [!IMPORTANT]
> **Core Principle:** Refactoring must flow from **upstream/foundational** domains to **downstream/consumer** domains. If we refactor a consumer module (like Sales) before its dependencies (like Inventory or Finance), we risk building on unstable contracts and having to rewrite integrations later.

---

## 1. HR (Human Resources & Payroll)
*Status: Up Next*

- **Why First:** The most independent bounded context.
- **Influences:**
  - `Sales` (Sale Agents / Commission allocations)
  - `Manufacturing` (Labor routing, standard human working costs)
  - `ProjectManagement` (Project Team Members, Resource allocations, time tracking)
- **Refactoring Focus:** Employee core structure, TRNC labor laws (tax brackets, social security rules), payroll engines.

## 2. Finance (General Ledger & Treasury)
- **Why Second:** The financial backbone of the entire ERP. It acts as the ultimate "sink" for all sub-ledgers.
- **Depends On:** None structurally (though it relates to [CRM](file:///c:/Users/Oxir/source/repos/ModulerERP/src/Modules/CRM/ModulerERP.CRM.Infrastructure/Persistence/CRMDbContext.cs#15-20) and `HR` via Domain Events for automated journals).
- **Influences:**
  - **Everything.**
  - `Inventory` (Valuation profiles, Cost of Goods Sold logic)
  - `Procurement` (Accounts Payable, payments to suppliers)
  - `Sales` (Accounts Receivable, invoicing, credit limits)
  - `FixedAssets` (Depreciation ledgers)
- **Refactoring Focus:** Multi-currency compliance (TRNC requirements), Chart of Accounts hierarchy, automated journal posting configurations.

## 3. Inventory (Stock & Logistics)
- **Why Third:** The physical backbone for all supply chain operations.
- **Depends On:** `Finance` (for COGS accounting configuration).
- **Influences:**
  - `Procurement` (Receiving goods into warehouses)
  - `Sales` (Shipping goods out of warehouses, reserving stock)
  - `Manufacturing` (Bill of Materials, consuming raw materials)
  - `ProjectManagement` (Deducting material costs for projects)
- **Refactoring Focus:** TRNC custom unit conversions, robust warehouse bin locations, tracking lot/serial numbers.

---
> [!NOTE]
> *At this stage, the three foundational pillars (People, Money, Goods) alongside the completed CRM (External Partners) are fully stable. We now move to the transactional/process modules.*
---

## 4. Procurement (Supply Chain Inbound)
- **Depends On:**
  - [CRM](file:///c:/Users/Oxir/source/repos/ModulerERP/src/Modules/CRM/ModulerERP.CRM.Infrastructure/Persistence/CRMDbContext.cs#15-20) (Suppliers [BusinessPartner](file:///c:/Users/Oxir/source/repos/ModulerERP/src/Modules/CRM/ModulerERP.CRM.Domain/Entities/BusinessPartner.cs#67-68) logic completed previously)
  - `Inventory` (Items to buy and receive)
  - `Finance` (Invoicing and Accounts Payable)
- **Influences:** Generates the inbound supply and AP liabilities.
- **Refactoring Focus:** Purchase Requisitions, Request for Quotation (RFQ) bidding logic, Quality Control processing.

## 5. Sales (Supply Chain Outbound)
- **Depends On:**
  - [CRM](file:///c:/Users/Oxir/source/repos/ModulerERP/src/Modules/CRM/ModulerERP.CRM.Infrastructure/Persistence/CRMDbContext.cs#15-20) (Customers [BusinessPartner](file:///c:/Users/Oxir/source/repos/ModulerERP/src/Modules/CRM/ModulerERP.CRM.Domain/Entities/BusinessPartner.cs#67-68), [Opportunity](file:///c:/Users/Oxir/source/repos/ModulerERP/src/Modules/CRM/ModulerERP.CRM.Domain/Entities/Opportunity.cs#11-122) logic completed previously)
  - `Inventory` (Items to sell and reserve)
  - `Finance` (Invoicing, Accounts Receivable, and Credit Control)
- **Influences:** Generates the outbound supply and AR assets.
- **Refactoring Focus:** Quote-to-Cash workflow, dispatch/shipping integrations, dynamic pricing rules.

## 6. Manufacturing (Production Engine)
- **Depends On:**
  - `Inventory` (Raw materials, Bill of Materials)
  - `HR` (Workstations, Labor operations)
  - `Finance` (Standard Costing vs. Actual Costing variance)
- **Influences:** Finished Goods inventory.
- **Refactoring Focus:** Work Orders, MRP (Material Requirements Planning), shop-floor execution tracking.

## 7. FixedAssets (Asset Management)
- **Depends On:**
  - `Finance` (Depreciation accounts, accumulated depreciation)
  - `Procurement` (Capitalizing purchased items via AP Invoices)
- **Influences:** None structurally, purely an internal management sink.
- **Refactoring Focus:** Straight-line vs. declining balance depreciation tailored to TRNC tax codes.

## 8. ProjectManagement (Enterprise Services)
- **Why Last:** The most complex aggregator module that bridges across almost all domains.
- **Depends On:**
  - `HR` (Assigning engineers and workers)
  - `Inventory` (Consuming project materials)
  - `Procurement` (Subcontracting services or direct-to-site purchasing)
  - `Sales` (Progress payments / "Hakediş" billing)
  - `Finance` (End-to-end profitability tracking)
- **Refactoring Focus:** Progress payment (Hakediş) retention calculations, phase-to-phase resource locking, overall project profitability reporting.
