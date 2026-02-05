using Microsoft.EntityFrameworkCore;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.CRM.Infrastructure.Persistence;

namespace ModulerERP.CRM.Infrastructure.Services;

public class TagService : ITagService
{
    private readonly CRMDbContext _context;

    public TagService(CRMDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TagListDto>> GetTagsAsync(Guid tenantId, string? entityType = null, CancellationToken ct = default)
    {
        var query = _context.Tags
            .Where(t => t.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(t => t.EntityType == null || t.EntityType == entityType);

        return await query
            .OrderBy(t => t.Name)
            .Select(t => new TagListDto(t.Id, t.Name, t.ColorCode, t.EntityType))
            .ToListAsync(ct);
    }

    public async Task<TagDetailDto?> GetTagByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id, ct);

        if (tag == null) return null;

        return new TagDetailDto(tag.Id, tag.Name, tag.ColorCode, tag.EntityType, tag.CreatedAt);
    }

    public async Task<TagDetailDto> CreateTagAsync(Guid tenantId, CreateTagDto dto, Guid userId, CancellationToken ct = default)
    {
        var tag = Tag.Create(tenantId, dto.Name, userId, dto.ColorCode, dto.EntityType);

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync(ct);

        return new TagDetailDto(tag.Id, tag.Name, tag.ColorCode, tag.EntityType, tag.CreatedAt);
    }

    public async Task<TagDetailDto> UpdateTagAsync(Guid tenantId, Guid id, UpdateTagDto dto, CancellationToken ct = default)
    {
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id, ct)
            ?? throw new KeyNotFoundException("Tag not found");

        tag.Update(dto.Name, dto.ColorCode, dto.EntityType);
        await _context.SaveChangesAsync(ct);

        return new TagDetailDto(tag.Id, tag.Name, tag.ColorCode, tag.EntityType, tag.CreatedAt);
    }

    public async Task DeleteTagAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct = default)
    {
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id, ct)
            ?? throw new KeyNotFoundException("Tag not found");

        tag.Delete(userId);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<EntityTagDto>> GetEntityTagsAsync(Guid tenantId, Guid entityId, string entityType, CancellationToken ct = default)
    {
        return await _context.EntityTags
            .Include(et => et.Tag)
            .Where(et => et.TenantId == tenantId && et.EntityId == entityId && et.EntityType == entityType)
            .Select(et => new EntityTagDto(et.Id, et.TagId, et.Tag!.Name, et.Tag.ColorCode))
            .ToListAsync(ct);
    }

    public async Task<EntityTagDto> AddTagToEntityAsync(Guid tenantId, CreateEntityTagDto dto, CancellationToken ct = default)
    {
        // Check if tag exists
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == dto.TagId, ct)
            ?? throw new KeyNotFoundException("Tag not found");

        // Check if already assigned
        var exists = await _context.EntityTags
            .AnyAsync(et => et.TenantId == tenantId && 
                           et.TagId == dto.TagId && 
                           et.EntityId == dto.EntityId && 
                           et.EntityType == dto.EntityType, ct);

        if (exists)
            throw new InvalidOperationException("Tag already assigned to this entity");

        var entityTag = EntityTag.Create(tenantId, dto.TagId, dto.EntityId, dto.EntityType);

        _context.EntityTags.Add(entityTag);
        await _context.SaveChangesAsync(ct);

        return new EntityTagDto(entityTag.Id, tag.Id, tag.Name, tag.ColorCode);
    }

    public async Task RemoveTagFromEntityAsync(Guid tenantId, Guid entityTagId, CancellationToken ct = default)
    {
        var entityTag = await _context.EntityTags
            .FirstOrDefaultAsync(et => et.TenantId == tenantId && et.Id == entityTagId, ct)
            ?? throw new KeyNotFoundException("EntityTag not found");

        _context.EntityTags.Remove(entityTag);
        await _context.SaveChangesAsync(ct);
    }
}
