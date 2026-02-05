# Module 1: System Core & Infrastructure (Enterprise V2.1)

## 1. Architectural Standards & Design Principles

### A. Base Entity Schema (Inheritance)
To ensure **Enterprise-grade data integrity** and **Soft Delete** capabilities, all entity tables (excluding logs and junction tables) must inherit the following structure. This prevents data loss and ensures standard traceability.

| Field Name | Type | Constraint | Description |
| :--- | :--- | :--- | :--- |
| `Id` | UUID | PK | Unique Identifier (GUID). |
| `TenantId` | UUID | FK | **Multi-Tenancy Key.** Isolates data per company. |
| `IsActive` | Boolean | Not Null | **Operational State.** `true` = Available for new transactions. `false` = Passive/Archived (but visible in history). |
| `IsDeleted` | Boolean | Not Null | **Existence State.** `true` = Soft Deleted (In Trash). Excluded from standard queries via Global Filter. |
| `CreatedAt` | Timestamp| Not Null | |
| `CreatedBy` | UUID | FK | User who created the record. |
| `UpdatedAt` | Timestamp| Nullable | |
| `UpdatedBy` | UUID | FK | User who last updated the record. |
| `DeletedAt` | Timestamp| Nullable | Filled only if `IsDeleted` = true. |
| `DeletedBy` | UUID | FK | User who performed the soft delete. |

### B. TRNC Market Specifics
* **Multi-Currency Core:** The system supports a dual-currency approach. While `Currencies` are global, every Tenant must define a `BaseCurrencyId` (e.g., TRY for tax reporting) while allowing transactions in GBP/EUR/USD (common in Cyprus trade).
* **Localization:** Built-in `Languages` and `Translations` tables to support TR/EN interfaces seamlessly.

---

## 2. Tenant Management (SaaS Core)

### `Tenants`
Represents the organizations using the SaaS platform.

* **Rationale:** Cleaned up to remove hardcoded text fields. `BaseCurrencyId` is strictly enforced for financial integrity.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity Fields)* | | Includes `Id`, `IsActive`, `IsDeleted`, etc. |
| `Name` | Text | Company Name. |
| `Subdomain` | Text | Unique identifier for access (e.g., `acme`.erp.com). |
| `DbSchema` | Text | For database-level isolation strategies. |
| `BaseCurrencyId` | UUID | **FK to Currencies.** The Reporting/Tax currency (e.g., TRY). |
| `SubscriptionPlan` | Text | Code (e.g., 'ENTERPRISE_PLUS'). |
| `SubscriptionExpiresAt`| Timestamp| |
| `TimeZone` | Text | e.g., 'Europe/Nicosia'. |

### `TenantSettings`
* **Rationale:** Dynamic Key-Value storage for configuration, preventing `Tenants` table pollution.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | Primary Key. |
| `TenantId` | UUID | FK to Tenants. |
| `Key` | Text | Config Key (e.g., `SMTP_HOST`, `INVOICE_PREFIX`). |
| `Value` | Text | Config Value. |
| `DataType` | Text | 'String', 'Int', 'Boolean', 'Json'. |
| `IsEncrypted` | Boolean | If true, value is encrypted at rest (e.g., passwords). |

### `TenantFeatures`
* **Rationale:** Feature flags to enable/disable specific modules per tenant without code changes.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | Primary Key. |
| `TenantId` | UUID | FK to Tenants. |
| `FeatureCode` | Text | Unique code (e.g., `MOD_MANUFACTURING`, `MOD_HR`). |
| `IsEnabled` | Boolean | Switch. |
| `ValidUntil` | Timestamp| Expiration date (for trial features). |

---

## 3. Identity & Security (Authentication)

### `Users`
* **Rationale:** Focuses strictly on Identity. `RoleId` is removed (moved to `UserRoles` for N-N). `RefreshToken` is removed (moved to `UserSessions` for multi-device).

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity Fields)* | | |
| `Email` | Text | Unique Login ID (per Tenant). |
| `PasswordHash` | Text | Bcrypt/Argon2 hash. |
| `FirstName` | Text | |
| `LastName` | Text | |
| `Phone` | Text | |
| `TwoFactorEnabled` | Boolean | Security policy flag. |
| `FailedLoginAttempts`| Integer | Brute-force protection counter. |
| `LockoutEnd` | Timestamp| Account suspension end time. |
| `LastPasswordChangeDate` | Timestamp | For mandatory rotation policies. |

### `UserSessions`
* **Rationale:** Supports simultaneous login on multiple devices (Mobile App + Web Dashboard) and remote logout.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | Primary Key. |
| `UserId` | UUID | FK to Users. |
| `RefreshToken` | Text | Secure opaque token. |
| `ExpiresAt` | Timestamp| Validity. |
| `IpAddress` | Text | Audit trail. |
| `DeviceInfo` | Text | User Agent string (e.g., 'iPhone 13 / iOS 15'). |
| `IsRevoked` | Boolean | If true, the session is killed. |
| `CreatedAt` | Timestamp| |
| `TenantId` | UUID | |

### `UserLoginHistory`
* **Rationale:** Security audit log for login attempts (Success/Fail).

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | Primary Key. |
| `UserId` | UUID | FK to Users. |
| `LoginTime` | Timestamp| |
| `IpAddress` | Text | |
| `UserAgent` | Text | |
| `IsSuccessful` | Boolean | |
| `TenantId` | UUID | |

---

## 4. Authorization (RBAC - Access Control)

### `Permissions`
* **Rationale:** Hard-coded catalog of software capabilities. Defined by developers, not users.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | Primary Key. |
| `Code` | Text | Unique System Code (e.g., `INVOICE_CREATE`). |
| `ModuleName` | Text | Functional grouping (e.g., 'Finance'). |
| `Description` | Text | Friendly name. |
| `IsScopeable` | Boolean | Can this permission be restricted by data scope? |

### `Roles`
* **Rationale:** Technical access profiles (e.g., 'Finance_Viewer'). **Not** HR Job Titles.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity Fields)* | | |
| `Name` | Text | Role Name. |
| `Description` | Text | |
| `IsSystemRole` | Boolean | Protected from deletion (e.g., 'Admin'). |
| `ParentRoleId` | UUID | **FK to Roles.** For Inheritance (Child role inherits Parent permissions). |

### `RolePermissions`
* **Rationale:** Maps technical profiles to capabilities, adding Data Scope.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | Primary Key. |
| `RoleId` | UUID | FK to Roles. |
| `PermissionId` | UUID | FK to Permissions. |
| `Scope` | Integer | **Enum:** 1=Own Data, 2=Department, 3=Branch, 4=Global. |

### `UserRoles`
* **Rationale:** Assigns one or more technical roles to a user.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | Primary Key. |
| `UserId` | UUID | FK to Users. |
| `RoleId` | UUID | FK to Roles. |
| `TenantId` | UUID | |

---

## 5. Global Master Data

### `Currencies`
* **Rationale:** Global list (ISO 4217). **TenantId removed** because currencies are universal facts.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | Primary Key. |
| `Code` | Text | 'TRY', 'USD', 'GBP'. |
| `Name` | Text | 'Turkish Lira'. |
| `Symbol` | Text | '₺'. |
| `Precision` | Integer | Decimal places (e.g., 2). |
| `IsActive` | Boolean | System-wide visibility. |

### `Languages` & `Translations`
* **Rationale:** Supports TRNC's bilingual (Turkish/English) business environment.

| Table | Fields | Description |
| :--- | :--- | :--- |
| `Languages` | `Id`, `Code` (tr-TR), `Name`, `IsRtl`, `IsActive` | Available system languages. |
| `Translations` | `Id`, `LanguageCode`, `Key`, `Value` | Dynamic UI strings. |

---

## 6. Centralized Assets & Geography

