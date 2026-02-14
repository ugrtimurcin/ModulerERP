# MISSION: INVENTORY MODULE ENTERPRISE REFACTORING

**ROLE:** You are a Senior Solution Architect and Lead Developer specialized in Supply Chain Management systems, .NET 8 (DDD/CQRS), and React.

**OBJECTIVE:** Refactor the existing **Inventory Module** to support complex Construction/Manufacturing scenarios (like multi-site transfers, unit conversions, and project-based warehousing) for the `ModulerERP` SaaS project.

**CRITICAL INTEGRATION:** This module must tightly integrate with the recently refactored **Project Management** module via Domain Events.

---

## PHASE 1: DOMAIN LAYER & ENTITIES [COMPLETED]

**Goal:** Create a robust inventory model capable of tracking stock per warehouse, handling reservations, and managing multiple units of measure.

1.  **`Warehouse.cs` Update:** [DONE]
    * Add `WarehouseType` Enum: `Central`, `ProjectSite`, `Transit`, `Quarantine`.
    * Add `Guid? ProjectId`: To link a warehouse directly to a construction project.
    * Ensure `IMultiTenant` implementation.

2.  **`StockTransaction.cs` (Ledger of Inventory):** [DONE]
    * This is the immutable history of all movements.
    * Fields: `ProductId`, `WarehouseId`, `Quantity`, `TransactionType` (In, Out, Transfer, Adjustment), `ReferenceId` (e.g., PurchaseOrderId), `UnitCost` (at the time of movement).

3.  **`Product.cs` Update:** [DONE]
    * Add `ProductType`: `StockItem`, `Service`, `ExpenseItem` (Non-stock).
    * Add `TrackingMethod`: `Quantity`, `Batch`, `Serial` (Implement Quantity first, prepare others).
    * Add `SalesPrice` and `PurchasePrice` (Money Value Objects).

4.  **`UnitConversion.cs`:** [DONE]
    * Implement logic to convert between Base Unit (e.g., Kg) and Transaction Unit (e.g., Bag/Pallet).
    * Example: 1 Bag = 50 Kg.

5.  **`InventoryReservation.cs` (New):** [DONE]
    * Entity to lock stock for a specific `ProjectTask` before it is consumed.
    * Fields: `ProductId`, `WarehouseId`, `ReservedQuantity`, `ExpiryDate`, `RelatedTaskId`.

---

## PHASE 2: APPLICATION LAYER (CQRS & LOGIC) [COMPLETED]

**Goal:** Implement the complex business rules for moving and costing stock.

1.  **Move to CQRS:** [DONE]
    * Refactor current Services into `Features/Inventory/Commands` and `Queries`.
    * **Create:** `CreateStockTransferCommand`, `AdjustStockCommand`, `ReceiveGoodsCommand`.

2.  **Logic: Project Warehouse Automation:** [DONE]
    * Create **`ProjectCreatedConsumer.cs`**:
    * **Trigger:** When `ProjectCreatedEvent` is received from PM Module.
    * **Action:** Automatically create a new `Warehouse` with name "{ProjectName} - Site Storage" and type `ProjectSite`.

3.  **Logic: Stock Transfer Engine (Two-Step Transfer):** [DONE]
    * Direct transfer is risky. Implement "Transit" logic.
    * *Step 1 (Ship):* Move stock from Source Warehouse to a "Virtual Transit Warehouse".
    * *Step 2 (Receive):* Move stock from Transit to Destination Warehouse upon user confirmation.

4.  **Logic: Unit Conversion Service:** [DONE]
    * Create a service that calculates quantity changes.
    * *Input:* 10 "Pallets". *Config:* 1 Pallet = 100 "Bricks".
    * *Output:* Increase stock by 1000 Bricks.

---

## PHASE 3: API & CONTROLLERS [COMPLETED]

**Goal:** Clean architecture exposure.

1.  **Refactor Controllers:** [DONE]
    * Move all inventory controllers to `Controllers/Inventory/`.
    * Ensure all return `Result<T>`.

2.  **Key Endpoints:** [DONE]
    * `GET /api/inventory/stock-levels?warehouseId={id}`: Get current stock.
    * `POST /api/inventory/transfer`: Initiate transfer.
    * `POST /api/inventory/transfer/receive`: Complete transfer.
    * `GET /api/inventory/products/{id}/movements`: Get transaction history (Cardex).

---

## PHASE 4: FRONTEND (REACT)

**Goal:** Provide clear visibility of stock across multiple locations.

1.  **`StockLevelsPage.tsx` (Major Upgrade):**
    * Use a DataGrid with grouping by Warehouse.
    * Show "On Hand", "Reserved", and "Available" columns.

2.  **`StockTransferWizard.tsx`:**
    * A wizard to select Source Warehouse -> Destination Warehouse -> Items.
    * Validation: Prevent transfer if Source doesn't have enough stock.

3.  **`ProductDetail.tsx`:**
    * Add a tab "Stock Locations" showing where this product exists currently.
    * Add a tab "Transaction History".

4.  **Localization:**
    * Add terms: "Ambar Transferi", "Mevcut Stok", "Rezerve Miktar", "Birim Çevrimi".

---

**EXECUTION INSTRUCTIONS:**
* **Strictly enforce Multi-Tenancy.** Use `TenantId` in all queries.
* **Do not break** existing Procurement integrations; instead, enhance them (e.g., when a Goods Receipt is created in Procurement, it must call `InventoryService` to increase stock).
* Provide complete file contents.