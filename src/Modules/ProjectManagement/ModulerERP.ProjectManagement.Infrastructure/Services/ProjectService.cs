using Microsoft.EntityFrameworkCore;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.ProjectManagement.Domain.Entities;
using ModulerERP.ProjectManagement.Infrastructure.Persistence;

namespace ModulerERP.ProjectManagement.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly ProjectManagementDbContext _context;
    private readonly MediatR.IPublisher _publisher;

    public ProjectService(ProjectManagementDbContext context, MediatR.IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<List<ProjectDto>> GetAllAsync(Guid tenantId)
    {
        return await _context.Projects
            .Include(x => x.BoQItems)
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapToDto(x))
            .ToListAsync();
    }

    public async Task<ProjectDto> GetByIdAsync(Guid tenantId, Guid id)
    {
        var project = await _context.Projects
            .Include(x => x.BoQItems)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);

        if (project == null) throw new KeyNotFoundException($"Project {id} not found.");

        return MapToDto(project);
    }

    public async Task<ProjectDto> CreateAsync(Guid tenantId, Guid userId, CreateProjectDto dto)
    {
        var project = new Project
        {
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            CustomerId = dto.CustomerId,
            ProjectManagerId = dto.ProjectManagerId,
            ContractCurrencyId = dto.ContractCurrencyId,
            ContractAmount = dto.ContractAmount,
            StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc),
            TargetDate = dto.TargetDate.HasValue ? DateTime.SpecifyKind(dto.TargetDate.Value, DateTimeKind.Utc) : null,
            Status = Domain.Enums.ProjectStatus.Planning,
            CompletionPercentage = 0
            // BoQItems initialized as empty
        };

        project.SetTenant(tenantId);
        project.SetCreator(userId);

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        
        // Publish Event
        await _publisher.Publish(new ModulerERP.SharedKernel.IntegrationEvents.ProjectCreatedEvent(
            tenantId,
            project.Id,
            project.Code,
            project.Name
        ));

        return MapToDto(project);
    }

    public async Task UpdateAsync(Guid tenantId, Guid id, UpdateProjectDto dto)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);

        if (project == null) throw new KeyNotFoundException($"Project {id} not found.");

        project.Name = dto.Name;
        project.Description = dto.Description;
        project.Status = dto.Status;
        project.ActualFinishDate = dto.ActualFinishDate.HasValue ? DateTime.SpecifyKind(dto.ActualFinishDate.Value, DateTimeKind.Utc) : null;

        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(Guid tenantId, Guid id)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);

        if (project == null) return;

        project.Delete(Guid.Empty); // TODO: Pass user ID
        await _context.SaveChangesAsync();
    }

    public async Task<BillOfQuantitiesItemDto> AddBoQItemAsync(Guid tenantId, Guid projectId, CreateBoQItemDto dto)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == projectId && x.TenantId == tenantId && !x.IsDeleted);

        if (project == null) throw new KeyNotFoundException($"Project {projectId} not found.");

        var item = new BillOfQuantitiesItem
        {
            ProjectId = projectId,
            ParentId = dto.ParentId,
            ItemCode = dto.ItemCode,
            Description = dto.Description,
            Quantity = dto.Quantity,
            UnitOfMeasureId = dto.UnitOfMeasureId,
            ContractUnitPrice = dto.ContractUnitPrice,
            EstimatedUnitCost = dto.EstimatedUnitCost,
            Category = dto.Category
        };
        
        item.SetTenant(tenantId);
        item.SetCreator(Guid.Empty); // TODO: Pass user ID

        _context.BillOfQuantitiesItems.Add(item);
        await _context.SaveChangesAsync();

        return new BillOfQuantitiesItemDto(
            item.Id,
            item.ProjectId,
            item.ParentId,
            item.ItemCode,
            item.Description,
            item.Quantity,
            item.UnitOfMeasureId,
            item.ContractUnitPrice,
            item.EstimatedUnitCost,
            item.TotalContractAmount,
            item.TotalEstimatedCost,
            item.Category
        );
    }

    public async Task UpdateBoQItemAsync(Guid tenantId, Guid projectId, Guid itemId, UpdateBoQItemDto dto)
    {
        var item = await _context.BillOfQuantitiesItems
            .FirstOrDefaultAsync(x => x.Id == itemId && x.ProjectId == projectId && x.TenantId == tenantId && !x.IsDeleted);

        if (item == null) throw new KeyNotFoundException($"BoQ Item {itemId} not found.");

        item.ItemCode = dto.ItemCode;
        item.Description = dto.Description;
        item.Quantity = dto.Quantity;
        item.ContractUnitPrice = dto.ContractUnitPrice;
        item.EstimatedUnitCost = dto.EstimatedUnitCost;
        item.UnitOfMeasureId = dto.UnitOfMeasureId;
        item.Category = dto.Category;
        
        // item.SetUpdater(userId);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteBoQItemAsync(Guid tenantId, Guid projectId, Guid itemId)
    {
        var item = await _context.BillOfQuantitiesItems
            .FirstOrDefaultAsync(x => x.Id == itemId && x.ProjectId == projectId && x.TenantId == tenantId && !x.IsDeleted);

        if (item == null) return;

        item.Delete(Guid.Empty); // TODO: Pass user ID
        await _context.SaveChangesAsync();
    }

    private static ProjectDto MapToDto(Project p)
    {
        return new ProjectDto(
            p.Id,
            p.Code,
            p.Name,
            p.Description,
            p.CustomerId,
            p.ProjectManagerId,
            p.ContractCurrencyId,
            p.ContractAmount,
            p.StartDate,
            p.TargetDate,
            p.ActualFinishDate,
            p.Status,
            p.CompletionPercentage,
            p.BoQItems.Select(b => new BillOfQuantitiesItemDto(
                b.Id,
                b.ProjectId,
                b.ParentId,
                b.ItemCode,
                b.Description,
                b.Quantity,
                b.UnitOfMeasureId,
                b.ContractUnitPrice,
                b.EstimatedUnitCost,
                b.TotalContractAmount,
                b.TotalEstimatedCost,
                b.Category
            )).ToList(),
            p.GetTotalContractAmount(),
            p.GetTotalEstimatedCost()
        );
    }
}
