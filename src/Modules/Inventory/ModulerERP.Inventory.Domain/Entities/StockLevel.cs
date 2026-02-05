using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Inventory.Domain.Entities;

/// <summary>
/// Cached stock quantity per Product per Warehouse.
/// Updated via triggers/events from StockMovements.
/// </summary>
public class StockLevel : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid? LocationId { get; private set; }
    
    /// <summary>Current on-hand quantity</summary>
    public decimal QuantityOnHand { get; private set; }
    
    /// <summary>Quantity reserved for orders</summary>
    public decimal QuantityReserved { get; private set; }
    
    /// <summary>Quantity expected from purchase orders</summary>
    public decimal QuantityOnOrder { get; private set; }
    
    // Navigation
    public Product? Product { get; private set; }
    public Warehouse? Warehouse { get; private set; }
    public WarehouseLocation? Location { get; private set; }

    /// <summary>Available for new orders</summary>
    public decimal QuantityAvailable => QuantityOnHand - QuantityReserved;
    
    /// <summary>Projected quantity including incoming</summary>
    public decimal ProjectedQuantity => QuantityOnHand - QuantityReserved + QuantityOnOrder;

    private StockLevel() { } // EF Core

    public static StockLevel Create(
        Guid tenantId,
        Guid productId,
        Guid warehouseId,
        Guid? locationId = null)
    {
        var level = new StockLevel
        {
            ProductId = productId,
            WarehouseId = warehouseId,
            LocationId = locationId,
            QuantityOnHand = 0,
            QuantityReserved = 0,
            QuantityOnOrder = 0
        };
        level.SetTenant(tenantId);
        return level;
    }

    public void AddStock(decimal quantity)
    {
        QuantityOnHand += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveStock(decimal quantity)
    {
        if (QuantityOnHand < quantity)
            throw new InvalidOperationException("Insufficient stock");
        
        QuantityOnHand -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reserve(decimal quantity)
    {
        if (QuantityAvailable < quantity)
            throw new InvalidOperationException("Insufficient available stock for reservation");
        
        QuantityReserved += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseReservation(decimal quantity)
    {
        QuantityReserved = Math.Max(0, QuantityReserved - quantity);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateOnOrder(decimal quantity)
    {
        QuantityOnOrder = quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
