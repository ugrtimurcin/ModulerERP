using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Manufacturing.Domain.Entities;

/// <summary>
/// Actual material consumption in a production run.
/// </summary>
public class ProductionRunItem : BaseEntity
{
    public Guid ProductionRunId { get; private set; }
    public Guid ProductId { get; private set; }
    
    /// <summary>Quantity required per BOM</summary>
    public decimal RequiredQuantity { get; private set; }
    
    /// <summary>Quantity actually consumed</summary>
    public decimal ConsumedQuantity { get; private set; }
    
    /// <summary>Source warehouse for material</summary>
    public Guid WarehouseId { get; private set; }

    // Navigation
    public ProductionRun? ProductionRun { get; private set; }

    private ProductionRunItem() { } // EF Core

    public static ProductionRunItem Create(
        Guid tenantId,
        Guid productionRunId,
        Guid productId,
        decimal requiredQuantity,
        Guid warehouseId,
        Guid createdByUserId)
    {
        var item = new ProductionRunItem
        {
            ProductionRunId = productionRunId,
            ProductId = productId,
            RequiredQuantity = requiredQuantity,
            WarehouseId = warehouseId
        };

        item.SetTenant(tenantId);
        item.SetCreator(createdByUserId);
        return item;
    }

    public void RecordConsumption(decimal quantity)
    {
        ConsumedQuantity = quantity;
    }
}
