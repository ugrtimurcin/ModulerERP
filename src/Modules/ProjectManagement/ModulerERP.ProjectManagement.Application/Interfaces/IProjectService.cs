using ModulerERP.ProjectManagement.Application.DTOs;

namespace ModulerERP.ProjectManagement.Application.Interfaces;

public interface IProjectService
{
    Task<List<ProjectDto>> GetAllAsync(Guid tenantId);
    Task<ProjectDto> GetByIdAsync(Guid tenantId, Guid id);
    Task<ProjectDto> CreateAsync(Guid tenantId, Guid userId, CreateProjectDto dto);
    Task UpdateAsync(Guid tenantId, Guid id, UpdateProjectDto dto);
    Task DeleteAsync(Guid tenantId, Guid id);
    Task<ProjectBudgetLineDto> AddBudgetLineAsync(Guid tenantId, Guid projectId, CreateBudgetLineDto dto);
    Task UpdateBudgetLineAsync(Guid tenantId, Guid projectId, Guid lineId, UpdateBudgetLineDto dto);
    Task DeleteBudgetLineAsync(Guid tenantId, Guid projectId, Guid lineId);
}
