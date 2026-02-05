using ModulerERP.SharedKernel.Entities;
using ModulerERP.Manufacturing.Domain.Enums;

namespace ModulerERP.Manufacturing.Domain.Entities;

/// <summary>
/// Actual production execution tracking.
/// </summary>
public class ProductionRun : BaseEntity
{
    public Guid ProductionOrderId { get; private set; }
    
    /// <summary>Run number within the order</summary>
    public int RunNumber { get; private set; } = 1;
    
    public ProductionRunStatus Status { get; private set; } = ProductionRunStatus.Pending;
    
    /// <summary>Quantity produced in this run</summary>
    public decimal QuantityProduced { get; private set; }
    
    /// <summary>Quantity that failed quality control</summary>
    public decimal QuantityScrapped { get; private set; }
    
    /// <summary>Operator user ID</summary>
    public Guid? OperatorUserId { get; private set; }
    
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    
    public string? Notes { get; private set; }

    // Navigation
    public ProductionOrder? ProductionOrder { get; private set; }
    public ICollection<ProductionRunItem> Items { get; private set; } = new List<ProductionRunItem>();

    private ProductionRun() { } // EF Core

    public static ProductionRun Create(
        Guid tenantId,
        Guid productionOrderId,
        int runNumber,
        Guid createdByUserId,
        Guid? operatorUserId = null)
    {
        var run = new ProductionRun
        {
            ProductionOrderId = productionOrderId,
            RunNumber = runNumber,
            OperatorUserId = operatorUserId
        };

        run.SetTenant(tenantId);
        run.SetCreator(createdByUserId);
        return run;
    }

    public void Start()
    {
        Status = ProductionRunStatus.InProgress;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete(decimal quantityProduced, decimal quantityScrapped = 0)
    {
        Status = ProductionRunStatus.Completed;
        QuantityProduced = quantityProduced;
        QuantityScrapped = quantityScrapped;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = ProductionRunStatus.Cancelled;
    }
}

public enum ProductionRunStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}
