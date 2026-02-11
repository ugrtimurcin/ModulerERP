using ModulerERP.SharedKernel.DTOs;
using ModulerERP.SystemCore.Application.DTOs;

namespace ModulerERP.SystemCore.Application.Interfaces;

public interface ITenantService
{
    Task<PagedResult<TenantListDto>> GetTenantsAsync(int page, int pageSize);
    Task<TenantDto?> GetTenantByIdAsync(Guid tenantId);
    Task<TenantDto> CreateTenantAsync(CreateTenantDto dto);
    Task<TenantDto> UpdateTenantAsync(Guid tenantId, UpdateTenantDto dto);
    // Task DeleteTenantAsync(Guid tenantId); // Soft delete? Hard delete? For now maybe just deactivate via Update
}
