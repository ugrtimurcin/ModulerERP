using ModulerERP.ProjectManagement.Application.DTOs;

namespace ModulerERP.ProjectManagement.Application.Interfaces;

public interface IProjectResourceService
{
    Task<List<ProjectResourceDto>> GetByProjectIdAsync(Guid tenantId, Guid projectId);
    Task<ProjectResourceDto> CreateAsync(Guid tenantId, Guid userId, CreateProjectResourceDto dto);
    Task DeleteAsync(Guid tenantId, Guid id);
}
