using ModulerERP.SharedKernel.Entities;
using ModulerERP.Manufacturing.Domain.Enums;

namespace ModulerERP.Manufacturing.Domain.Entities;

/// <summary>
/// Production order header.
/// </summary>
public class ProductionOrder : BaseEntity
{
    /// <summary>Production order number (e.g., 'PO-MFG-2026-001')</summary>
    public string OrderNumber { get; private set; } = string.Empty;
    
    public Guid BomId { get; private set; }
    public Guid ProductId { get; private set; }
    
    public ProductionOrderStatus Status { get; private set; } = ProductionOrderStatus.Draft;
    
    /// <summary>Quantity to produce</summary>
    public decimal PlannedQuantity { get; private set; }
    
    /// <summary>Quantity actually produced</summary>
    public decimal ProducedQuantity { get; private set; }
    
    /// <summary>Target warehouse for finished goods</summary>
    public Guid WarehouseId { get; private set; }
    
    /// <summary>Planned start date</summary>
    public DateTime? PlannedStartDate { get; private set; }
    
    /// <summary>Planned end date</summary>
    public DateTime? PlannedEndDate { get; private set; }
    
    /// <summary>Actual start date</summary>
    public DateTime? ActualStartDate { get; private set; }
    
    /// <summary>Actual end date</summary>
    public DateTime? ActualEndDate { get; private set; }
    
    /// <summary>Priority (1 = highest)</summary>
    public int Priority { get; private set; } = 5;
    
    public string? Notes { get; private set; }

    // Navigation
    public BillOfMaterials? Bom { get; private set; }
    public ICollection<ProductionOrderLine> Lines { get; private set; } = new List<ProductionOrderLine>();

    private ProductionOrder() { } // EF Core

    public static ProductionOrder Create(
        Guid tenantId,
        string orderNumber,
        Guid bomId,
        Guid productId,
        decimal plannedQuantity,
        Guid warehouseId,
        Guid createdByUserId,
        DateTime? plannedStartDate = null,
        DateTime? plannedEndDate = null,
        int priority = 5)
    {
        var order = new ProductionOrder
        {
            OrderNumber = orderNumber,
            BomId = bomId,
            ProductId = productId,
            PlannedQuantity = plannedQuantity,
            WarehouseId = warehouseId,
            PlannedStartDate = plannedStartDate,
            PlannedEndDate = plannedEndDate,
            Priority = priority
        };

        order.SetTenant(tenantId);
        order.SetCreator(createdByUserId);
        return order;
    }

    public void Plan() => Status = ProductionOrderStatus.Planned;
    public void Release() => Status = ProductionOrderStatus.Released;

    public void Start()
    {
        Status = ProductionOrderStatus.InProgress;
        ActualStartDate = DateTime.UtcNow;
    }

    public void Complete()
    {
        Status = ProductionOrderStatus.Completed;
        ActualEndDate = DateTime.UtcNow;
    }

    public void Cancel() => Status = ProductionOrderStatus.Cancelled;

    public void RecordProduction(decimal quantity) => ProducedQuantity += quantity;
}
