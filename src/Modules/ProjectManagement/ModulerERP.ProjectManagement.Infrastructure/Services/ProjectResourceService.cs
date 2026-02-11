using Microsoft.EntityFrameworkCore;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.ProjectManagement.Domain.Entities;
using MediatR;
using ModulerERP.ProjectManagement.Infrastructure.Persistence;
using ModulerERP.SharedKernel.IntegrationEvents;

namespace ModulerERP.ProjectManagement.Infrastructure.Services;

public class ProjectResourceService : IProjectResourceService
{
    private readonly ProjectManagementDbContext _context;
    private readonly IResourceCostProvider _costProvider;
    private readonly IPublisher _publisher;

    public ProjectResourceService(
        ProjectManagementDbContext context,
        IResourceCostProvider costProvider,
        IPublisher publisher)
    {
        _context = context;
        _costProvider = costProvider;
        _publisher = publisher;
    }

    public async Task<List<ProjectResourceDto>> GetByProjectIdAsync(Guid tenantId, Guid projectId)
    {
        return await _context.ProjectResources
            .Where(x => x.ProjectId == projectId && x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Role)
            .Select(x => new ProjectResourceDto(
                x.Id,
                x.ProjectId,
                x.EmployeeId,
                null, // TODO: Join with HR Module via Integration Event or Cached Data
                x.AssetId,
                null, // TODO: Join with Asset Module
                x.Role,
                x.HourlyCost,
                x.CurrencyId
            ))
            .ToListAsync();
    }

    public async Task<ProjectResourceDto> CreateAsync(Guid tenantId, Guid userId, CreateProjectResourceDto dto)
    {
        var resource = new ProjectResource
        {
            ProjectId = dto.ProjectId,
            EmployeeId = dto.EmployeeId,
            AssetId = dto.AssetId,
            Role = dto.Role,
            HourlyCost = dto.HourlyCost,
            CurrencyId = dto.CurrencyId
        };

        // Auto-fetch cost if not provided
        // Auto-fetch cost if not provided
        if (resource.HourlyCost == 0)
        {
            var today = DateTime.UtcNow.Date;
            
            // Define base query for active rate cards
            var rateCardQuery = _context.ResourceRateCards
                .Where(x => x.TenantId == tenantId &&
                           (x.EffectiveTo == null || x.EffectiveTo >= today) &&
                           x.EffectiveFrom <= today);

            ResourceRateCard? appliedRateCard = null;

            if (resource.EmployeeId.HasValue)
            {
                // Find matching Employee rates
                var rates = await rateCardQuery
                    .Where(x => x.EmployeeId == resource.EmployeeId.Value && 
                               (x.ProjectId == resource.ProjectId || x.ProjectId == null))
                    .ToListAsync();

                // Select best match: Project-specific first, then most recent effective date
                appliedRateCard = rates
                    .OrderByDescending(x => x.ProjectId.HasValue)
                    .ThenByDescending(x => x.EffectiveFrom)
                    .FirstOrDefault();

                if (appliedRateCard != null)
                {
                    resource.HourlyCost = appliedRateCard.HourlyRate;
                    resource.CurrencyId = appliedRateCard.CurrencyId;
                }
                else
                {
                    // Fallback to HR Module
                    var cost = await _costProvider.GetEmployeeHourlyCostAsync(resource.EmployeeId.Value);
                    if (cost.HasValue) resource.HourlyCost = cost.Value;
                }
            }
            else if (resource.AssetId.HasValue)
            {
                // Find matching Asset rates
                var rates = await rateCardQuery
                    .Where(x => x.AssetId == resource.AssetId.Value && 
                               (x.ProjectId == resource.ProjectId || x.ProjectId == null))
                    .ToListAsync();

                appliedRateCard = rates
                    .OrderByDescending(x => x.ProjectId.HasValue)
                    .ThenByDescending(x => x.EffectiveFrom)
                    .FirstOrDefault();

                if (appliedRateCard != null)
                {
                    resource.HourlyCost = appliedRateCard.HourlyRate;
                    resource.CurrencyId = appliedRateCard.CurrencyId;
                }
                else
                {
                    // Fallback to Fixed Assets Module
                    var cost = await _costProvider.GetAssetHourlyCostAsync(resource.AssetId.Value);
                    if (cost.HasValue) resource.HourlyCost = cost.Value;
                }
            }
        }

        resource.SetTenant(tenantId);
        resource.SetCreator(userId);

        _context.ProjectResources.Add(resource);
        await _context.SaveChangesAsync();

        // Publish Integration Event for Assets
        if (resource.AssetId.HasValue)
        {
            var project = await _context.Projects
                .Where(p => p.Id == resource.ProjectId)
                .Select(p => new { p.Name })
                .FirstOrDefaultAsync();

            if (project != null)
            {
                await _publisher.Publish(new ProjectResourceAssignedEvent(
                    resource.ProjectId,
                    project.Name,
                    resource.AssetId.Value,
                    tenantId,
                    DateTime.UtcNow
                ));
            }
        }

        return new ProjectResourceDto(
            resource.Id,
            resource.ProjectId,
            resource.EmployeeId,
            null,
            resource.AssetId,
            null,
            resource.Role,
            resource.HourlyCost,
            resource.CurrencyId
        );
    }

    public async Task DeleteAsync(Guid tenantId, Guid id)
    {
        var resource = await _context.ProjectResources
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);

        if (resource == null) return;

        resource.Delete(Guid.Empty); // TODO: Pass user ID
        await _context.SaveChangesAsync();
    }
}
