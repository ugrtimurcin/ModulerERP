using ModulerERP.CRM.Application.DTOs;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.CRM.Application.Interfaces;

public interface IOpportunityService
{
    Task<PagedResult<OpportunityListDto>> GetOpportunitiesAsync(Guid tenantId, int page, int pageSize, string? stage = null, Guid? assignedUserId = null);
    Task<OpportunityDetailDto?> GetOpportunityByIdAsync(Guid tenantId, Guid id);
    Task<OpportunityDetailDto> CreateOpportunityAsync(Guid tenantId, CreateOpportunityDto dto, Guid createdByUserId);
    Task<OpportunityDetailDto> UpdateOpportunityAsync(Guid tenantId, Guid id, UpdateOpportunityDto dto);
    Task DeleteOpportunityAsync(Guid tenantId, Guid id, Guid deletedByUserId);
    Task UpdateStageAsync(Guid tenantId, Guid id, string stage);
}
