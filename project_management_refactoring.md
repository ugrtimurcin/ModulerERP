# MISSION: PROJECT MANAGEMENT MODULE ENTERPRISE REFACTORING (FULL IMPLEMENTATION)

**ROLE:** You are a Senior Solution Architect and Lead Full-Stack Developer specializing in .NET 8 (Clean Architecture/DDD) and React (Vite/TypeScript).

**OBJECTIVE:** Perform a comprehensive, "Enterprise-Grade" refactoring of the **Project Management Module** in the provided `ModulerERP` repository. The goal is to meet the specific requirements of the **TRNC (KKTC) Construction Industry** and strict SaaS standards.

**CRITICAL RULES (NON-NEGOTIABLE):**
1.  **COMPLETE IMPLEMENTATION:** Do not leave comments like `// TODO: Implement logic here`. You must write the *full* working logic, including edge cases, validation, and error handling.
2.  **MODULE ISOLATION:** Work strictly within `Modules/ProjectManagement` and its corresponding API/Frontend folders. Do not break other modules.
3.  **MULTI-TENANCY:** Every new Entity and Query MUST implement/respect `IMultiTenant`. Data leakage is a failure.
4.  **LOCALIZATION:** All UI text must be extracted to `tr.json` and `en.json`. Hardcoded strings in UI are forbidden.
5.  **NO ASSUMPTIONS:** If a standard library (like MediatR or FluentValidation) is missing in a file, add it.

---

## PHASE 1: API & CONTROLLER RESTRUCTURING

**Goal:** Clean up the root `Controllers` folder and enforce a modular structure with CQRS.

1.  **Folder Structure:**
    * Move ALL Project Management related controllers to: `src/ModulerERP.Api/Controllers/ProjectManagement/`.
    * **Namespace:** `ModulerERP.Api.Controllers.ProjectManagement`.

2.  **Controller Definitions (Refactor & Create):**
    * **`ProjectsController.cs`**: CRUD, Status Changes (`/close`, `/suspend`).
    * **`DailyLogsController.cs`**:
        * `POST {id}/approve`: Triggers cost calculation.
        * `POST {id}/validate-attendance`: Compares generic HR logs with project logs to find anomalies.
    * **`ProgressPaymentsController.cs`** (Consolidate duplicate files):
        * `POST /calculate-draft`: Generates a payment draft based on "Green Book" (Yeşil Defter) quantities.
    * **`ProjectBoQController.cs`** (New):
        * `GET {projectId}/tree`: Returns hierarchical JSON for the TreeGrid.
    * **`ProjectTransactionsController.cs`**: Make strictly `ReadOnly` for system-generated transactions. Only allow `POST` for manual "Expenses".

3.  **Standardization:**
    * All endpoints must return `Result<T>` (Envelope pattern).
    * Inject `IMediator` strictly (Thin Controllers).

---

## PHASE 2: DOMAIN LAYER (ENTITIES & VALUE OBJECTS)

**Goal:** Deepen the domain to support KKTC Construction Laws.

1.  **`Project.cs`**:
    * Add `BudgetCurrencyId` (FK) and `LocalCurrencyId` (FK).
    * Ensure `ContractAmount` uses a Money Value Object (Amount + Currency).

2.  **`BillOfQuantitiesItem.cs` (WBS Support):**
    * Add `Guid? ParentId` for hierarchical structures (Block -> Floor -> Work Item).
    * Add methods to calculate subtree totals.

3.  **`DailyLog.cs` & `DailyLogResourceUsage.cs` (The "Puantaj" System):**
    * **Properties:**
        * `RawHours` (decimal): Data from HR/Barcode system (Read-only reference).
        * `ApprovedHours` (decimal): The actual billable hours approved by the Site Chief.
        * `ValidationStatus` Enum: `Pending`, `Verified`, `Anomaly` (e.g., Missing Checkout), `ManualEntry`.
    * **Logic:** `DailyLog` acts as the Aggregate Root.

