using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.Inventory.Domain.Enums;

namespace ModulerERP.Inventory.Application.Interfaces;

public interface IUnitOfMeasureService
{
    Task<IEnumerable<UnitOfMeasureDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UnitOfMeasureDto>> GetByTypeAsync(UomType type, Guid tenantId, CancellationToken cancellationToken = default);
    Task<UnitOfMeasureDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task<UnitOfMeasureDto> CreateAsync(CreateUnitOfMeasureDto dto, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateUnitOfMeasureDto dto, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}
