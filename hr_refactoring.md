# MISSION: HR MODULE ENTERPRISE REFACTORING & PM INTEGRATION

**ROLE:** You are a Senior Solution Architect and Lead Full-Stack Developer specializing in .NET 8 (Clean Architecture/DDD) and React (Vite/TypeScript).

**OBJECTIVE:** Perform a comprehensive, "Enterprise-Grade" refactoring of the **HR (Human Resources)** module in `ModulerERP`. The goal is to support **TRNC (KKTC) Labor Laws**, complex Payroll calculations, and **Real-time Integration with the Project Management Module**.

**CRITICAL RULES (NON-NEGOTIABLE):**
1.  **COMPLETE IMPLEMENTATION:** Write full, working logic. No `// TODO` comments.
2.  **MODULE ISOLATION:** Work strictly within `Modules/HR` and its corresponding API/Frontend folders.
3.  **INTEGRATION:** You MUST implement the Event Bus logic to feed attendance data into the Project Management module.
4.  **MULTI-TENANCY:** Ensure `IMultiTenant` is respected in all queries and commands.
5.  **LOCALIZATION:** Extract all UI text to `tr.json` / `en.json`.

---

## PHASE 1: API & CONTROLLER RESTRUCTURING

**Goal:** Centralize HR controllers and enforce CQRS.

1.  **Folder Structure:**
    * Move ALL HR-related controllers (Employees, Payroll, Attendance, etc.) to: `src/ModulerERP.Api/Controllers/HR/`.
    * **Namespace:** `ModulerERP.Api.Controllers.HR`.

2.  **Controller Definitions:**
    * **`EmployeesController`**: CRUD, Document Upload (Work Permit), QR Code Generation.
    * **`AttendanceController`**:
        * `POST /scan`: For device/barcode integration.
        * `POST /manual-entry`: **NEW endpoint** for Office/Accountants to enter time manually (with Audit logging).
        * `POST /approve-daily`: To finalize the day and trigger PM integration.
    * **`PayrollController`**:
        * `POST /calculate`: Triggers the complex payroll engine for a specific period.
        * `GET /slip/{id}`: Generates a PDF Payslip.

---

## PHASE 2: DOMAIN LAYER (ENTITIES & COMPLIANCE)

**Goal:** Deepen the domain to support KKTC Laws and "Proxy Entry" scenarios.

1.  **`Employee.cs` Updates:**
    * **Citizenship & Legal:** `CitizenshipType` (TRNC, Turkey, Other), `IdentityNumber`, `WorkPermitNumber`, `WorkPermitExpiryDate` (Notification trigger).
    * **Financial:** `ContractCurrencyId` (e.g., GBP), `PaymentCurrencyId` (e.g., TL), `BankName`, `Iban`.
    * **Salary:** `BaseSalary` (Money Value Object).

2.  **`DailyAttendance.cs` (Refactor):**
    * **Source Tracking:** Add `AttendanceSource` Enum (`DeviceScan`, `ManualEntry`, `MobileApp`).
    * **Hours Breakdown (KKTC Rules):**
        * `NormalHours` (08:00-17:00).
        * `Overtime1x` (Weekdays after 17:00).
        * `Overtime2x` (Sundays/Holidays).
    * **Geo-Fencing:** Add `MatchedProjectId` (Guid?) - derived from GPS coordinates if available.
    * **Audit:** `IsManualEntry` (bool), `ModifiedByUserId` (Who fixed the time?).

3.  **`Payroll.cs` (KKTC Calculation Model):**
    * **Earnings:** `GrossSalary`, `OvertimePay`, `Bonus`, `TransportationAllowance`.
    * **Deductions (KKTC Specific):**
        * `SocialSecurityEmployee` (Sigorta İşçi - usually 9%).
        * `ProvidentFundEmployee` (İhtiyat Sandığı İşçi - usually 5%).
        * `IncomeTax` (Gelir Vergisi - Progressive brackets).
    * **Employer Costs:** `SocialSecurityEmployer`, `ProvidentFundEmployer`, `UnemploymentInsurance`.
    * **Net:** `NetSalary`, `ExchangeRateUsed` (if Contract is GBP but payment is TL).

---

## PHASE 3: APPLICATION LAYER (BUSINESS LOGIC & INTEGRATION)

**Goal:** The "Brains" of the system. Handle calculations and communicate with PM module.

1.  **Features/Attendance/Commands/CreateManualAttendance:**
    * **Logic:** Allow authorized users (Office) to create/update an attendance record.
    * **Validation:** Check for conflicts.
    * **Trigger:** If finalized, publish `FQy4HArVdBbZ87AHrbfdhSXRgyE5NUbrh6GaL8enMUeh`.

2.  **Features/Attendance/Commands/ProcessDeviceScan:**
    * **Logic:** Process raw `AttendanceLog`. Calculate duration. Determine `MatchedProjectId` based on Device Location vs Project Location.
    * **Overtime Logic:** If `DayOfWeek == Sunday`, all hours = `Overtime2x`. Else, hours > 9 = `Overtime1x`.

3.  **The Integration Bridge (CRITICAL):**
    * **Event:** Define `FQy4HArVdBbZ87AHrbfdhSXRgyE5NUbrh6GaL8enMUeh` in `SharedKernel`.
        * *Payload:* `EmployeeId`, `Date`, `ProjectId`, `NormalHours`, `OvertimeHours`, `IsManual`.
    * **Handler (in Service):** When an attendance record is approved/saved, publish this event so the **Project Management** module can consume it and update the `DailyLog` costs automatically.

4.  **Features/Payroll/Commands/CalculatePayroll:**
    * **Engine:** Implement a service that takes `EmployeeId` and `Period`.
    * Fetch `Attendance` records -> Sum hours.
    * Apply KKTC Tax Brackets (Hardcode 2025 brackets for now but keep clean).
    * Handle Currency Conversion (GBP -> TL) using `ExchangeRateService`.
    * Save `Payroll` entity.

---

## PHASE 4: FRONTEND REFACTORING (REACT)

1.  **`EmployeesPage.tsx` & `EmployeeDialog.tsx`:**
    * Add tabs for "Personal Info", "Work Permit/Legal", "Financial/Salary".
    * Add validation for Identity Numbers.

2.  **`AttendancePage.tsx` (The Dashboard):**
    * **View:** Calendar or List view showing status (Present, Absent, Late, Missing Checkout).
    * **Action:** Add a "Manual Entry" button that opens a dialog to fix missing punches for office staff.
    * **Visuals:** Highlight `ManualEntry` rows in a different color (e.g., light yellow) to distinguish from Device scans.

3.  **`PayrollPage.tsx`:**
    * Create a data grid showing: Employee | Gross (GBP) | Ex. Rate | Gross (TL) | Deductions | Net Pay.
    * Add "Print Payslip" action.

4.  **Localization (`tr.json`):**
    * Add specific terms: "İhtiyat Sandığı", "Sosyal Sigorta", "Gelir Vergisi Matrahı", "Çalışma İzni", "Mesai Çarpanı".

---

**EXECUTION ORDER:**
1.  **Domain:** Update Entities & Enums.
2.  **Infra:** Database Migrations.
3.  **App:** Implement `PayrollEngine` and `AttendanceService` (with Event Publishing).
4.  **API:** Move and update Controllers.
5.  **UI:** Update React pages.

**Output:** Provide the full file content for every modified or created file.