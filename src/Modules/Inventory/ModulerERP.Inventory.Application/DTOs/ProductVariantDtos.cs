
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.Inventory.Application.DTOs;

public record ProductVariantDto(
    Guid Id,
    Guid ProductId,
    string Code,
    string Name,
    string Attributes, // JSON string
    int StockLevel, // Calculated/Summary
    Guid? ImageId,
    DateTime CreatedAt
);

public record CreateProductVariantDto(
    Guid ProductId,
    string Code,
    string Name,
    string Attributes 
);

public record UpdateProductVariantDto(
    string Name,
    string Attributes
);
