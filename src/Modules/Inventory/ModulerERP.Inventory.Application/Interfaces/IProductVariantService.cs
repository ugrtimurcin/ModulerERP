
using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.SharedKernel.Results;

namespace ModulerERP.Inventory.Application.Interfaces;

public interface IProductVariantService
{
    Task<Result<IEnumerable<ProductVariantDto>>> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
    Task<Result<ProductVariantDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<ProductVariantDto>> CreateAsync(CreateProductVariantDto dto, Guid tenantId, Guid userId, CancellationToken ct = default);
    Task<Result<ProductVariantDto>> UpdateAsync(Guid id, UpdateProductVariantDto dto, Guid userId, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
}
