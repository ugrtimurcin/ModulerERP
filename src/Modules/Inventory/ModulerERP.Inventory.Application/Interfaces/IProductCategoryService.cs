using ModulerERP.Inventory.Application.DTOs;

namespace ModulerERP.Inventory.Application.Interfaces;

public interface IProductCategoryService
{
    Task<IEnumerable<ProductCategoryDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<ProductCategoryDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task<ProductCategoryDto> CreateAsync(CreateProductCategoryDto dto, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateProductCategoryDto dto, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}