4.  **`ProgressPayment.cs` (KKTC Hakediş System):**
    * **New Fields (decimal):**
        * `RetentionAmount`: "Stopaj" (Usually 3% - 10%).
        * `SecurityDeposit`: "Teminat Kesintisi".
        * `AdvanceRepayment`: "Avans Mahsubu".
        * `GreenBookAmount`: Total value derived from measured works.
    * **Logic:** Ensure cumulative calculations: `PreviousTotal + CurrentPeriod = CumulativeTotal`.

5.  **`ProjectTransaction.cs` (The Financial Ledger):**
    * **Multi-Currency Core:**
        * `TransactionAmount` (The value on the invoice/receipt).
        * `TransactionCurrencyId`.
        * `ExchangeRate` (The rate at the specific `TransactionDate`).
        * `ProjectCurrencyAmount` (Calculated: Transaction / Rate).
        * `LocalCurrencyAmount` (Calculated for Accounting).
    * **SourceType Enum:** `StockUsage`, `DirectPurchase`, `LaborCost`, `SubcontractorBill`, `ExpenseClaim`.

---

## PHASE 3: APPLICATION LAYER (BUSINESS LOGIC)

**Goal:** Implement the "Cost Engines" that automate financial tracking.

1.  **Features/DailyLogs/Commands/ApproveDailyLog:**
    * **Logic:** When approved:
        1.  Fetch `ResourceRateCard` for each worker/machine.
        2.  Fetch active `ExchangeRate` for the log date.
        3.  Calculate `Cost = ApprovedHours * HourlyRate`.
        4.  Create `ProjectTransaction` entries (Source: `LaborCost`).
    * **Event:** Publish `DailyLogApprovedEvent`.

2.  **Infrastructure/Consumers/StockConsumedConsumer.cs (Refactor):**
    * **Current:** Likely only reduces stock.
    * **Required:**
        1.  Get the `AverageCost` of the product from Inventory.
        2.  Create a `ProjectTransaction` (Source: `StockUsage`) attached to the specific Project Task.
        3.  Handle currency conversion if Stock is in TL but Project is in GBP.

3.  **Features/ProgressPayments/Queries/GetPaymentDraft:**
    * Implement logic to aggregate all approved "Green Book" (Production) quantities up to date, apply unit prices, deduct previous payments, calculate retention, and return a preview DTO.

---

## PHASE 4: FRONTEND REFACTORING (REACT + VITE)

**Goal:** Move from "Data Entry" to "Process Management" UI.

1.  **`DailyLogTab.tsx` (Complete Rewrite):**
    * **Component:** Use a DataGrid (e.g., TanStack Table).
    * **Features:**
        * **Batch Editing:** Allow entering hours for 20 workers in a grid view.
        * **Validation Visuals:** If `RawHours` (HR) != `ApprovedHours`, highlight the cell in Red/Yellow.
        * **Actions:** "Copy from Yesterday", "Bulk Approve".

2.  **`PaymentsTab.tsx` (KKTC Workflow):**
    * **UI Structure:** Create a Wizard or Tabbed Modal:
        * *Tab 1: Green Book (Yeşil Defter):* Input measured quantities (m2, m3).
        * *Tab 2: Financials:* Auto-calculated amounts. Input fields for "Stopaj %", "Teminat %".
        * *Tab 3: Summary:* Final Payable Amount.

3.  **`ProjectDetailPage.tsx`:**
    * Add a **Financial Widget**: Show "Budget vs. Actual Cost" handling multi-currency display (e.g., a toggle to switch view between GBP and TL).

4.  **Localization (`tr.json`):**
    * Add these specific keys:
        ```json
        "project_management": {
          "green_book": "Yeşil Defter",
          "retention": "Stopaj Kesintisi",
          "security_deposit": "Teminat Kesintisi",
          "advance_repayment": "Avans Mahsubu",
          "materials_on_site": "İhzarat",
          "boq": "Keşif Metrajı",
          "labor_cost": "İşçilik Maliyeti",
          "anomaly_detected": "Puantaj Uyuşmazlığı"
        }
        ```

---

**EXECUTION INSTRUCTIONS:**
* Start with **Backend Domain & Persistence** (EF Configurations) to ensure the DB structure is solid.
* Move to **Application Layer** (Command/Queries & Cost Engines).
* Refactor **Controllers**.
* Finish with **Frontend** components.
* **Provide full file contents** for modified files.