### `MediaFiles`
* **Rationale:** Centralized storage (DMS) for all file types. Replaces ad-hoc URL columns.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity Fields)* | | |
| `FileName` | Text | Original filename. |
| `StoragePath` | Text | Path (S3/Disk). |
| `Extension` | Text | .pdf, .jpg. |
| `MimeType` | Text | application/pdf. |
| `SizeInBytes` | BigInt | |
| `EntityType` | Text | **Polymorphic:** 'UserAvatar', 'InvoiceDoc'. |
| `EntityId` | UUID | **Polymorphic:** ID of the related record. |

### `Addresses`
* **Rationale:** Decoupled from CRM. Now a shared resource for Users, Warehouses, Leads, and Partners.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity Fields)* | | |
| `Name` | Text | Friendly name (e.g., 'Kyrenia Branch'). |
| `Street` | Text | |
| `City` | Text | |
| `State` | Text | |
| `Country` | Text | |
| `PostalCode` | Text | |
| `EntityType` | Integer | **Enum:** 1=Partner, 2=Warehouse, 3=User, etc. |
| `EntityId` | UUID | ID of the linked entity. |
| `IsDefault` | Boolean | Is this the primary address for the entity? |

---

## 7. Integration & Async Processing

### `QueuedJobs`
* **Rationale:** Handles heavy tasks (Emails, Reports) asynchronously to prevent UI freezing.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | Primary Key. |
| `TenantId` | UUID | |
| `JobType` | Text | e.g., 'ReportGenerator'. |
| `Payload` | Jsonb | Job parameters. |
| `Status` | Integer | **Enum:** 0=Pending, 1=Processing, 2=Completed, 3=Failed. |
| `TryCount` | Integer | |
| `NextTryAt` | Timestamp| |
| `ErrorMessage` | Text | |
| `CreatedAt` | Timestamp| |
| `CompletedAt` | Timestamp| |

### `ApiKeys` & `Webhooks`
* **Rationale:** Enterprise connectivity with external systems (E-commerce, Logistics).

| Table | Key Fields | Description |
| :--- | :--- | :--- |
| `ApiKeys` | `Name`, `KeyHash`, `Permissions` (JSON), `ExpiresAt` | Inbound API Access. |
| `Webhooks` | `Url`, `Secret`, `Events` (Array) | Outbound Event Notification. |

---

## 8. Communication Hub

### `NotificationTemplates`
* **Rationale:** Managing Email/SMS content centrally with language support.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity Fields)* | | |
| `Code` | Text | Template Code (e.g., `INVOICE_PAID`). |
| `Channel` | Integer | **Enum:** 0=InApp, 1=Email, 2=SMS. |
| `Subject` | Text | Email Subject. |
| `BodyHtml` | Text | HTML content. |
| `LanguageCode` | Text | e.g., 'tr-TR'. |

### `Notifications`
* **Rationale:** In-App alerts for users.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | Primary Key. |
| `TenantId` | UUID | |
| `UserId` | UUID | Target user. |
| `Title` | Text | |
| `Content` | Text | |
| `IsRead` | Boolean | |
| `Link` | Text | Action URL. |
| `CreatedAt` | Timestamp| |

---

## 9. System Audit

### `AuditLogs`
* **Rationale:** Deep traceability using JSONB for querying old/new values.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | Primary Key. |
| `TenantId` | UUID | |
| `UserId` | UUID | Performer. |
| `ImpersonatorUserId`| UUID | Support admin (if applicable). |
| `EntityName` | Text | Table Name (e.g., 'Products'). |
| `EntityId` | UUID | Record ID. |
| `Action` | Text | INSERT, UPDATE, SOFT_DELETE, RESTORE. |
| `OldValues` | Jsonb | Snapshot before change. |
| `NewValues` | Jsonb | Snapshot after change. |
| `AffectedColumns` | Text[] | List of changed fields. |
| `Timestamp` | Timestamp| |
| `TraceId` | UUID | Correlates grouped actions. |

###
---
###

# Module 2: CRM & Partner Management (Enterprise V2.1)

## 1. Architectural Strategy
This module is the central hub for all external commercial relationships. It is designed to handle the specific needs of the **TRNC market** (Multi-currency trading) and **Enterprise workflows** (Detailed logs, Help Desk).

**Key Design Decisions:**
* **TRNC Multi-Currency:** Partners do not have a single balance column. Instead, a dedicated `PartnerBalances` cache table tracks debt/credit in TRY, GBP, EUR, USD simultaneously.
* **Communication Detail:** `BusinessPartners` table expanded to support distinct communication channels (Mobile, Landline, Fax, WhatsApp) required by corporate entities.
* **HR Integration:** `SaleAgents` are linked to `Employees` (not Users) to enable future payroll and commission calculations.
* **Activity Audit:** Strict separation between the record creator (`CreatedBy`) and the actual performer (`PerformedByUserId`) in CRM activities.
* **Polymorphism:** Addresses are delegated to Module 1. Tags and Activities use polymorphic associations to support Leads, Partners, and Tickets.

---

## 2. Core Partner Management

### `BusinessPartners`
The central entity for Customers and Suppliers.
* **Refinement:** Removed address fields (moved to Mod-1). Removed `CurrentBalance` (moved to cache). Added granular phone fields.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | Inherits Id, TenantId, IsActive, IsDeleted, CreatedAt, etc. |
| `Code` | Text | Unique ERP Code (e.g., 'C-001'). |
| `Name` | Text | Commercial Title / Full Name. |
| `IsCustomer` | Boolean | Role Flag. |
| `IsSupplier` | Boolean | Role Flag. |
| `TaxOffice` | Text | Official Tax Office. |
| `TaxNumber` | Text | VKN (For Companies). |
| `IdentityNumber`| Text | TCKN (For Individuals). |
| `Kind` | Integer | **Enum:** 1=Company, 2=Individual. |
| `GroupId` | UUID | FK to BusinessPartnerGroups. |
| `ParentPartnerId`| UUID | **FK (Self).** For Holding/Branch hierarchy. |
| `DefaultCurrencyId`| UUID | **FK.** Default trading currency (Critical for TRNC contracts). |
| `PaymentTermDays`| Integer | Net days for due date calculation. |
| `CreditLimit` | Decimal | Max allowed debt (in DefaultCurrency). |
| `DefaultDiscountRate`| Decimal | **New.** Base discount % applied to invoice totals (Negotiated rate). |
| `Website` | Text | |
| `Email` | Text | Primary billing email. |
| `MobilePhone` | Text | **New.** GSM Number. |
| `Landline` | Text | **New.** Fixed line. |
| `Fax` | Text | **New.** |
| `WhatsappNumber`| Text | **New.** Dedicated WA line. |

### `BusinessPartnerGroups`
Categorization for reporting.
* **Refinement:** Removed `DiscountRate`. Advanced pricing logic deferred to Pricing Module.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `Name` | Text | e.g., 'Hotels', 'Construction Firms'. |
| `Description` | Text | |

### `Contacts`
Individual people working at the Business Partner's company.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `PartnerId` | UUID | FK to BusinessPartners. |
| `FirstName` | Text | |
| `LastName` | Text | |
| `Position` | Text | Job Title (e.g., 'Purchasing Manager'). |
| `Email` | Text | Personal work email. |
| `Phone` | Text | Mobile/Direct line. |
| `IsPrimary` | Boolean | Is this the main point of contact? |

---

## 3. Financial Cache (TRNC Critical)

### `PartnerBalances`
* **Rationale:** Caching layer. Calculates live balances per currency. Prevents performance bottlenecks on listing screens.
* **Logic:** Updated via triggers or background jobs whenever a `LedgerTransaction` occurs.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | Primary Key. |
| `TenantId` | UUID | |
| `PartnerId` | UUID | FK to BusinessPartners. |
| `CurrencyId` | UUID | FK to Currencies (TRY, GBP, USD). |
| `Balance` | Decimal | Positive = User owes us (Debit). Negative = We owe user (Credit). |
| `LastUpdated` | Timestamp| For cache invalidation logic. |

