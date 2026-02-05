using ModulerERP.SharedKernel.Entities;
using ModulerERP.Inventory.Domain.Enums;

namespace ModulerERP.Inventory.Domain.Entities;

/// <summary>
/// Unit of Measure with conversion support.
/// </summary>
public class UnitOfMeasure : BaseEntity
{
    /// <summary>Short code (e.g., 'PCS', 'KG', 'M')</summary>
    public string Code { get; private set; } = string.Empty;
    
    public string Name { get; private set; } = string.Empty;
    
    public UomType Type { get; private set; }
    
    /// <summary>Conversion factor to base unit</summary>
    public decimal ConversionFactor { get; private set; } = 1;
    
    /// <summary>Reference to base unit (null if this is base)</summary>
    public Guid? BaseUnitId { get; private set; }

    // Navigation
    public UnitOfMeasure? BaseUnit { get; private set; }

    private UnitOfMeasure() { } // EF Core

    public static UnitOfMeasure Create(
        Guid tenantId,
        string code,
        string name,
        UomType type,
        Guid createdByUserId,
        decimal conversionFactor = 1,
        Guid? baseUnitId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        var uom = new UnitOfMeasure
        {
            Code = code.ToUpperInvariant(),
            Name = name,
            Type = type,
            ConversionFactor = conversionFactor,
            BaseUnitId = baseUnitId
        };

        uom.SetTenant(tenantId);
        uom.SetCreator(createdByUserId);
        return uom;
    }

    public void Update(string code, string name, UomType type, decimal conversionFactor)
    {
        Code = code.ToUpperInvariant();
        Name = name;
        Type = type;
        ConversionFactor = conversionFactor;
    }

    public decimal ConvertToBase(decimal quantity) => quantity * ConversionFactor;
    public decimal ConvertFromBase(decimal baseQuantity) => baseQuantity / ConversionFactor;
}
