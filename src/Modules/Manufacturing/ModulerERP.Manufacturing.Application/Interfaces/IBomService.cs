using ModulerERP.Manufacturing.Application.DTOs;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.Manufacturing.Application.Interfaces;

public interface IBomService
{
    Task<PagedResult<BomListDto>> GetBomsAsync(Guid tenantId, int page, int pageSize, string? search = null, CancellationToken ct = default);
    Task<BomDetailDto?> GetBomByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<IEnumerable<BomListDto>> GetBomsByProductAsync(Guid tenantId, Guid productId, CancellationToken ct = default);
    Task<BomDetailDto> CreateBomAsync(Guid tenantId, CreateBomDto dto, Guid userId, CancellationToken ct = default);
    Task<BomDetailDto> UpdateBomAsync(Guid tenantId, Guid id, UpdateBomDto dto, CancellationToken ct = default);
    Task DeleteBomAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct = default);
    
    // Components
    Task<BomComponentDto> AddComponentAsync(Guid tenantId, Guid bomId, CreateBomComponentDto dto, Guid userId, CancellationToken ct = default);
    Task RemoveComponentAsync(Guid tenantId, Guid componentId, CancellationToken ct = default);
}