---

## 4. Sales Force Automation (CRM)

### `Leads`
Potential clients (Marketing stage) not yet in the accounting system.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `Title` | Text | e.g., 'Mr.', 'Mrs.', 'Dr.'. |
| `FirstName` | Text | |
| `LastName` | Text | |
| `Company` | Text | Company Name (Text only, not a relation). |
| `Source` | Text | e.g., 'Instagram', 'Referral', 'Web Form'. |
| `Status` | Integer | **Enum:** New, Contacted, Qualified, Junk. |
| `ConvertedPartnerId`| UUID | FK. If converted, link to the new Partner record. |

### `Opportunities`
Deals in the pipeline.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `Title` | Text | Deal Name (e.g., '500 Unit AC Project'). |
| `LeadId` | UUID | Optional FK (if from Lead). |
| `PartnerId` | UUID | Optional FK (if existing Partner). |
| `Stage` | Integer | **Enum:** Discovery, Proposal, Negotiation, Won, Lost. |
| `Amount` | Decimal | Estimated Deal Value. |
| `CurrencyId` | UUID | Currency of the deal. |
| `Probability` | Integer | 0-100%. |
| `ExpectedCloseDate`| Timestamp| |

### `SaleAgents`
Internal Sales Representatives (Plasiyer).
* **Refinement:** Linked to `Employees` (Mod-8) instead of Users for payroll integration.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `EmployeeId` | UUID | **FK to Employees.** (Defined in HR Module). |
| `Name` | Text | Display name cache. |
| `CommissionType` | Integer | **Enum:** 1=Percent of Sales, 2=Percent of Profit. |
| `CommissionRate` | Decimal | |

---

## 5. Support & Activities

### `Activities`
Interaction history timeline.
* **Refinement:** `PerformedByUserId` added to distinguish the actor from the record creator.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | `CreatedBy` = Who typed the data (e.g., Assistant). |
| `Subject` | Text | e.g., 'Lunch meeting'. |
| `Type` | Integer | **Enum:** Call, Email, Meeting, Note. |
| `Description` | Text | Details/Minutes of meeting. |
| `ActivityDate` | Timestamp| Actual time of interaction. |
| `EntityId` | UUID | **Polymorphic:** ID of Partner or Lead. |
| `EntityType` | Text | 'Partner', 'Lead'. |
| `PerformedByUserId`| UUID | **FK to Users.** Who actually performed the activity (e.g., Sales Rep). |

### `SupportTickets`
Post-sales issue tracking (Help Desk).

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `TicketNumber` | Text | Auto-generated (e.g., 'T-2024-001'). |
| `PartnerId` | UUID | FK to BusinessPartners. |
| `Subject` | Text | |
| `Priority` | Integer | **Enum:** Low, Medium, High, Urgent. |
| `Status` | Integer | **Enum:** Open, Pending, Resolved, Closed. |
| `AssignedUserId` | UUID | FK to Users (Support Agent). |

### `TicketMessages`
Conversation history within a ticket.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `TicketId` | UUID | FK to SupportTickets. |
| `SenderUserId` | UUID | Null if sent by Customer (External). |
| `Message` | Text | Content. |
| `IsInternal` | Boolean | If true, invisible to customer (Internal Note). |

---

## 6. Master Data

### `Tags` (Master)
Centralized tag definitions.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `Name` | Text | Tag Name. |
| `ColorCode` | Text | Hex code for UI (e.g., '#FF0000'). |
| `EntityType` | Text | Optional. Restrict tag to specific modules? |

### `EntityTags` (Mapping)
Polymorphic tagging system.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `TenantId` | UUID | |
| `TagId` | UUID | FK to Tags. |
| `EntityId` | UUID | ID of Partner, Lead, Ticket, etc. |
| `EntityType` | Text | 'Partner', 'Lead', 'Ticket'. |

###
---
###

# Module 3: Inventory & Product Management (Enterprise V2.0)

## 1. Architectural Strategy
This module manages the physical flow of goods, from definition to warehousing and costing. It is specifically engineered for **TRNC's import-heavy economy**, supporting landed cost allocation (distributing freight/customs duties to item cost) and complex warehousing.

**Key Design Decisions:**
* **WMS Lite:** Moving beyond simple warehouses to **Bin/Shelf (Location)** level tracking.
* **Separation of Concerns:** `Products` table does NOT store stock quantities or single barcodes. Stock is calculated/cached in `Stocks`, and barcodes are managed in `ProductBarcodes` (supporting multiple codes per item).
* **Costing Engine:** Supports **Landed Cost Allocation** to accurately calculate the cost of imported goods (Item Price + Freight + Customs = True Cost).
* **Service Items:** Explicit support for Service/Non-Stock items which bypass inventory logic but integrate with Sales/Purchasing.
* **Advanced Units:** `UnitSets` allow buying in "Boxes" and selling in "Pieces" with automatic conversion.

---

## 2. Product Catalog (Master Data)

### `Products`
The definition of items.
* **Refinement:** Removed `StockQuantity` (calculated field), `Barcode` (moved to table). Added `Type` for logic differentiation.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | Inherits Id, TenantId, IsActive, IsDeleted, etc. |
| `Code` | Text | Unique ERP Code (e.g., 'P-1001'). |
| `Name` | Text | Product Name. |
| `Type` | Integer | **Enum:** 1=Inventory Item, 2=Service, 3=Non-Stock (Expense), 4=Semi-Finished. |
| `UnitSetId` | UUID | FK to UnitSets. Defines the units available for this product. |
| `DefaultUnitId`| UUID | FK. The base unit for stock counting (usually the smallest). |
| `BrandId` | UUID | FK to Brands. |
| `CategoryId` | UUID | FK to Categories. |
| `TaxRate` | Decimal | VAT % (KDV). |
| `TrackingMethod`| Integer | **Enum:** 0=None, 1=Batch (Lot), 2=Serial Number. |
| `HasVariants` | Boolean | If true, stock is tracked at `ProductVariants` level. |
| `Description` | Text | HTML/Rich text for quotes. |
| `ImageId` | UUID | **FK to MediaFiles** (Mod-1). Main image. |

### `ProductVariants`
SKU level definitions (e.g., "T-Shirt - Red - XL").
* **Logic:** If a product has variants, all transactions (Sales, Stock) must reference `VariantId`.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `ProductId` | UUID | FK to Products. |
| `Code` | Text | Unique Variant Code (SKU). |
| `Name` | Text | e.g., 'Red / XL'. |
| `Attributes` | Jsonb | Key-value storage (e.g., `{"Color": "Red", "Size": "XL"}`). |
| `ImageId` | UUID | Variant specific image. |

### `ProductBarcodes`
* **Rationale:** Supports multiple barcodes (EAN, UPC, Supplier Code) for the same item/unit.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `ProductId` | UUID | FK to Products. |
| `VariantId` | UUID | Nullable. FK to ProductVariants. |
| `UnitId` | UUID | FK to Units. (Barcodes can differ by Unit, e.g., Box vs Piece). |
| `Barcode` | Text | The scannable code. |
| `Type` | Text | e.g., 'EAN-13', 'Internal', 'Supplier'. |

### `Brands` & `Categories`
Hierarchical organization.

| Table | Key Fields | Description |
| :--- | :--- | :--- |
| `Brands` | `Name`, `Website` | Manufacturer definitions. |
| `Categories` | `Name`, `ParentCategoryId` | Hierarchical tree (e.g., Electronics -> Laptops). |

### `AttributeDefinitions` & `AttributeValues`
* **Rationale:** Standardizes variant creation (prevents "Red", "red", "Kırmızı" mess).

| Table | Key Fields | Description |
| :--- | :--- | :--- |
| `AttributeDefinitions`| `Name`, `Type` | e.g., 'Color', 'Size'. |
| `AttributeValues` | `DefinitionId`, `Value` | e.g., 'Red', 'Blue', 'XL'. |

