using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Inventory.Domain.Entities;

/// <summary>
/// Defines conversion factors between units of measure.
/// Can be global or product-specific.
/// </summary>
public class UnitConversion : BaseEntity
{
    /// <summary>Source unit of measure</summary>
    public Guid FromUomId { get; private set; }
    
    /// <summary>Target unit of measure</summary>
    public Guid ToUomId { get; private set; }
    
    /// <summary>Multiplier: ToQuantity = FromQuantity * Factor</summary>
    public decimal ConversionFactor { get; private set; }
    
    /// <summary>If set, this conversion only applies to this product</summary>
    public Guid? ProductId { get; private set; }

    // Navigation
    public UnitOfMeasure? FromUom { get; private set; }
    public UnitOfMeasure? ToUom { get; private set; }
    public Product? Product { get; private set; }

    private UnitConversion() { }

    public static UnitConversion Create(
        Guid tenantId,
        Guid fromUomId,
        Guid toUomId,
        decimal conversionFactor,
        Guid createdByUserId,
        Guid? productId = null)
    {
        if (conversionFactor <= 0)
            throw new ArgumentException("Conversion factor must be positive", nameof(conversionFactor));
        if (fromUomId == toUomId)
            throw new ArgumentException("From and To UOM must be different");

        var conversion = new UnitConversion
        {
            FromUomId = fromUomId,
            ToUomId = toUomId,
            ConversionFactor = conversionFactor,
            ProductId = productId
        };

        conversion.SetTenant(tenantId);
        conversion.SetCreator(createdByUserId);
        return conversion;
    }

    public void UpdateFactor(decimal factor)
    {
        if (factor <= 0) throw new ArgumentException("Factor must be positive");
        ConversionFactor = factor;
    }

    public decimal Convert(decimal quantity)
    {
        return quantity * ConversionFactor;
    }
}
