using ModulerERP.SharedKernel.Entities;
using ModulerERP.Manufacturing.Domain.Enums;

namespace ModulerERP.Manufacturing.Domain.Entities;

/// <summary>
/// Bill of Materials header.
/// </summary>
public class BillOfMaterials : BaseEntity
{
    /// <summary>BOM code (e.g., 'BOM-PRD-001')</summary>
    public string Code { get; private set; } = string.Empty;
    
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>The product this BOM produces</summary>
    public Guid ProductId { get; private set; }
    
    public BomType Type { get; private set; } = BomType.Standard;
    
    /// <summary>Quantity produced per run</summary>
    public decimal Quantity { get; private set; } = 1;
    
    /// <summary>Is this the default BOM for the product?</summary>
    public bool IsDefault { get; private set; }
    
    public DateTime? EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }
    
    public string? Notes { get; private set; }

    // Navigation
    public ICollection<BomComponent> Components { get; private set; } = new List<BomComponent>();

    private BillOfMaterials() { } // EF Core

    public static BillOfMaterials Create(
        Guid tenantId,
        string code,
        string name,
        Guid productId,
        decimal quantity,
        Guid createdByUserId,
        BomType type = BomType.Standard,
        bool isDefault = true)
    {
        var bom = new BillOfMaterials
        {
            Code = code,
            Name = name,
            ProductId = productId,
            Quantity = quantity,
            Type = type,
            IsDefault = isDefault
        };

        bom.SetTenant(tenantId);
        bom.SetCreator(createdByUserId);
        return bom;
    }

    public void SetValidity(DateTime? from, DateTime? to)
    {
        EffectiveFrom = from;
        EffectiveTo = to;
    }

    public bool IsValidAt(DateTime date)
    {
        if (EffectiveFrom.HasValue && date < EffectiveFrom.Value) return false;
        if (EffectiveTo.HasValue && date > EffectiveTo.Value) return false;
        return true;
    }
}
