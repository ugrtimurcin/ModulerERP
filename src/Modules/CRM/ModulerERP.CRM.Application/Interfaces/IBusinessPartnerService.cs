using ModulerERP.CRM.Application.DTOs;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.CRM.Application.Interfaces;

/// <summary>
/// Service interface for BusinessPartner CRUD operations.
/// </summary>
public interface IBusinessPartnerService
{
    Task<PagedResult<BusinessPartnerListDto>> GetPartnersAsync(Guid tenantId, int page, int pageSize, bool? isCustomer = null, bool? isSupplier = null);
    Task<BusinessPartnerDetailDto?> GetPartnerByIdAsync(Guid tenantId, Guid id);
    Task<BusinessPartnerDetailDto> CreatePartnerAsync(Guid tenantId, CreateBusinessPartnerDto dto, Guid userId);
    Task<BusinessPartnerDetailDto> UpdatePartnerAsync(Guid tenantId, Guid id, UpdateBusinessPartnerDto dto);
    Task DeletePartnerAsync(Guid tenantId, Guid id, Guid userId);
}
