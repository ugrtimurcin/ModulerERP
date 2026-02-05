using ModulerERP.ProjectManagement.Application.DTOs;

namespace ModulerERP.ProjectManagement.Application.Interfaces;

public interface IProjectDocumentService
{
    Task<List<ProjectDocumentDto>> GetByProjectIdAsync(Guid tenantId, Guid projectId);
    Task<ProjectDocumentDto> CreateAsync(Guid tenantId, Guid userId, CreateProjectDocumentDto dto);
}
