using Microsoft.EntityFrameworkCore;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.ProjectManagement.Domain.Entities;
using ModulerERP.ProjectManagement.Infrastructure.Persistence;

namespace ModulerERP.ProjectManagement.Infrastructure.Services;

public class ProjectResourceService : IProjectResourceService
{
    private readonly ProjectManagementDbContext _context;

    public ProjectResourceService(ProjectManagementDbContext context)
    {
        _context = context;
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

        resource.SetTenant(tenantId);
        resource.SetCreator(userId);

        _context.ProjectResources.Add(resource);
        await _context.SaveChangesAsync();

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
