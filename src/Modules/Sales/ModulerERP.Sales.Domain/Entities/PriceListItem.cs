using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Sales.Domain.Entities;

/// <summary>
/// Price list item with tiered pricing support (MinQuantity).
/// </summary>
public class PriceListItem : BaseEntity
{
    public Guid PriceListId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? VariantId { get; private set; }
    public Guid UnitId { get; private set; }

    public decimal Price { get; private set; }
    
    /// <summary>Minimum quantity for this price tier</summary>
    public decimal MinQuantity { get; private set; } = 1;

    // Navigation
    public PriceList? PriceList { get; private set; }

    private PriceListItem() { } // EF Core

    public static PriceListItem Create(
        Guid tenantId,
        Guid priceListId,
        Guid productId,
        Guid unitId,
        decimal price,
        Guid createdByUserId,
        Guid? variantId = null,
        decimal minQuantity = 1)
    {
        var item = new PriceListItem
        {
            PriceListId = priceListId,
            ProductId = productId,
            UnitId = unitId,
            Price = price,
            VariantId = variantId,
            MinQuantity = minQuantity
        };

        item.SetTenant(tenantId);
        item.SetCreator(createdByUserId);
        return item;
    }

    public void UpdatePrice(decimal price, decimal minQuantity = 1)
    {
        Price = price;
        MinQuantity = minQuantity;
    }
}