---

## 3. Unit Management

### `UnitSets`
Groups of units (e.g., "Beverage Set").

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `Name` | Text | e.g., 'Weight', 'Count', 'Volume'. |

### `UnitConversions`
Rules for converting units.
* **Logic:** Base Unit Factor is always 1. Other units are multipliers.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `UnitSetId` | UUID | FK to UnitSets. |
| `Name` | Text | e.g., 'Box', 'Piece', 'Kg'. |
| `Factor` | Decimal | Multiplier relative to base unit. (e.g., Box = 12). |
| `IsBaseUnit` | Boolean | True if this is the smallest unit (Factor=1). |

---

## 4. Advanced Warehousing (WMS)

### `Warehouses`
Physical buildings/sites.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `Code` | Text | |
| `Name` | Text | e.g., 'Famagusta Main Depot'. |
| `AddressId` | UUID | **Polymorphic link** to Module 1 Addresses. |
| `IsTransit` | Boolean | For transfers (Goods in transit). |

### `WarehouseLocations`
Bin/Shelf definitions within a warehouse.
* **Rationale:** Pinpoint accuracy ("Aisle 1, Shelf B").

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `WarehouseId` | UUID | FK to Warehouses. |
| `Code` | Text | e.g., 'A-01-02'. |
| `Description` | Text | |

### `Stocks` (Cache Table)
* **Rationale:** Real-time stock snapshot. Updated by triggers on transactions.
* **Granularity:** Detailed down to Location and Variant.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `TenantId` | UUID | |
| `WarehouseId` | UUID | FK. |
| `LocationId` | UUID | FK to WarehouseLocations (Nullable if simple warehouse). |
| `ProductId` | UUID | FK. |
| `VariantId` | UUID | FK (Nullable). |
| `QuantityOnHand`| Decimal | Physical count. |
| `QuantityReserved`| Decimal | Sold/Allocated but not shipped. |
| `QuantityOnOrder`| Decimal | Purchased but not received. |

---

## 5. Traceability

### `ProductSerials`
Individual item tracking (Unique IDs).
* **Usage:** For high-value items (Electronics, Machinery).

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `ProductId` | UUID | FK. |
| `SerialNumber` | Text | Unique Identifier (S/N, IMEI). |
| `Status` | Integer | **Enum:** InStock, Sold, Defective, Returned. |
| `CurrentLocationId`| UUID | Where is it now? |

### `ProductBatches`
Lot tracking for expiry management.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `ProductId` | UUID | FK. |
| `BatchNumber` | Text | Lot No. |
| `ManufacturingDate`| Timestamp| |
| `ExpiryDate` | Timestamp| Critical for FIFO/FEFO logic. |

---

## 6. Transactions & Costing (TRNC Critical)

### `StockTransactions` (The Ledger)
Immutable history of every stock movement.
* **Source of Truth:** All reports are derived from here.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `TenantId` | UUID | |
| `TransactionDate`| Timestamp| |
| `Type` | Integer | **Enum:** Purchase, Sale, Transfer, Adjustment, Production. |
| `ReferenceId` | UUID | Link to Invoice/Dispatch Note. |
| `ProductId` | UUID | |
| `VariantId` | UUID | |
| `WarehouseId` | UUID | |
| `LocationId` | UUID | |
| `Quantity` | Decimal | In/Out amount. |
| `UnitCost` | Decimal | Cost at the time of transaction (Moving Average). |
| `CurrencyId` | UUID | Currency of the cost. |

### `LandedCosts` (Cost Allocation)
* **Rationale:** Distributing "Freights & Customs" bills onto "Inventory Items" to calculate true cost.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `PurchaseInvoiceId`| UUID | The invoice containing items (Stock). |
| `CostInvoiceId` | UUID | The invoice containing service/tax (Freight/Customs). |
| `AllocationMethod`| Integer | **Enum:** By Value, By Weight, By Quantity. |
| `AllocatedAmount` | Decimal | How much cost was distributed? |

###
---
###

# Module 4: Sales & Distribution (Enterprise V3.0)

## 1. Architectural Strategy
This module manages the operational side of the "Quote-to-Cash" cycle. It creates a strict separation between "Commitment" (Orders) and "Financial Liability" (Invoices).

**Key Design Decisions:**
* **Audit-Proof History:**
    * **Versioning:** Quotations use a revision system (`Revision`, `ParentQuoteId`) to keep track of negotiations.
    * **Snapshots:** Addresses are copied as static JSON/Text into documents at creation time.
* **Operational Flexibility:**
    * **Partial Flows:** Supports partial shipments (Backorders) and partial cancellations at the line level.
    * **Overrides:** Sales reps can override payment terms (`PaymentTermDays`) and apply header-level discounts.
* **TRNC Specifics:**
    * **Multi-Currency:** Transaction Currency (GBP) vs Reporting Currency (TRY) calculated at the document date.
    * **Incoterms:** Standardized delivery terms (Ex-Works, CIF) critical for island trade.
* **Pricing Hierarchy:**
    1. Quantity Break (Volume Discount)
    2. Price List Item (Customer/Currency Specific)
    3. Product Base Price (Fallback)

---

## 2. Pricing Engine

### `PriceLists`
Container for pricing strategies.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | Inherits Id, TenantId, IsActive, etc. |
| `Code` | Text | e.g., 'PL-001'. |
| `Name` | Text | e.g., '2026 Wholesale GBP'. |
| `CurrencyId` | UUID | FK. The fixed currency for this list. |
| `IsInternal` | Boolean | If true, not visible in B2B portals. |

### `PriceListItems`
Product-specific price overrides.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `PriceListId` | UUID | FK to PriceLists. |
| `ProductId` | UUID | FK to Products. |
| `VariantId` | UUID | FK (Nullable). |
| `UnitId` | UUID | FK. |
| `Price` | Decimal | The override price. |

### `QuantityBreaks`
Volume discount rules (e.g., "Buy 10+, get 5% off").

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `PriceListId` | UUID | Optional FK. Global rule if null. |
| `ProductId` | UUID | FK. |
| `MinQuantity` | Decimal | e.g., 10. |
| `DiscountPercentage`| Decimal | e.g., 5%. |
| `FixedPrice` | Decimal | Optional. If set, overrides %. |

---

## 3. Sales Documents (Headers)

### `Quotations`
Offers with full revision history.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `QuoteNumber` | Text | e.g., 'Q-2026-1001'. |
| `Revision` | Integer | 0, 1, 2... (Auto-increment). |
| `ParentQuoteId` | UUID | FK to previous version. |
| `IsLatest` | Boolean | Indexing flag for the active version. |
| `PartnerId` | UUID | FK to BusinessPartners. |
| `PriceListId` | UUID | Used for pricing logic. |
| `Status` | Integer | **Enum:** Draft, Sent, Accepted, Rejected, Outdated, Converted. |
| `CurrencyId` | UUID | Transaction Currency. |
| `ExchangeRate` | Decimal | Rate at creation time. |
| `TotalAmount` | Decimal | Total in Transaction Currency. |
| `TotalAmountBase`| Decimal | Total in Reporting Currency. |
| `ValidUntil` | Timestamp| Expiration date. |
| `SnapshotAddress`| Jsonb | Full address copy at time of quote. |
| `PaymentTermDays`| Integer | Override from Partner definition. |
| `DeliveryMethodId`| UUID | FK (e.g., Cargo, Courier). |
| `Incoterm` | Text | e.g., 'EXW - Ex Works'. |

