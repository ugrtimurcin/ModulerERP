namespace ModulerERP.Inventory.Application.DTOs;

// Brand DTOs
public record BrandDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string? Website,
    string? LogoUrl,
    bool IsActive);

public record CreateBrandDto(
    string Code,
    string Name,
    string? Description = null,
    string? Website = null);

public record UpdateBrandDto(
    string Name,
    string? Description,
    string? Website,
    string? LogoUrl);

// Unit Conversion DTOs
public record UnitConversionDto(
    Guid Id,
    Guid FromUomId,
    string? FromUomCode,
    Guid ToUomId,
    string? ToUomCode,
    decimal ConversionFactor,
    Guid? ProductId,
    string? ProductSku);

public record CreateUnitConversionDto(
    Guid FromUomId,
    Guid ToUomId,
    decimal ConversionFactor,
    Guid? ProductId = null);

// Product Serial DTOs
public record ProductSerialDto(
    Guid Id,
    Guid ProductId,
    string? ProductSku,
    string SerialNumber,
    int Status,
    string StatusName,
    Guid? WarehouseId,
    string? WarehouseName,
    string? SupplierSerial,
    DateTime? WarrantyExpiry);

public record CreateProductSerialDto(
    Guid ProductId,
    string SerialNumber,
    Guid? WarehouseId = null,
    string? SupplierSerial = null,
    DateTime? WarrantyExpiry = null);

// Product Batch DTOs
public record ProductBatchDto(
    Guid Id,
    Guid ProductId,
    string? ProductSku,
    string BatchNumber,
    Guid WarehouseId,
    string? WarehouseName,
    decimal Quantity,
    decimal InitialQuantity,
    DateTime? ManufactureDate,
    DateTime? ExpiryDate,
    int Status,
    string StatusName,
    int? DaysUntilExpiry);

public record CreateProductBatchDto(
    Guid ProductId,
    string BatchNumber,
    Guid WarehouseId,
    decimal Quantity,
    DateTime? ManufactureDate = null,
    DateTime? ExpiryDate = null);

// Attribute Definition DTOs
public record AttributeDefinitionDto(
    Guid Id,
    string Code,
    string Name,
    int Type,
    string TypeName,
    int SortOrder,
    IEnumerable<AttributeValueDto> Values);

public record CreateAttributeDefinitionDto(
    string Code,
    string Name,
    int Type,
    int SortOrder = 0);

// Attribute Value DTOs
public record AttributeValueDto(
    Guid Id,
    Guid AttributeDefinitionId,
    string Value,
    string? Code,
    int SortOrder);

public record CreateAttributeValueDto(
    Guid AttributeDefinitionId,
    string Value,
    string? Code = null,
    int SortOrder = 0);
