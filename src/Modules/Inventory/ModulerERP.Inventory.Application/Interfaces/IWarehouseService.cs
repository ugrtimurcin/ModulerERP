using ModulerERP.Inventory.Application.DTOs;

namespace ModulerERP.Inventory.Application.Interfaces;

public interface IWarehouseService
{
    Task<IEnumerable<WarehouseDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<WarehouseDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateWarehouseDto dto, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task SetDefaultAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
}