### `Orders`
Committed sales commitments.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `OrderNumber` | Text | e.g., 'ORD-2026-500'. |
| `QuoteId` | UUID | FK (Nullable). Source quote. |
| `PartnerId` | UUID | |
| `Status` | Integer | **Enum:** Pending, Confirmed, PartiallyShipped, Shipped, Invoiced, Cancelled. |
| `CurrencyId` | UUID | |
| `ExchangeRate` | Decimal | |
| `ShippingAddress`| Jsonb | **Snapshot.** |
| `BillingAddress` | Jsonb | **Snapshot.** |
| `WarehouseId` | UUID | Default source warehouse. |
| `SalesAgentId` | UUID | FK to Employees. |
| `PaymentTermDays`| Integer | **Override.** (e.g., 60 days for this specific order). |
| `TotalDiscountAmount`| Decimal | **New.** Lump sum discount applied to header. |
| `Incoterm` | Text | Delivery responsibility terms. |
| `CancellationReasonId`| UUID | FK. Filled only if fully cancelled. |
| `CancelledByUserId`| UUID | Audit log. |
| `CancelledAt` | Timestamp| Audit log. |

---

## 4. Sales Document Lines (Details)

### `QuotationItems`
Flexible line items for quotes.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `QuoteId` | UUID | FK. |
| `ProductId` | UUID | |
| `Quantity` | Decimal | |
| `UnitPrice` | Decimal | |
| `DiscountRate` | Decimal | Line specific discount %. |
| `TaxRate` | Decimal | VAT %. |
| `Description` | Text | |
| `IsOptional` | Boolean | "Optional add-on" flag. |

### `OrderItems`
Strict line items for orders with logistic tracking.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `OrderId` | UUID | FK. |
| `ProductId` | UUID | |
| `VariantId` | UUID | |
| `UnitId` | UUID | |
| `Quantity` | Decimal | Original Order Qty. |
| `QuantityShipped`| Decimal | How many sent so far? |
| `QuantityCancelled`| Decimal | How many cancelled? |
| `UnitPrice` | Decimal | |
| `TaxAmount` | Decimal | **Calculated.** (Price * Qty * TaxRate). Prevents rounding errors. |
| `LineTotal` | Decimal | Net + Tax. |

---

## 5. Logistics (Shipment)

### `Deliveries` (Shipments)
Physical goods movement. **Trigger Point:** Creates `StockTransaction` (Output) in Module 3.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `DeliveryNumber` | Text | Official Dispatch Note No. |
| `OrderId` | UUID | FK. |
| `PartnerId` | UUID | |
| `Status` | Integer | **Enum:** Draft, Picked, Shipped, Delivered. |
| `ShipDate` | Timestamp| Actual exit date. |
| `TrackingNumber` | Text | |
| `DriverName` | Text | |
| `PlateNumber` | Text | |

### `DeliveryItems`
Specifies **WHERE** the item was picked from (WMS).

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `DeliveryId` | UUID | FK. |
| `OrderItemId` | UUID | Link to Order Line. |
| `ProductId` | UUID | |
| `WarehouseId` | UUID | Source Warehouse. |
| `LocationId` | UUID | **Source Bin/Shelf.** (e.g., A-01). |
| `Quantity` | Decimal | Shipped amount. |
| `BatchId` | UUID | (If Batch tracked). |
| `SerialId` | UUID | (If Serial tracked). |

---

## 6. Returns & Master Data

### `SaleReturns` (RMA)
Customer returns. **Trigger Point:** Creates `StockTransaction` (Input) in Module 3.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `ReturnNumber` | Text | e.g., 'RMA-001'. |
| `OrderId` | UUID | FK to Original Order. |
| `SourceDeliveryId`| UUID | **New.** Which shipment is coming back? |
| `Status` | Integer | **Enum:** Requested, Approved, Received, Completed. |
| `ReturnDate` | Timestamp| |

### `SaleReturnItems`
| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `ReturnId` | UUID | FK. |
| `OrderItemId` | UUID | Reference to original line. |
| `ReasonId` | UUID | FK to ReturnReasons. |
| `Quantity` | Decimal | |
| `Action` | Integer | **Enum:** ReturnToStock, Scrap. |

### `OrderCancellationReasons` & `ReturnReasons`
Master data for reporting.
* `Code`, `Name`.

###
---
###

# Module 5: Procurement (Enterprise V2.0)

## 1. Architectural Strategy
This module manages the entire "Procure-to-Pay" operational cycle. It introduces strict internal controls (Requisitions), competitive sourcing (RFQs), and quality assurance (QC) before goods enter the available inventory.

**Key Design Decisions:**
* **Requisition Fulfillment:** Tracks how much of an internal request has been ordered to prevent over-purchasing (`QuantityOrdered` vs `QuantityRequested`).
* **Quarantine Logic:** Goods received via `GoodsReceipt` do NOT go to "Available Stock" immediately. They enter a "Quarantine" status/warehouse. Only `QualityControl` processes move them to "Available".
* **Estimated Costing:** Since vendor invoices arrive later than goods, `GoodsReceiptItems` store an `EstimatedUnitCost` (from PO) to allow immediate margin calculation for sales.
* **RFQ Structuring:** One RFQ header can contain multiple items, and multiple suppliers can bid on specific items.

---

## 2. Internal Requests (Demand)

### `PurchaseRequisitions`
Internal requests from employees (e.g., "HR needs 5 laptops").

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | Inherits Id, TenantId, IsActive... |
| `RequisitionNumber`| Text | e.g., 'PR-2026-001'. |
| `RequestedByUserId`| UUID | FK to Users (Employee). |
| `DepartmentId` | UUID | Cost center tracking. |
| `Status` | Integer | **Enum:** Pending, Approved, Rejected, Completed. |
| `RequiredDate` | Timestamp| When is it needed? |
| `Description` | Text | Justification. |

### `RequisitionItems`
| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `RequisitionId` | UUID | FK. |
| `ProductId` | UUID | |
| `QuantityRequested`| Decimal | Original demand. |
| `QuantityOrdered` | Decimal | **Progress tracking.** How many bought so far? |
| `Status` | Integer | **Enum:** Pending, Ordered, Cancelled. |

---

## 3. Sourcing (RFQ & Quotes)

### `RFQs` (Request for Quotation)
The "Tender" or "Bid" header.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `RfqNumber` | Text | e.g., 'RFQ-2026-10'. |
| `Title` | Text | e.g., 'Q1 Laptop Procurement'. |
| `DeadLine` | Timestamp| Bidding closes at... |
| `Status` | Integer | **Enum:** Open, Closed, Awarded. |

### `RFQItems`
What exactly are we asking for?

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `RfqId` | UUID | FK. |
| `ProductId` | UUID | |
| `TargetQuantity` | Decimal | Expected purchase volume. |

### `PurchaseQuotes`
Offers received from suppliers.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `RfqId` | UUID | FK. |
| `SupplierId` | UUID | FK to BusinessPartners. |
| `QuoteReference` | Text | Supplier's offer number. |
| `ValidUntil` | Timestamp| |
| `TotalAmount` | Decimal | |
| `IsSelected` | Boolean | Is this the winning bid? |

### `PurchaseQuoteItems`
Line details of the supplier's offer.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `QuoteId` | UUID | FK. |
| `RfqItemId` | UUID | Link to the specific requirement. |
| `ProductId` | UUID | |
| `Price` | Decimal | Offered price. |
| `LeadTimeDays` | Integer | Delivery speed promise. |

---

## 4. Purchasing (Orders)

### `PurchaseOrders` (PO)
Official contract sent to supplier.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `OrderNumber` | Text | e.g., 'PO-2026-900'. |
| `SupplierId` | UUID | FK. |
| `Status` | Integer | **Enum:** Draft, Sent, Confirmed, Received, Closed, Cancelled. |
| `CurrencyId` | UUID | Transaction currency. |
| `ExchangeRate` | Decimal | |
| `QuoteId` | UUID | Link to winning PurchaseQuote (if exists). |
| `ShippingAddress` | Jsonb | Snapshot. Where should they deliver? |
| `PaymentTermDays` | Integer | |

### `PurchaseOrderItems`
| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `OrderId` | UUID | FK. |
| `RequisitionItemId`| UUID | **Link back to demand.** |
| `ProductId` | UUID | |
| `Quantity` | Decimal | |
| `UnitPrice` | Decimal | |
| `QuantityReceived` | Decimal | How many arrived so far? |
| `TaxAmount` | Decimal | |

