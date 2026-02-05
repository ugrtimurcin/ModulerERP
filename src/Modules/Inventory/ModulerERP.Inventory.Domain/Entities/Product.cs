using ModulerERP.SharedKernel.Entities;
using ModulerERP.Inventory.Domain.Enums;

namespace ModulerERP.Inventory.Domain.Entities;

/// <summary>
/// Core product/item entity.
/// Uses Template Pattern for variants.
/// </summary>
public class Product : BaseEntity
{
    /// <summary>Unique ERP Stock Code (e.g., 'PRD-001')</summary>
    public string Sku { get; private set; } = string.Empty;
    
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    
    public ProductType Type { get; private set; } = ProductType.Inventory;
    
    public Guid? CategoryId { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    
    /// <summary>Default purchase UOM if different from base</summary>
    public Guid? PurchaseUomId { get; private set; }
    
    /// <summary>Default sales UOM if different from base</summary>
    public Guid? SalesUomId { get; private set; }
    
    /// <summary>Default buy price in base currency</summary>
    public decimal PurchasePrice { get; private set; }
    
    /// <summary>Default sell price in base currency</summary>
    public decimal SalesPrice { get; private set; }
    
    /// <summary>Current cost per unit (updated by costing method)</summary>
    public decimal CostPrice { get; private set; }
    
    /// <summary>For FIFO/Standard costing</summary>
    public CostingMethod CostingMethod { get; private set; } = CostingMethod.WeightedAverage;
    
    /// <summary>Trigger for low stock alerts</summary>
    public decimal MinStockLevel { get; private set; }
    
    /// <summary>Reorder target</summary>
    public decimal ReorderLevel { get; private set; }
    
    /// <summary>This product has variants (e.g., Size, Color)</summary>
    public bool HasVariants { get; private set; }
    
    /// <summary>Template product for variants</summary>
    public Guid? ParentProductId { get; private set; }
    
    /// <summary>Weight in kg (for shipping)</summary>
    public decimal? Weight { get; private set; }
    
    /// <summary>Dimensions in JSON format</summary>
    public string? Dimensions { get; private set; }
    
    /// <summary>HSN/HS Code for customs</summary>
    public string? HsCode { get; private set; }
    
    /// <summary>Country of manufacture</summary>
    public string? OriginCountry { get; private set; }
    
    /// <summary>Brand/Manufacturer</summary>
    public Guid? BrandId { get; private set; }
    
    /// <summary>Can be sold?</summary>
    public bool IsSellable { get; private set; } = true;
    
    /// <summary>Can be purchased?</summary>
    public bool IsPurchasable { get; private set; } = true;
    
    /// <summary>Track stock levels?</summary>
    public bool TrackInventory { get; private set; } = true;
    
    /// <summary>Track individual serial numbers?</summary>
    public bool TrackSerials { get; private set; }
    
    /// <summary>Track lot/batch numbers?</summary>
    public bool TrackBatches { get; private set; }

    // Navigation
    public ProductCategory? Category { get; private set; }
    public UnitOfMeasure? UnitOfMeasure { get; private set; }
    public Brand? Brand { get; private set; }
    public Product? ParentProduct { get; private set; }
    public ICollection<Product> Variants { get; private set; } = new List<Product>(); // Legacy self-referencing
    public ICollection<ProductVariant> ProductVariants { get; private set; } = new List<ProductVariant>(); // New separate table
    public ICollection<ProductBarcode> Barcodes { get; private set; } = new List<ProductBarcode>();
    public ICollection<ProductPrice> Prices { get; private set; } = new List<ProductPrice>();
    public ICollection<StockLevel> StockLevels { get; private set; } = new List<StockLevel>();
    public ICollection<ProductSerial> Serials { get; private set; } = new List<ProductSerial>();
    public ICollection<ProductBatch> Batches { get; private set; } = new List<ProductBatch>();

    private Product() { } // EF Core

    public static Product Create(
        Guid tenantId,
        string sku,
        string name,
        ProductType type,
        Guid unitOfMeasureId,
        Guid createdByUserId,
        Guid? categoryId = null,
        decimal purchasePrice = 0,
        decimal salesPrice = 0)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required", nameof(sku));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        var product = new Product
        {
            Sku = sku.ToUpperInvariant(),
            Name = name,
            Type = type,
            UnitOfMeasureId = unitOfMeasureId,
            CategoryId = categoryId,
            PurchasePrice = purchasePrice,
            SalesPrice = salesPrice,
            CostPrice = purchasePrice
        };

        product.SetTenant(tenantId);
        product.SetCreator(createdByUserId);
        return product;
    }

    public void UpdateBasicInfo(string name, string? description, Guid? categoryId)
    {
        Name = name;
        Description = description;
        CategoryId = categoryId;
    }

    public void UpdatePricing(decimal purchasePrice, decimal salesPrice)
    {
        PurchasePrice = purchasePrice;
        SalesPrice = salesPrice;
    }

    public void UpdateCostPrice(decimal costPrice) => CostPrice = costPrice;

    public void SetStockLevels(decimal minStockLevel, decimal reorderLevel)
    {
        MinStockLevel = minStockLevel;
        ReorderLevel = reorderLevel;
    }

    public void EnableVariants() => HasVariants = true;
    public void SetAsVariant(Guid parentProductId) => ParentProductId = parentProductId;
    
    public void SetTradeSettings(bool isSellable, bool isPurchasable, bool trackInventory)
    {
        IsSellable = isSellable;
        IsPurchasable = isPurchasable;
        TrackInventory = trackInventory;
    }
}
