using ModulerERP.Inventory.Application.DTOs;

namespace ModulerERP.Inventory.Application.Interfaces;

public interface IBrandService
{
    Task<IEnumerable<BrandDto>> GetAllAsync(Guid tenantId, CancellationToken ct = default);
    Task<BrandDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<BrandDto> CreateAsync(Guid tenantId, CreateBrandDto dto, Guid userId, CancellationToken ct = default);
    Task<BrandDto?> UpdateAsync(Guid tenantId, Guid id, UpdateBrandDto dto, Guid userId, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct = default);
}

public interface IUnitConversionService
{
    Task<IEnumerable<UnitConversionDto>> GetAllAsync(Guid tenantId, Guid? productId = null, CancellationToken ct = default);
    Task<UnitConversionDto> CreateAsync(Guid tenantId, CreateUnitConversionDto dto, Guid userId, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct = default);
    Task<decimal> ConvertAsync(Guid tenantId, Guid fromUomId, Guid toUomId, decimal quantity, Guid? productId = null, CancellationToken ct = default);
}

public interface IProductSerialService
{
    Task<IEnumerable<ProductSerialDto>> GetByProductAsync(Guid tenantId, Guid productId, CancellationToken ct = default);
    Task<ProductSerialDto?> GetBySerialNumberAsync(Guid tenantId, string serialNumber, CancellationToken ct = default);
    Task<ProductSerialDto> CreateAsync(Guid tenantId, CreateProductSerialDto dto, Guid userId, CancellationToken ct = default);
    Task<ProductSerialDto?> ReserveAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<ProductSerialDto?> SellAsync(Guid tenantId, Guid id, Guid salesOrderLineId, CancellationToken ct = default);
}

public interface IProductBatchService
{
    Task<IEnumerable<ProductBatchDto>> GetByProductAsync(Guid tenantId, Guid productId, CancellationToken ct = default);
    Task<IEnumerable<ProductBatchDto>> GetExpiringAsync(Guid tenantId, int daysAhead = 30, CancellationToken ct = default);
    Task<ProductBatchDto> CreateAsync(Guid tenantId, CreateProductBatchDto dto, Guid userId, CancellationToken ct = default);
    Task<ProductBatchDto?> ConsumeAsync(Guid tenantId, Guid id, decimal quantity, CancellationToken ct = default);
    Task<ProductBatchDto?> QuarantineAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<ProductBatchDto?> ReleaseAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}

public interface IAttributeService
{
    Task<IEnumerable<AttributeDefinitionDto>> GetAllDefinitionsAsync(Guid tenantId, CancellationToken ct = default);
    Task<AttributeDefinitionDto> CreateDefinitionAsync(Guid tenantId, CreateAttributeDefinitionDto dto, Guid userId, CancellationToken ct = default);
    Task<AttributeValueDto> AddValueAsync(Guid tenantId, CreateAttributeValueDto dto, Guid userId, CancellationToken ct = default);
    Task<bool> DeleteValueAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct = default);
}
