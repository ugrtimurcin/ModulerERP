using ModulerERP.Inventory.Application.DTOs;

namespace ModulerERP.Inventory.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductDto dto, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateProductDto dto, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    // Barcode Management
    Task AddBarcodeAsync(Guid productId, CreateProductBarcodeDto dto, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task RemoveBarcodeAsync(Guid productId, Guid barcodeId, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task SetPrimaryBarcodeAsync(Guid productId, Guid barcodeId, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    // Price Management
    Task AddPriceAsync(Guid productId, CreateProductPriceDto dto, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task RemovePriceAsync(Guid productId, Guid priceId, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}
