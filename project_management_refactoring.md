# ACT AS: Senior ERP Architect & Full-Stack Developer (Approaching Principal Level)

# CONTEXT & GOAL
We are executing a "Deep Refactoring" of the `ProjectManagement` module in "ModulerERP". The target market is the **Turkish Republic of Northern Cyprus (TRNC/KKTC)**, which requires specific construction accounting logic (Green Book/Yeşil Defter), multi-currency handling (GBP/EUR/TRY), and strict resource controls.

**Your Mission:** Refactor the Backend (Domain/Infrastructure) and Frontend (React) to transform the current CRUD-based module into an Enterprise-grade Construction Management System.

---

# 1. DOMAIN LAYER REFACTORING (Backend)

### A. Update Aggregate Root: `Project.cs`
**File:** `src/Modules/ProjectManagement/ModulerERP.ProjectManagement.Domain/Entities/Project.cs`
- **Decouple from Sales:** Make `SalesOrderId` nullable. Allow "Standalone Projects" (manual entry).
- **New Fields:**
    - `ContractAmount` (decimal): Manual override for legacy projects.
    - `ContractCurrencyId` (Guid): The currency of the contract (usually GBP in TRNC).
    - `VirtualWarehouseId` (Guid): **CRITICAL.** Link to a dedicated `Warehouse` (Inventory Module) created automatically for this project site.
    - `SiteAddress` (Address ValueObject): District, City, Parcel/Block info.
    - **TRNC Financial Settings:**
        - `DefaultRetentionRate` (decimal): e.g., 0.10 (Teminat).
        - `DefaultWithholdingTaxRate` (decimal): e.g., 0.04 (Stopaj).

### B. Refactor Budget to BoQ: `BillOfQuantitiesItem.cs`
**Action:** Rename `ProjectBudgetLine` to `BillOfQuantitiesItem`.
- **Hierarchy:** Add `ParentId` (Guid?) to support WBS (Work Breakdown Structure) (e.g., Substructure -> Concrete).
- **Cost vs. Price (Profitability Logic):**
    - `ContractUnitPrice` (decimal): Price sold to customer (Income).
    - `EstimatedUnitCost` (decimal): Internal budget cost (Expense).
- **Identification:** Add `ItemCode` (string) (e.g., "15.120.1001").

### C. Refactor `ProgressPayment.cs` (The "Green Book" Logic)
**Context:** In TRNC, payments are calculated cumulatively.
- **New Fields:**
    - `PeriodStart` & `PeriodEnd` (Dates).
    - `GrossWorkAmount` (decimal): Total production value.
    - `MaterialOnSiteAmount` (decimal): **"İhzar"** (Value of materials delivered but not yet installed).
    - `CumulativeTotalAmount` (decimal): (GrossWork + MaterialOnSite) of *this* payment.
    - `PreviousCumulativeAmount` (decimal): Total from Payment #N-1.
    - **Deductions:**
        - `RetentionAmount` (decimal): Calculated from Period Delta.
        - `WithholdingTaxAmount` (decimal): Calculated from Period Delta.
        - `AdvanceDeductionAmount` (decimal).
    - `NetPayableAmount` (decimal): The final invoice amount.
    - `ApprovalStatus` (Enum): Draft -> PendingPM -> PendingFinance -> Approved.

### D. New Entities
1.  **`ProjectResource`:** Link `Employee` (HR) or `FixedAsset` (Machines) to `Project` with an `HourlyCost`.
2.  **`DailyLog` (Puantaj):**
    -   Root for daily site entries.
    -   **Collections:** `ResourceUsages` (Hours worked) and `MaterialUsages` (Items consumed from Virtual Warehouse).
3.  **`MaterialRequest` (MRF):**
    -   Site requests for materials. Has `PendingProjectManagerApproval` status before becoming a Purchase Requisition.
4.  **`SubcontractorContract`:**
    -   To manage external vendors (`SubcontractorId`), distinct from Customer Contracts.

---

# 2. BUSINESS LOGIC & SERVICES (Backend)

### A. `ProgressPaymentService.cs` (The Calculator)
Implement a robust `CalculatePayment` method that follows this algorithm:
1.  `CurrentTotal` = (Sum of Completed BoQ Quantities * UnitPrice) + `MaterialOnSite`.
2.  `PeriodDelta` = `CurrentTotal` - `PreviousPayment.CumulativeTotalAmount`.
3.  `Retention` = `PeriodDelta` * `Project.RetentionRate`.
4.  `Stopaj` = `PeriodDelta` * `Project.WithholdingTaxRate`.
5.  `NetPayable` = `PeriodDelta` - `Retention` - `Stopaj` - `AdvanceDeduction` + VAT.

### B. `ProjectService.cs` - Virtual Warehouse Logic
-   **On Project Creation:** Automatically call Inventory Module to create a new `Warehouse` named "Site: {ProjectName}".
-   **On Daily Log Approval:** Trigger `StockTransfer` (Main Warehouse -> Site Warehouse) OR `StockConsumption` (Site Warehouse -> Consumed).

### C. Domain Events
-   `PaymentApprovedEvent`: Triggers Finance Module (Create Invoice for Income, Create Bill for Expense).
-   `MaterialRequestApprovedEvent`: Triggers Procurement Module.

---

# 3. FRONTEND IMPLEMENTATION (React/TypeScript)

### A. Project Detail Page (Tabbed Interface)
Refactor `ProjectDetailPage.tsx` to use a clean Tab system:
1.  **Overview:** Dashboard with KPI Cards (Profit Margin, Completion %).
2.  **BoQ (Metraj):**
    -   Use a **TreeGrid** component.
    -   Columns: Item Code, Description, Unit, Qty, **Unit Cost**, **Unit Price**, Total.
    -   Feature: "Import from Excel".
3.  **Financials (Hakediş):**
    -   List of Payments.
    -   **"New Payment" Dialog:** Must show the "Green Book" calculation columns (Previous, Current, Cumulative) side-by-side.
4.  **Site Logs (Puantaj):**
    -   Calendar view for Daily Logs.
    -   Resource picker linked to HR module.
5.  **Material Requests:**
    -   Grid showing Request Status (Pending -> Ordered -> Delivered).

### B. UI Components
-   **`PaymentCalculator.tsx`:** A dedicated component/hook to handle the complex TRNC math on the client side before submitting.
-   **`ResourceSelector.tsx`:** A lookup component fetching Employees and Assets.

---

# 4. LOCALIZATION (i18n - HARD REQUIREMENT)

You **MUST** update `tr.json` and `en.json` for all current and new terms. Do not leave hardcoded strings. Use correct industry terminology.