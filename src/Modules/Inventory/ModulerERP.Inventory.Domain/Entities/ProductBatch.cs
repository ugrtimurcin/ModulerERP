using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Inventory.Domain.Entities;

/// <summary>
/// Tracks lot/batch inventory with expiry dates for FEFO (First Expired First Out) management.
/// </summary>
public class ProductBatch : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string BatchNumber { get; private set; } = string.Empty;
    
    /// <summary>Current warehouse location</summary>
    public Guid WarehouseId { get; private set; }
    
    /// <summary>Current quantity in this batch</summary>
    public decimal Quantity { get; private set; }
    
    /// <summary>Original quantity when batch was created</summary>
    public decimal InitialQuantity { get; private set; }
    
    public DateTime? ManufactureDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    
    /// <summary>Supplier batch/lot number</summary>
    public string? SupplierBatchNumber { get; private set; }
    
    /// <summary>Reference to purchase where batch was received</summary>
    public Guid? PurchaseOrderLineId { get; private set; }
    
    public BatchStatus Status { get; private set; } = BatchStatus.Available;
    public string? Notes { get; private set; }

    // Navigation
    public Product? Product { get; private set; }
    public Warehouse? Warehouse { get; private set; }

    private ProductBatch() { }

    public static ProductBatch Create(
        Guid tenantId,
        Guid productId,
        string batchNumber,
        Guid warehouseId,
        decimal quantity,
        Guid createdByUserId,
        DateTime? manufactureDate = null,
        DateTime? expiryDate = null)
    {
        if (string.IsNullOrWhiteSpace(batchNumber))
            throw new ArgumentException("Batch number is required", nameof(batchNumber));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        var batch = new ProductBatch
        {
            ProductId = productId,
            BatchNumber = batchNumber.ToUpperInvariant(),
            WarehouseId = warehouseId,
            Quantity = quantity,
            InitialQuantity = quantity,
            ManufactureDate = manufactureDate,
            ExpiryDate = expiryDate,
            Status = BatchStatus.Available
        };

        batch.SetTenant(tenantId);
        batch.SetCreator(createdByUserId);
        return batch;
    }

    public void Consume(decimal qty)
    {
        if (qty > Quantity) throw new InvalidOperationException("Insufficient quantity in batch");
        Quantity -= qty;
        if (Quantity == 0) Status = BatchStatus.Depleted;
    }

    public void AddQuantity(decimal qty)
    {
        if (qty <= 0) throw new ArgumentException("Quantity must be positive");
        Quantity += qty;
        if (Status == BatchStatus.Depleted) Status = BatchStatus.Available;
    }

    public void Quarantine() => Status = BatchStatus.Quarantine;
    public void Release() => Status = BatchStatus.Available;
    public void Expire() => Status = BatchStatus.Expired;

    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
    public int? DaysUntilExpiry => ExpiryDate.HasValue ? (int)(ExpiryDate.Value - DateTime.UtcNow).TotalDays : null;
}

public enum BatchStatus
{
    Available = 0,
    Quarantine = 1,
    Expired = 2,
    Depleted = 3
}
