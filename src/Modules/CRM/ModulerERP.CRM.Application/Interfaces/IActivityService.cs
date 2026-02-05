using ModulerERP.CRM.Application.DTOs;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.CRM.Application.Interfaces;

public interface IActivityService
{
    Task<PagedResult<ActivityDto>> GetActivitiesAsync(Guid tenantId, Guid? entityId, string? entityType, int page, int pageSize);
    Task<ActivityDto?> GetActivityByIdAsync(Guid tenantId, Guid id);
    Task<ActivityDto> CreateActivityAsync(Guid tenantId, CreateActivityDto dto, Guid createdByUserId);
    Task<ActivityDto> UpdateActivityAsync(Guid tenantId, Guid id, UpdateActivityDto dto);
    Task DeleteActivityAsync(Guid tenantId, Guid id, Guid deletedByUserId);
    Task MarkAsCompletedAsync(Guid tenantId, Guid id);
}
