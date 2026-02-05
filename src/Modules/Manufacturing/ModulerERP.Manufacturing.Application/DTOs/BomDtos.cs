namespace ModulerERP.Manufacturing.Application.DTOs;

// Bill of Materials DTOs
public record BomListDto(
    Guid Id,
    string Code,
    string Name,
    Guid ProductId,
    string? ProductName,
    decimal Quantity,
    int Type,
    string TypeName,
    bool IsDefault,
    int ComponentCount,
    DateTime CreatedAt);

public record BomDetailDto(
    Guid Id,
    string Code,
    string Name,
    Guid ProductId,
    string? ProductName,
    decimal Quantity,
    int Type,
    string TypeName,
    bool IsDefault,
    DateTime? EffectiveFrom,
    DateTime? EffectiveTo,
    string? Notes,
    IEnumerable<BomComponentDto> Components,
    DateTime CreatedAt);

public record BomComponentDto(
    Guid Id,
    Guid ProductId,
    string? ProductName,
    decimal Quantity,
    string? UnitOfMeasure,
    int Sequence);

public record CreateBomDto(
    string Code,
    string Name,
    Guid ProductId,
    decimal Quantity,
    int Type = 0,
    bool IsDefault = true,
    DateTime? EffectiveFrom = null,
    DateTime? EffectiveTo = null,
    string? Notes = null);

public record UpdateBomDto(
    string Name,
    decimal Quantity,
    int Type,
    bool IsDefault,
    DateTime? EffectiveFrom,
    DateTime? EffectiveTo,
    string? Notes);

public record CreateBomComponentDto(
    Guid ProductId,
    decimal Quantity,
    int Sequence = 0);
