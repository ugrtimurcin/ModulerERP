namespace ModulerERP.Manufacturing.Domain.Entities;

/// <summary>
/// Production order component consumption/issuance.
/// </summary>
public class ProductionOrderLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ProductionOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    
    /// <summary>Required quantity</summary>
    public decimal RequiredQuantity { get; private set; }
    
    /// <summary>Quantity issued from stock</summary>
    public decimal IssuedQuantity { get; private set; }
    
    /// <summary>Quantity consumed in production</summary>
    public decimal ConsumedQuantity { get; private set; }
    
    public Guid UnitOfMeasureId { get; private set; }
    
    /// <summary>Source warehouse</summary>
    public Guid? WarehouseId { get; private set; }

    // Navigation
    public ProductionOrder? ProductionOrder { get; private set; }

    public decimal RemainingToIssue => RequiredQuantity - IssuedQuantity;

    private ProductionOrderLine() { } // EF Core

    public static ProductionOrderLine Create(
        Guid productionOrderId,
        Guid productId,
        decimal requiredQuantity,
        Guid unitOfMeasureId,
        Guid? warehouseId = null)
    {
        return new ProductionOrderLine
        {
            ProductionOrderId = productionOrderId,
            ProductId = productId,
            RequiredQuantity = requiredQuantity,
            UnitOfMeasureId = unitOfMeasureId,
            WarehouseId = warehouseId
        };
    }

    public void Issue(decimal quantity) => IssuedQuantity += quantity;
    public void Consume(decimal quantity) => ConsumedQuantity += quantity;
}
