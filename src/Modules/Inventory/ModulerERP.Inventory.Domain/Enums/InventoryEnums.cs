namespace ModulerERP.Inventory.Domain.Enums;

/// <summary>
/// Product type classification
/// </summary>
public enum ProductType
{
    StockItem = 1,
    Service = 2,
    ExpenseItem = 3, // Non-stock
    Digital = 4
}

public enum TrackingMethod
{
    Quantity = 1,
    Batch = 2,
    Serial = 3
}

/// <summary>
/// Stock movement types
/// </summary>
public enum MovementType
{
    /// <summary>Purchased from supplier</summary>
    Purchase = 1,
    /// <summary>Sold to customer</summary>
    Sale = 2,
    /// <summary>Internal transfer between warehouses</summary>
    Transfer = 3,
    /// <summary>Positive correction</summary>
    AdjustmentIn = 4,
    /// <summary>Negative correction</summary>
    AdjustmentOut = 5,
    /// <summary>Used in manufacturing</summary>
    Consumption = 6,
    /// <summary>Produced from manufacturing</summary>
    Production = 7,
    /// <summary>Customer return</summary>
    SalesReturn = 8,
    /// <summary>Supplier return</summary>
    PurchaseReturn = 9
}

/// <summary>
/// Stock transfer status
/// </summary>
public enum TransferStatus
{
    Pending = 0,
    InTransit = 1,
    Completed = 2,
    Cancelled = 3
}

/// <summary>
/// Costing method for inventory valuation
/// </summary>
public enum CostingMethod
{
    FIFO = 1,
    WeightedAverage = 2,
    StandardCost = 3
}

/// <summary>
/// Unit of Measure type
/// </summary>
public enum UomType
{
    Unit = 1,
    Weight = 2,
    Volume = 3,
    Length = 4,
    Area = 5
}
