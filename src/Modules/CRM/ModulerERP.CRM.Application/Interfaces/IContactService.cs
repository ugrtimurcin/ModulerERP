using ModulerERP.CRM.Application.DTOs;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.CRM.Application.Interfaces;

public interface IContactService
{
    Task<PagedResult<ContactListDto>> GetContactsAsync(Guid tenantId, int page, int pageSize, Guid? partnerId = null);
    Task<ContactDetailDto?> GetContactByIdAsync(Guid tenantId, Guid id);
    Task<ContactDetailDto> CreateContactAsync(Guid tenantId, CreateContactDto dto, Guid createdByUserId);
    Task<ContactDetailDto> UpdateContactAsync(Guid tenantId, Guid id, UpdateContactDto dto);
    Task DeleteContactAsync(Guid tenantId, Guid id, Guid deletedByUserId);
    Task SetPrimaryContactAsync(Guid tenantId, Guid partnerId, Guid contactId);
}
