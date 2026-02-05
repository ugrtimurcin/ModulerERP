using ModulerERP.CRM.Application.DTOs;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.CRM.Application.Interfaces;

public interface ILeadService
{
    Task<PagedResult<LeadListDto>> GetLeadsAsync(Guid tenantId, int page, int pageSize, string? status = null, Guid? assignedUserId = null);
    Task<LeadDetailDto?> GetLeadByIdAsync(Guid tenantId, Guid id);
    Task<LeadDetailDto> CreateLeadAsync(Guid tenantId, CreateLeadDto dto, Guid createdByUserId);
    Task<LeadDetailDto> UpdateLeadAsync(Guid tenantId, Guid id, UpdateLeadDto dto);
    Task DeleteLeadAsync(Guid tenantId, Guid id, Guid deletedByUserId);
    Task<Guid> ConvertToPartnerAsync(Guid tenantId, Guid leadId, Guid convertedByUserId);
}
