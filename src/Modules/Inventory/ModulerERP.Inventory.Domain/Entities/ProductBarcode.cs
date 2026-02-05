namespace ModulerERP.Inventory.Domain.Entities;

/// <summary>
/// Multiple barcode support per product (EAN-13, EAN-128, UPC, Internal).
/// </summary>
public class ProductBarcode
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ProductId { get; private set; }
    public Guid TenantId { get; private set; }
    
    /// <summary>Barcode value</summary>
    public string Barcode { get; private set; } = string.Empty;
    
    /// <summary>Type: EAN13, EAN128, UPC, Internal</summary>
    public string BarcodeType { get; private set; } = "EAN13";
    
    /// <summary>Is this the primary barcode?</summary>
    public bool IsPrimary { get; private set; }

    // Navigation
    public Product? Product { get; private set; }

    private ProductBarcode() { } // EF Core

    public static ProductBarcode Create(Guid tenantId, Guid productId, string barcode, string barcodeType = "EAN13", bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            throw new ArgumentException("Barcode is required", nameof(barcode));

        return new ProductBarcode
        {
            TenantId = tenantId,
            ProductId = productId,
            Barcode = barcode,
            BarcodeType = barcodeType,
            IsPrimary = isPrimary
        };
    }

    public void SetAsPrimary() => IsPrimary = true;
    public void RemovePrimary() => IsPrimary = false;
}