---

## 5. Inbound Logistics & QC

### `GoodsReceipts` (GRN)
Triggers stock entry into **Quarantine**.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `ReceiptNumber` | Text | e.g., 'GRN-555'. |
| `OrderId` | UUID | FK to PO. |
| `SupplierDeliveryNote`| Text | Supplier's document ID. |
| `ReceiptDate` | Timestamp| Actual arrival. |
| `WarehouseId` | UUID | **Default: Quarantine Warehouse.** |

### `GoodsReceiptItems`
| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `ReceiptId` | UUID | FK. |
| `OrderItemId` | UUID | Link to PO Line. |
| `ProductId` | UUID | |
| `Quantity` | Decimal | Counted amount. |
| `EstimatedUnitCost`| Decimal | **Critical.** Copied from PO. Used for valuation until Invoice arrives. |
| `BatchNumber` | Text | If item is batch tracked. |
| `ExpiryDate` | Timestamp| |

### `QualityControlInspections` (QC)
Moves stock from **Quarantine -> Available** or **Quarantine -> Return**.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `ReceiptItemId` | UUID | Which incoming item are we checking? |
| `InspectorId` | UUID | Employee. |
| `InspectionDate` | Timestamp| |
| `QuantityPassed` | Decimal | Moves to TargetWarehouseId (Available). |
| `QuantityRejected` | Decimal | Moves to Blocked/Return Location. |
| `RejectionReasonId`| UUID | FK. |
| `TargetWarehouseId`| UUID | Where do good items go? |
| `TargetLocationId` | UUID | Specific shelf for good items. |

---

## 6. Purchase Returns

### `PurchaseReturns`
Returning rejected/defective goods to supplier.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `ReturnNumber` | Text | |
| `SupplierId` | UUID | |
| `GoodsReceiptId` | UUID | Link to source receipt. |
| `Status` | Integer | Draft, Shipped, Completed. |

### `PurchaseReturnItems`
| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `ReturnId` | UUID | FK. |
| `ReceiptItemId` | UUID | |
| `Quantity` | Decimal | |
| `ReasonId` | UUID | |

###
---
###

# Module 6: Finance & Accounting (Enterprise V2.0)

## 1. Architectural Strategy
This module acts as the "General Ledger Lite" (Preliminary Accounting). It aggregates financial data from Sales and Procurement while handling generic expenses and treasury operations.

**Key Design Decisions:**
* **Hybrid Invoicing:** Supports both "Stock Invoices" (linked to Products/Deliveries) and "Service Invoices" (linked to Expense Categories) in a single structure.
* **TRNC Cheque Lifecycle:** Implements a strict State Machine for Promissory Notes/Cheques (Portfolio -> Endorsed -> Bank -> Paid/Bounced).
* **Automated FX Matching:** `TransactionMatches` table automatically calculates Exchange Rate Differences (Kur Farkı) when closing invoices with payments in volatile currencies.
* **Dual Currency Reporting:** Every financial transaction stores `Amount` (Transaction Currency) and `ReportingAmount` (Base Currency - TRY) for tax reporting.

---

## 2. Invoicing (AP & AR)

### `Invoices`
Central table for Sales (AR) and Purchase (AP) invoices.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | Inherits Id, TenantId, IsActive... |
| `InvoiceNumber` | Text | Official Document No. |
| `Type` | Integer | **Enum:** Sales, Purchase, PurchaseReturn, SalesReturn. |
| `PartnerId` | UUID | FK to BusinessPartners. |
| `IssueDate` | Timestamp| Document Date. |
| `DueDate` | Timestamp| Payment Deadline. |
| `CurrencyId` | UUID | Transaction Currency (e.g., GBP). |
| `ExchangeRate` | Decimal | Rate at Issue Date. |
| `TotalAmount` | Decimal | |
| `WithholdingRate`| Decimal | **TRNC Specific.** Stopaj Rate (for Rent/Services). |
| `SourceModule` | Integer | **Enum:** Sales, Purchasing, Manual. |
| `SourceDocumentId`| UUID | Link to Delivery/Order if auto-generated. |
| `IsPosted` | Boolean | If true, locked for editing (Accounting integration). |

### `InvoiceItems`
Polymorphic lines (Product vs Service).

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `InvoiceId` | UUID | FK. |
| `ProductId` | UUID | Nullable. (For Stock Items). |
| `ExpenseCategoryId`| UUID | Nullable. (For Service/Overhead Items e.g., Electricity). |
| `Quantity` | Decimal | |
| `UnitPrice` | Decimal | |
| `TaxAmount` | Decimal | |
| `LineTotal` | Decimal | |
| `Description` | Text | |

---

## 3. Treasury (Cash & Bank)

### `CashAccounts` (Safes)
Physical cash points.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `Name` | Text | e.g., 'Merkez Kasa TL'. |
| `CurrencyId` | UUID | Strict currency per safe. |
| `Balance` | Decimal | Current snapshot. |

### `BankAccounts`
Bank accounts.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `BankName` | Text | |
| `Iban` | Text | |
| `CurrencyId` | UUID | |
| `AccountType` | Integer | **Enum:** Current, POS, Credit. |

### `FinancialTransactions` (The Ledger)
A unified stream of all money movements (Cash, Bank, Transfers).

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `Date` | Timestamp| |
| `Description` | Text | |
| `Type` | Integer | **Enum:** Payment, Collection, Transfer, Expense. |
| `SourceAccountId` | UUID | (Bank/Cash ID). |
| `TargetAccountId` | UUID | (Bank/Cash ID for transfers). |
| `PartnerId` | UUID | Nullable. (If payment/collection). |
| `Amount` | Decimal | |
| `ReportingAmount`| Decimal | Converted to Base Currency (TRY). |

---

## 4. Reconciliation (Matching)

### `TransactionMatches`
The "Clearing House". Matches Payments to Invoices.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `PaymentId` | UUID | FK to FinancialTransactions. |
| `InvoiceId` | UUID | FK to Invoices. |
| `MatchedAmount` | Decimal | How much of the invoice is closed? |
| `CurrencyDiffAmount`| Decimal | **Auto-Calc.** FX Profit/Loss generated by this match. |
| `MatchDate` | Timestamp| |

---

## 5. Cheque & Promissory Management (TRNC)

### `Cheques`
Tracks the cheque as a valuable asset object.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `ChequeNumber` | Text | |
| `Type` | Integer | **Enum:** OwnCheque (Given), CustomerCheque (Received). |
| `BankName` | Text | |
| `DueDate` | Timestamp| Maturity Date (Vade). |
| `Amount` | Decimal | |
| `CurrencyId` | UUID | |
| `CurrentStatus` | Integer | **Enum:** Portfolio, Endorsed, BankCollection, Pledged, Paid, Bounced. |
| `CurrentLocationId`| UUID | Where is it physically? (Safe ID / Bank ID / Partner ID). |

### `ChequeHistory`
Audit trail of the cheque's lifecycle.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `ChequeId` | UUID | FK. |
| `TransactionDate`| Timestamp| |
| `FromStatus` | Integer | |
| `ToStatus` | Integer | |
| `Description` | Text | e.g., "Given to Supplier X". |

---

## 6. Expenses

### `ExpenseCategories`
Chart of accounts for expenses.
* `Code`, `Name` (e.g., "770.01 - Travel Expenses").

### `ExpenseClaims`
Employee pocket expenses.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `EmployeeId` | UUID | FK. |
| `Status` | Integer | **Enum:** Pending, Approved, Paid. |
| `TotalAmount` | Decimal | |
| `Description` | Text | |
| `ReceiptImageId` | UUID | FK to MediaFiles. |

###
---
###

# Module 7: Human Resources (Enterprise V2.0)

