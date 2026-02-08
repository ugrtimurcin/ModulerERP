using ModulerERP.ProjectManagement.Application.DTOs;

namespace ModulerERP.ProjectManagement.Application.Interfaces;

public interface IProjectFinancialService
{
    Task<ProjectFinancialSummaryDto> GetProjectFinancialSummaryAsync(Guid tenantId, Guid projectId);
}
