using ModulerERP.ProjectManagement.Application.DTOs;

namespace ModulerERP.ProjectManagement.Application.Interfaces;

public interface IProjectTaskService
{
    Task<List<ProjectTaskDto>> GetByProjectIdAsync(Guid tenantId, Guid projectId);
    Task<ProjectTaskDto> CreateAsync(Guid tenantId, Guid userId, CreateProjectTaskDto dto);
    Task UpdateProgressAsync(Guid tenantId, UpdateProjectTaskProgressDto dto);
}
