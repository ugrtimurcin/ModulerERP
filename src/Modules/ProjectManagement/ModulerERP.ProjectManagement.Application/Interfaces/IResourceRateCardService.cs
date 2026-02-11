using ModulerERP.ProjectManagement.Application.DTOs;

namespace ModulerERP.ProjectManagement.Application.Interfaces;

public interface IResourceRateCardService
{
    Task<List<ResourceRateCardDto>> GetAllAsync(Guid tenantId, Guid? projectId);
    Task<ResourceRateCardDto> CreateAsync(Guid tenantId, Guid userId, CreateResourceRateCardDto dto);
    Task UpdateAsync(Guid tenantId, Guid userId, Guid id, UpdateResourceRateCardDto dto);
    Task DeleteAsync(Guid tenantId, Guid userId, Guid id);
}
