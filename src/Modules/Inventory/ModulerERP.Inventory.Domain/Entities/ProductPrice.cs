namespace ModulerERP.Inventory.Domain.Entities;

/// <summary>
/// Multi-currency pricing per product with validity periods.
/// TRNC Critical - supports TRY, GBP, EUR, USD simultaneously.
/// </summary>
public class ProductPrice
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ProductId { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid CurrencyId { get; private set; }
    
    /// <summary>Price type: Sales, Purchase</summary>
    public string PriceType { get; private set; } = "Sales";
    
    /// <summary>Price in the specified currency</summary>
    public decimal Price { get; private set; }
    
    /// <summary>Minimum quantity for this price tier</summary>
    public decimal MinQuantity { get; private set; } = 1;
    
    /// <summary>Validity start date</summary>
    public DateTime? ValidFrom { get; private set; }
    
    /// <summary>Validity end date</summary>
    public DateTime? ValidTo { get; private set; }

    // Navigation
    public Product? Product { get; private set; }

    private ProductPrice() { } // EF Core

    public static ProductPrice Create(
        Guid tenantId,
        Guid productId,
        Guid currencyId,
        decimal price,
        string priceType = "Sales",
        decimal minQuantity = 1,
        DateTime? validFrom = null,
        DateTime? validTo = null)
    {
        return new ProductPrice
        {
            TenantId = tenantId,
            ProductId = productId,
            CurrencyId = currencyId,
            Price = price,
            PriceType = priceType,
            MinQuantity = minQuantity,
            ValidFrom = validFrom,
            ValidTo = validTo
        };
    }

    public void UpdatePrice(decimal price) => Price = price;
    
    public void SetValidityPeriod(DateTime? from, DateTime? to)
    {
        ValidFrom = from;
        ValidTo = to;
    }

    public bool IsValidAt(DateTime date)
    {
        if (ValidFrom.HasValue && date < ValidFrom.Value) return false;
        if (ValidTo.HasValue && date > ValidTo.Value) return false;
        return true;
    }
}
