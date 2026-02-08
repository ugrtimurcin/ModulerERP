using ModulerERP.ProjectManagement.Application.DTOs;

namespace ModulerERP.ProjectManagement.Application.Interfaces;

public interface IProjectTaskService
{
    Task<List<ProjectTaskDto>> GetByProjectIdAsync(Guid tenantId, Guid projectId);
    Task<ProjectTaskDto> CreateAsync(Guid tenantId, Guid userId, CreateProjectTaskDto dto);
    Task UpdateTaskAsync(Guid tenantId, Guid taskId, UpdateProjectTaskDto dto);
    Task UpdateProgressAsync(Guid tenantId, UpdateProjectTaskProgressDto dto);
    Task DeleteTaskAsync(Guid tenantId, Guid userId, Guid taskId);
}
