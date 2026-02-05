using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Inventory.Domain.Entities;

/// <summary>
/// Tracks individual serialized items for products that require serial number tracking.
/// </summary>
public class ProductSerial : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string SerialNumber { get; private set; } = string.Empty;
    public SerialStatus Status { get; private set; } = SerialStatus.Available;
    
    /// <summary>Current warehouse location</summary>
    public Guid? WarehouseId { get; private set; }
    
    /// <summary>Reference to purchase where this serial was received</summary>
    public Guid? PurchaseOrderLineId { get; private set; }
    
    /// <summary>Reference to sale where this serial was sold</summary>
    public Guid? SalesOrderLineId { get; private set; }
    
    /// <summary>Supplier/vendor serial if different</summary>
    public string? SupplierSerial { get; private set; }
    
    /// <summary>Warranty expiry date</summary>
    public DateTime? WarrantyExpiry { get; private set; }
    
    public string? Notes { get; private set; }

    // Navigation
    public Product? Product { get; private set; }
    public Warehouse? Warehouse { get; private set; }

    private ProductSerial() { }

    public static ProductSerial Create(
        Guid tenantId,
        Guid productId,
        string serialNumber,
        Guid createdByUserId,
        Guid? warehouseId = null,
        Guid? purchaseOrderLineId = null)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            throw new ArgumentException("Serial number is required", nameof(serialNumber));

        var serial = new ProductSerial
        {
            ProductId = productId,
            SerialNumber = serialNumber.ToUpperInvariant(),
            WarehouseId = warehouseId,
            PurchaseOrderLineId = purchaseOrderLineId,
            Status = SerialStatus.Available
        };

        serial.SetTenant(tenantId);
        serial.SetCreator(createdByUserId);
        return serial;
    }

    public void Reserve() => Status = SerialStatus.Reserved;
    public void Sell(Guid salesOrderLineId)
    {
        SalesOrderLineId = salesOrderLineId;
        Status = SerialStatus.Sold;
    }
    public void ReturnToStock(Guid warehouseId)
    {
        WarehouseId = warehouseId;
        SalesOrderLineId = null;
        Status = SerialStatus.Available;
    }
    public void MarkDefective() => Status = SerialStatus.Defective;
    
    public void SetWarranty(DateTime expiryDate) => WarrantyExpiry = expiryDate;
    public void SetSupplierSerial(string supplierSerial) => SupplierSerial = supplierSerial;
}

public enum SerialStatus
{
    Available = 0,
    Reserved = 1,
    Sold = 2,
    Defective = 3,
    InTransit = 4
}
