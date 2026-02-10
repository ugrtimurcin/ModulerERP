using ModulerERP.ProjectManagement.Application.DTOs;

namespace ModulerERP.ProjectManagement.Application.Interfaces;

public interface IDailyLogService
{
    Task<List<DailyLogDto>> GetByProjectIdAsync(Guid tenantId, Guid projectId);
    Task<DailyLogDto> GetByIdAsync(Guid tenantId, Guid id);
    Task<DailyLogDto> CreateAsync(Guid tenantId, Guid userId, CreateDailyLogDto dto);
    Task ApproveAsync(Guid tenantId, Guid userId, Guid id);
}
