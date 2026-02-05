using ModulerERP.CRM.Application.DTOs;

namespace ModulerERP.CRM.Application.Interfaces;

public interface ITagService
{
    Task<IEnumerable<TagListDto>> GetTagsAsync(Guid tenantId, string? entityType = null, CancellationToken ct = default);
    Task<TagDetailDto?> GetTagByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<TagDetailDto> CreateTagAsync(Guid tenantId, CreateTagDto dto, Guid userId, CancellationToken ct = default);
    Task<TagDetailDto> UpdateTagAsync(Guid tenantId, Guid id, UpdateTagDto dto, CancellationToken ct = default);
    Task DeleteTagAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct = default);
    
    // EntityTag operations
    Task<IEnumerable<EntityTagDto>> GetEntityTagsAsync(Guid tenantId, Guid entityId, string entityType, CancellationToken ct = default);
    Task<EntityTagDto> AddTagToEntityAsync(Guid tenantId, CreateEntityTagDto dto, CancellationToken ct = default);
    Task RemoveTagFromEntityAsync(Guid tenantId, Guid entityTagId, CancellationToken ct = default);
}