## 1. Architectural Strategy
This module manages the workforce lifecycle, from attendance tracking via QR codes to payroll calculation. It is specifically designed for TRNC's multi-currency salary environment and commission-based sales culture.

**Key Design Decisions:**
* **QR-Based Attendance:** Employees do not scan themselves. A "Supervisor" scans the Employee's QR code (`QrToken`) using the mobile app. This ensures proof of presence.
* **Simplified Payroll:** Avoids complex tax engine implementation. Instead, it focuses on "Result-Oriented" payroll (Base + Overtime + Commission - Advance = Net Pay).
* **Semi-Auto Commissions:** Commissions are calculated by the system based on sales rules but require Manager approval/override before affecting payroll.
* **Multi-Currency Salaries:** Employees can be contracted in GBP, EUR, or TRY. Payroll calculates the local currency equivalent at the time of payment.

---

## 1. Personnel & Organization

### `EmployeeDetails`
Extended profile for `Users` (Employees).

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | Inherits Id (links to User), TenantId... |
| `IdentityNumber` | Text | TRNC ID / Passport No. |
| `BirthDate` | Timestamp| |
| `JobTitle` | Text | e.g., 'Senior Sales Rep'. |
| `DepartmentId` | UUID | FK. |
| `SupervisorId` | UUID | FK to Users. Who manages this person? |
| `QrToken` | Text | **Encrypted.** Unique string generated for the employee's QR badge. |
| `Iban` | Text | For salary transfer. |
| `CurrentSalary` | Decimal | Gross/Net amount. |
| `SalaryCurrencyId`| UUID | Contract currency (e.g., GBP). |
| `WorkPermitExpDate`| Timestamp| Critical for foreign workers in TRNC. |

### `SalaryHistory`
Audit trail of salary changes.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `EmployeeId` | UUID | FK. |
| `ChangeDate` | Timestamp| |
| `OldSalary` | Decimal | |
| `NewSalary` | Decimal | |
| `NewCurrencyId` | UUID | |
| `Reason` | Text | e.g., 'Annual Promotion'. |

---

## 2. Time & Attendance (QR Flow)

### `WorkShifts`
Definition of working hours.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `Name` | Text | e.g., '08:00-17:00 Center'. |
| `StartTime` | Time | 08:00. |
| `EndTime` | Time | 17:00. |
| `BreakMinutes` | Integer | e.g., 60. |

### `AttendanceLogs` (Raw Data)
The moment a Supervisor scans an Employee.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `SupervisorId` | UUID | **Who scanned?** (The active user). |
| `EmployeeId` | UUID | **Who was scanned?** (Decoded from QR). |
| `TransactionTime` | Timestamp| Exact time of scan. |
| `Type` | Integer | **Enum:** CheckIn, CheckOut. |
| `LocationId` | UUID | FK to Addresses/Warehouses. (Where did it happen?). |
| `GpsCoordinates` | Text | Lat/Long (Optional validation). |

### `DailyAttendance` (Processed)
Summarized daily record (Nightly Job).

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `EmployeeId` | UUID | FK. |
| `Date` | Date | |
| `ShiftId` | UUID | Expected shift. |
| `CheckInTime` | Timestamp| First log. |
| `CheckOutTime` | Timestamp| Last log. |
| `TotalWorkedMins` | Integer | Actual duration. |
| `OvertimeMins` | Integer | (Actual - ShiftDuration). Used for payroll. |
| `Status` | Integer | **Enum:** Present, Absent, Late, Leave, Holiday. |

### `Leaves` & `PublicHolidays`
* `Leaves`: Employee requests (Annual, Sick). Needs Approval.
* `PublicHolidays`: System-wide non-working days.

---

## 3. Compensation & Benefits

### `CommissionRules`
Logic for calculating bonuses.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `Role` | Text | e.g., 'SalesRepresentative'. |
| `MinTargetAmount` | Decimal | Threshold (e.g., 100k Sales). |
| `Percentage` | Decimal | Commission Rate (e.g., 2%). |
| `Basis` | Integer | **Enum:** InvoicedAmount, CollectedAmount, GrossProfit. |

### `PeriodCommissions`
Monthly commission calculation.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `Period` | Text | e.g., '2026-01'. |
| `EmployeeId` | UUID | FK. |
| `CalculatedAmount`| Decimal | System auto-calc based on Rules. |
| `FinalAmount` | Decimal | **Manager Approved.** Can be edited. |
| `Status` | Integer | **Enum:** Draft, Approved (Sent to Payroll). |

### `AdvanceRequests`
Borrowing against future salary.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `EmployeeId` | UUID | FK. |
| `RequestDate` | Timestamp| |
| `Amount` | Decimal | |
| `Status` | Integer | Pending, Approved, Paid, Deducted. |

---

## 4. Payroll (Financials)

### `Payrolls`
The monthly envelope.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `Period` | Text | '2026-01'. |
| `Description` | Text | e.g., 'January Salaries'. |
| `Status` | Integer | **Enum:** Draft, Approved, Paid. |
| `TotalAmount` | Decimal | Total cash out. |
| `CurrencyId` | UUID | Payment currency. |
| `FinanceTransactionId`| UUID | Link to Module 6 (General Ledger). |

### `PayrollEntries`
Individual slip per employee.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `PayrollId` | UUID | FK. |
| `EmployeeId` | UUID | FK. |
| `BaseSalary` | Decimal | From EmployeeDetails. |
| `OvertimePay` | Decimal | (OvertimeMins * Rate). |
| `CommissionPay` | Decimal | From PeriodCommissions. |
| `AdvanceDeduction`| Decimal | From AdvanceRequests. |
| `TaxDeduction` | Decimal | Simple manual/param entry. |
| `NetPayable` | Decimal | (Base + Overtime + Commission - Deductions). |
| `ExchangeRate` | Decimal | Used if BaseSalary is GBP but Payment is TRY. |

###
---
###

# Module 8: Fixed Assets (Enterprise V3.0)

## 1. Architectural Strategy
This module manages the complete lifecycle of company assets (Vehicles, IT Equipment, Machinery). It goes beyond simple listing by integrating "Meter Tracking" for fleet management, "Incident Workflows" for liability, and "Disposal Logic" for financial reporting.

**Key Design Decisions:**
* **Meter Logging:** Supports both "Assignment-Based" metering (Start/End KM) and "Periodic" metering (Monthly logs) to keep maintenance schedules accurate.
* **Incident Liability:** Accidents trigger a workflow (`AssetIncidents`) to determine if the user is at fault and if the cost should be deducted from payroll (Module 7 Integration).
* **Disposal Accounting:** Selling or scrapping an asset calculates Profit/Loss against its current Book Value.
* **Alert System:** Tracks expiration dates for Insurance, Inspections, and Warranties.

---

## 2. Asset Definitions & Registry

### `FixedAssetCategories`
Grouping for depreciation and maintenance rules.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `Name` | Text | e.g., 'Company Vehicles', 'Laptops'. |
| `UsefulLifeYears` | Integer | e.g., 5 Years. |
| `DepreciationMethod`| Integer | **Enum:** Linear, None, DecliningBalance. |
| `RequiresMeter` | Boolean | True for Vehicles/Machines. |

### `FixedAssets`
The Central Registry.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `AssetCode` | Text | Unique Tag/Barcode (e.g., 'FA-VEH-001'). |
| `Name` | Text | e.g., 'Ford Transit 34 ABC 123'. |
| `CategoryId` | UUID | FK. |
| `SerialNumber` | Text | VIN / Serial No. |
| `PurchaseDate` | Timestamp| |
| `PurchasePrice` | Decimal | Acquisition Cost. |
| `CurrencyId` | UUID | |
| `CurrentValue` | Decimal | (Purchase Price - Accumulated Depreciation). |
| `WarrantyEndDate` | Timestamp| |
| `InsurancePolicyNo` | Text | **New.** |
| `InsuranceExpiryDate`| Timestamp| **New.** Triggers Alert. |
| `InspectionExpiryDate`| Timestamp| **New.** (Muayene). Triggers Alert. |
| `Status` | Integer | **Enum:** InStock, Assigned, Maintenance, Scrapped, Sold. |
| `CurrentLocationId` | UUID | Last known physical spot. |

