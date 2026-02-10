using ModulerERP.ProjectManagement.Application.DTOs;

namespace ModulerERP.ProjectManagement.Application.Interfaces;

public interface IProjectService
{
    Task<List<ProjectDto>> GetAllAsync(Guid tenantId);
    Task<ProjectDto> GetByIdAsync(Guid tenantId, Guid id);
    Task<ProjectDto> CreateAsync(Guid tenantId, Guid userId, CreateProjectDto dto);
    Task UpdateAsync(Guid tenantId, Guid id, UpdateProjectDto dto);
    Task DeleteAsync(Guid tenantId, Guid id);
    Task<BillOfQuantitiesItemDto> AddBoQItemAsync(Guid tenantId, Guid projectId, CreateBoQItemDto dto);
    Task UpdateBoQItemAsync(Guid tenantId, Guid projectId, Guid itemId, UpdateBoQItemDto dto);
    Task DeleteBoQItemAsync(Guid tenantId, Guid projectId, Guid itemId);
}
