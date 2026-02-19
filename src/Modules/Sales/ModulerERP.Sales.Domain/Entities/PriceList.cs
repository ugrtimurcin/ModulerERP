using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Sales.Domain.Entities;

/// <summary>
/// Price list with validity periods and currency.
/// Supports tiered/customer-specific pricing.
/// </summary>
public class PriceList : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid CurrencyId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    
    /// <summary>Priority for price resolution (higher = preferred)</summary>
    public int Priority { get; private set; }

    // Navigation
    public ICollection<PriceListItem> Items { get; private set; } = new List<PriceListItem>();

    private PriceList() { } // EF Core

    public static PriceList Create(
        Guid tenantId,
        string name,
        Guid currencyId,
        Guid createdByUserId,
        string? description = null,
        DateTime? validFrom = null,
        DateTime? validTo = null,
        int priority = 0)
    {
        var priceList = new PriceList
        {
            Name = name,
            CurrencyId = currencyId,
            Description = description,
            ValidFrom = validFrom,
            ValidTo = validTo,
            Priority = priority
        };

        priceList.SetTenant(tenantId);
        priceList.SetCreator(createdByUserId);
        return priceList;
    }

    public void Update(string name, string? description, DateTime? validFrom, DateTime? validTo, int priority)
    {
        Name = name;
        Description = description;
        ValidFrom = validFrom;
        ValidTo = validTo;
        Priority = priority;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
