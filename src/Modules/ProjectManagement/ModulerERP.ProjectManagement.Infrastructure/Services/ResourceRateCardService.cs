using Microsoft.EntityFrameworkCore;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.ProjectManagement.Domain.Entities;
using ModulerERP.ProjectManagement.Infrastructure.Persistence;

namespace ModulerERP.ProjectManagement.Infrastructure.Services;

public class ResourceRateCardService : IResourceRateCardService
{
    private readonly ProjectManagementDbContext _context;

    public ResourceRateCardService(ProjectManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<ResourceRateCardDto>> GetAllAsync(Guid tenantId, Guid? projectId)
    {
        var query = _context.ResourceRateCards
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);

        if (projectId.HasValue)
        {
            // Return Global cards + Project specific cards
            query = query.Where(x => x.ProjectId == projectId || x.ProjectId == null);
        }
        else
        {
            // Return only Global cards? Or all?
            // Usually settings page wants all global definitions.
            query = query.Where(x => x.ProjectId == null);
        }

        return await query
            .OrderByDescending(x => x.EffectiveFrom)
            .Select(x => new ResourceRateCardDto(
                x.Id,
                x.ProjectId,
                x.EmployeeId,
                null, // Name not available
                x.AssetId,
                null, // Name not available
                x.HourlyRate,
                x.CurrencyId,
                x.EffectiveFrom,
                x.EffectiveTo
            ))
            .ToListAsync();
    }

    public async Task<ResourceRateCardDto> CreateAsync(Guid tenantId, Guid userId, CreateResourceRateCardDto dto)
    {
        var entity = new ResourceRateCard
        {
            ProjectId = dto.ProjectId,
            EmployeeId = dto.EmployeeId,
            AssetId = dto.AssetId,
            HourlyRate = dto.HourlyRate,
            CurrencyId = dto.CurrencyId,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo
        };

        entity.SetTenant(tenantId);
        entity.SetCreator(userId);

        _context.ResourceRateCards.Add(entity);
        await _context.SaveChangesAsync();

        return new ResourceRateCardDto(
            entity.Id,
            entity.ProjectId,
            entity.EmployeeId,
            null,
            entity.AssetId,
            null,
            entity.HourlyRate,
            entity.CurrencyId,
            entity.EffectiveFrom,
            entity.EffectiveTo
        );
    }

    public async Task UpdateAsync(Guid tenantId, Guid userId, Guid id, UpdateResourceRateCardDto dto)
    {
        var entity = await _context.ResourceRateCards
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);

        if (entity == null) throw new KeyNotFoundException("Rate Card not found");

        entity.HourlyRate = dto.HourlyRate;
        entity.CurrencyId = dto.CurrencyId;
        entity.EffectiveFrom = dto.EffectiveFrom;
        entity.EffectiveTo = dto.EffectiveTo;

        entity.SetUpdater(userId);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid tenantId, Guid userId, Guid id)
    {
        var entity = await _context.ResourceRateCards
             .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);

        if (entity == null) return;

        entity.Delete(userId);
        await _context.SaveChangesAsync();
    }
}
