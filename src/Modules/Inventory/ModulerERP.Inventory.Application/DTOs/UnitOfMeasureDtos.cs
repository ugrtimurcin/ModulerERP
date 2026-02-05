using ModulerERP.Inventory.Domain.Enums;

namespace ModulerERP.Inventory.Application.DTOs;

public class UnitOfMeasureDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UomType Type { get; set; }
    public decimal ConversionFactor { get; set; }
    public Guid? BaseUnitId { get; set; }
    public string? BaseUnitName { get; set; }
    public bool IsActive { get; set; }
}

public class CreateUnitOfMeasureDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UomType Type { get; set; }
    public decimal ConversionFactor { get; set; } = 1;
    public Guid? BaseUnitId { get; set; }
}

public class UpdateUnitOfMeasureDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UomType Type { get; set; }
    public decimal ConversionFactor { get; set; }
    public bool IsActive { get; set; }
}