---

## 3. Assignment & Usage (Zimmet)

### `AssetAssignments`
Tracks who holds the asset.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `AssetId` | UUID | FK. |
| `EmployeeId` | UUID | FK (Nullable). User responsible. |
| `LocationId` | UUID | FK (Nullable). Room responsible. |
| `AssignedDate` | Timestamp| Handover date. |
| `StartValue` | Decimal | **Meter Reading at Start** (e.g., 50,000 KM). |
| `ReturnedDate` | Timestamp| Null if active. |
| `EndValue` | Decimal | **Meter Reading at Return.** |
| `Condition` | Text | Notes (e.g., "Scratch on bumper"). |

### `AssetMeterLogs`
Periodic updates (e.g., Fuel station logs, Monthly checks).

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `AssetId` | UUID | FK. |
| `AssignmentId` | UUID | FK (Nullable). Who was driving? |
| `LogDate` | Timestamp| |
| `MeterValue` | Decimal | Current KM/Hours. |
| `Source` | Integer | **Enum:** FuelEntry, PeriodicCheck, Manual. |

---

## 4. Incidents & Maintenance

### `AssetIncidents`
Accident and Damage workflow.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `AssetId` | UUID | FK. |
| `AssignmentId` | UUID | FK. Link to active user. |
| `IncidentDate` | Timestamp| |
| `Description` | Text | |
| `Status` | Integer | **Enum:** Open, Investigating, Resolved, Closed. |
| `IsUserFault` | Boolean | Result of investigation. |
| `DeductFromSalary` | Boolean | If true, triggers HR Module. |
| `DeductionAmount` | Decimal | |

### `AssetMaintenances`
Services and Repairs.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `AssetId` | UUID | FK. |
| `IncidentId` | UUID | FK (Nullable). If linked to an accident. |
| `SupplierId` | UUID | Service provider. |
| `ServiceDate` | Timestamp| |
| `Cost` | Decimal | Expense amount (Linked to Finance Expenses). |
| `Description` | Text | |
| `NextServiceDate` | Timestamp| Reminder. |
| `NextServiceMeter` | Decimal | Reminder (e.g., "At 60,000 KM"). |

---

## 5. End of Life (Financials)

### `AssetDisposals`
Selling or Scrapping the asset.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `AssetId` | UUID | FK. |
| `DisposalDate` | Timestamp| |
| `Type` | Integer | **Enum:** Sale, Scrap, Stolen, Donation. |
| `PartnerId` | UUID | Buyer (if sold). |
| `SaleAmount` | Decimal | (0 if scrapped). |
| `BookValueAtDate` | Decimal | Asset value at that moment. |
| `ProfitLoss` | Decimal | (SaleAmount - BookValueAtDate). |

### `AssetDepreciations`
Annual value reduction.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `AssetId` | UUID | FK. |
| `FiscalYear` | Integer | e.g., 2026. |
| `DepreciationAmount`| Decimal | Expense for this year. |
| `BookValue` | Decimal | Remaining value. |

###
---
###

# Module 9: Manufacturing (Enterprise V2.0)

## 1. Architectural Strategy
This module manages the transformation of raw materials into finished goods. It is designed as a **"Standard Production"** system (Work Order based), sitting between Simple Assembly and Complex MRP II.

**Key Design Decisions:**
* **Integrated Resources:** Does not create new tables for "Machines" or "Labor". It strictly links to `FixedAssets` (Module 8) for machines and `Employees` (Module 7) for labor costs.
* **Execution-Based Costing:** Costs are not theoretical. They are calculated in real-time based on `ProductionRuns` (Actual materials used + Actual hours spent).
* **Flexible Flows:** Supports "Make-to-Stock" (Internal Plan) and "Make-to-Order" (Linked to Sales Orders).
* **Disassembly:** Supports breaking down a product into components (Reverse BOM).

---

## 2. Engineering (Bill of Materials)

### `BillOfMaterials` (BOM Header)
The recipe definition.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `ProductId` | UUID | **FK to Products.** The item being produced. |
| `Name` | Text | e.g., 'Standard Desk Rev 2'. |
| `Description` | Text | |
| `OutputQuantity` | Decimal | How many items does this recipe make? (Default: 1). |
| `IsDefault` | Boolean | Auto-select this for new orders. |
| `IsActive` | Boolean | |

### `BomComponents` (BOM Lines)
Ingredients and By-products.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `BomId` | UUID | FK. |
| `ProductId` | UUID | **FK to Products.** (Raw Material). |
| `Quantity` | Decimal | Amount required per batch. |
| `WastagePercentage`| Decimal | Planned scrap rate (e.g., 5%). |
| `Type` | Integer | **Enum:** Input (Raw Material), Output (By-Product/Scrap). |

---

## 3. Planning (Work Orders)

### `ProductionOrders`
The command to the shop floor.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| *(Base Entity)* | | |
| `OrderNumber` | Text | e.g., 'WO-2026-100'. |
| `Type` | Integer | **Enum:** Assembly, Disassembly, Repair. |
| `ProductId` | UUID | What to produce. |
| `BomId` | UUID | Which recipe to use. |
| `PlannedQuantity` | Decimal | Target output. |
| `CompletedQuantity`| Decimal | Actual output so far. |
| `Status` | Integer | **Enum:** Planned, Released, InProgress, Closed, Cancelled. |
| `TargetWarehouseId`| UUID | Where to put finished goods. |
| `SourceOrderId` | UUID | **FK to Sales.Orders** (Nullable). If Make-to-Order. |
| `StartDate` | Timestamp| Planned start. |
| `DueDate` | Timestamp| Deadline. |

---

## 4. Execution (Shop Floor)

### `ProductionRuns` (The Reality)
Logs actual production activity. Handles partial production.

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `ProductionOrderId`| UUID | FK. |
| `RunNumber` | Text | e.g., 'WO-1001/1'. |
| `Date` | Timestamp| Execution time. |
| `OutputQuantity` | Decimal | How many items produced in this run? |
| `MachineId` | UUID | **FK to FixedAssets (Nullable).** Links to Module 8. |
| `MachineHours` | Decimal | Used to update Asset Meter & calculate cost. |
| `LaborHours` | Decimal | Used to calculate overhead cost. |
| `CalculatedUnitCost`| Decimal | **Critical.** (Material + Labor + Machine) / Output. |
| `Status` | Integer | **Enum:** Draft, Posted (Inv. Updated). |

### `ProductionRunItems`
Actual material consumption for this specific run.
* *Pre-filled from BOM but editable (e.g., used more glue than expected).*

| Field Name | Type | Description |
| :--- | :--- | :--- |
| `Id` | UUID | PK. |
| `RunId` | UUID | FK. |
| `ProductId` | UUID | Raw Material. |
| `Quantity` | Decimal | **Actual** consumed quantity. |
| `WarehouseId` | UUID | Source warehouse for raw materials. |

---

## 5. Integration Updates (Required Changes)

### Updates to `Products` Table (Module 3)
* Add `SupplyMethod` (Integer Enum): **Purchase, Produce.**
    * *Logic:* If 'Produce', Purchase Orders should warn/block, Production Orders are allowed.

### Updates to `StockTransactions` Table (Module 3)
* Add `TransactionType` Enum Values:
    * **ProductionInput:** Consuming raw material (Decrease Stock).
    * **ProductionOutput:** Finished good entering stock (Increase Stock).
    * **ProductionByProduct:** Scrap/By-product entering stock (Increase Stock).

### Updates to `FixedAssets` Table (Module 8)
* Ensure `CategoryId` can represent "Machinery".
* `AssetAssignments` logic can be used to assign a Machine to a "Production Department" (Location).
