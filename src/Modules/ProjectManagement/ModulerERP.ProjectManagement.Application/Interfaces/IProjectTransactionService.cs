using ModulerERP.ProjectManagement.Application.DTOs;

namespace ModulerERP.ProjectManagement.Application.Interfaces;

public interface IProjectTransactionService
{
    Task<List<ProjectTransactionDto>> GetByProjectIdAsync(Guid tenantId, Guid projectId);
    Task<ProjectTransactionDto> CreateAsync(Guid tenantId, Guid userId, CreateProjectTransactionDto dto);
}
