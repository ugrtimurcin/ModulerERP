using ModulerERP.Inventory.Domain.Enums;

namespace ModulerERP.Inventory.Application.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProductType Type { get; set; }
    
    public Guid UnitOfMeasureId { get; set; }
    public string? UnitOfMeasureCode { get; set; }
    public string? UnitOfMeasureName { get; set; }
    
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    
    // Base pricing
    public decimal PurchasePrice { get; set; }
    public decimal SalesPrice { get; set; }
    public decimal CostPrice { get; set; }
    
    // Inventory settings
    public decimal MinStockLevel { get; set; }
    public decimal ReorderLevel { get; set; }
    public bool TrackInventory { get; set; }
    public bool IsSellable { get; set; }
    public bool IsPurchasable { get; set; }
    public bool HasVariants { get; set; }
    
    // Additional Info
    public string? HsCode { get; set; }
    public string? OriginCountry { get; set; }
    public decimal? Weight { get; set; }
    
    // Navigation collections
    public List<ProductBarcodeDto> Barcodes { get; set; } = new();
    public List<ProductPriceDto> Prices { get; set; } = new();
    public List<StockLevelDto> StockLevels { get; set; } = new();
}

public class ProductBarcodeDto
{
    public Guid Id { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string BarcodeType { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public class ProductPriceDto
{
    public Guid Id { get; set; }
    public Guid CurrencyId { get; set; }
    public string PriceType { get; set; } = string.Empty; // Sales, Purchase
    public decimal Price { get; set; }
    public decimal MinQuantity { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}



public class CreateProductDto
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProductType Type { get; set; } = ProductType.Inventory;
    public Guid UnitOfMeasureId { get; set; }
    public Guid? CategoryId { get; set; }
    
    public decimal PurchasePrice { get; set; }
    public decimal SalesPrice { get; set; }
    
    public decimal MinStockLevel { get; set; }
    public decimal ReorderLevel { get; set; }
    
    public bool IsSellable { get; set; } = true;
    public bool IsPurchasable { get; set; } = true;
    public bool TrackInventory { get; set; } = true;
    
    public string? HsCode { get; set; }
    public string? OriginCountry { get; set; }
    public decimal? Weight { get; set; }
    
    // Optional initial collections
    public List<CreateProductBarcodeDto> Barcodes { get; set; } = new();
}

public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    
    public decimal PurchasePrice { get; set; }
    public decimal SalesPrice { get; set; }
    
    public decimal MinStockLevel { get; set; }
    public decimal ReorderLevel { get; set; }
    
    public bool IsSellable { get; set; }
    public bool IsPurchasable { get; set; }
    public bool TrackInventory { get; set; }
    
    public string? HsCode { get; set; }
    public string? OriginCountry { get; set; }
    public decimal? Weight { get; set; }
}

public class CreateProductBarcodeDto
{
    public string Barcode { get; set; } = string.Empty;
    public string BarcodeType { get; set; } = "EAN13";
    public bool IsPrimary { get; set; }
}

public class CreateProductPriceDto
{
    public Guid CurrencyId { get; set; }
    public string PriceType { get; set; } = "Sales";
    public decimal Price { get; set; }
    public decimal MinQuantity { get; set; } = 1;
}
