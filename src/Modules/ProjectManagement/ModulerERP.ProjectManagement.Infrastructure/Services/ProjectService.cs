using Microsoft.EntityFrameworkCore;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.ProjectManagement.Domain.Entities;
using ModulerERP.ProjectManagement.Infrastructure.Persistence;

namespace ModulerERP.ProjectManagement.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly ProjectManagementDbContext _context;

    public ProjectService(ProjectManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectDto>> GetAllAsync(Guid tenantId)
    {
        return await _context.Projects
            .Include(x => x.BudgetLines)
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapToDto(x))
            .ToListAsync();
    }

    public async Task<ProjectDto> GetByIdAsync(Guid tenantId, Guid id)
    {
        var project = await _context.Projects
            .Include(x => x.BudgetLines)
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
            // BudgetLines initialized as empty
        };

        project.SetTenant(tenantId);
        project.SetCreator(userId);

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

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

    public async Task<ProjectBudgetLineDto> AddBudgetLineAsync(Guid tenantId, Guid projectId, CreateBudgetLineDto dto)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == projectId && x.TenantId == tenantId && !x.IsDeleted);

        if (project == null) throw new KeyNotFoundException($"Project {projectId} not found.");

        var line = new ProjectBudgetLine
        {
            ProjectId = projectId,
            CostCode = dto.CostCode,
            Description = dto.Description,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            UnitOfMeasureId = dto.UnitOfMeasureId,
            Category = dto.Category
        };
        
        line.CalculateTotal();
        line.SetTenant(tenantId);
        line.SetCreator(Guid.Empty); // TODO: Pass user ID

        _context.ProjectBudgetLines.Add(line);
        await _context.SaveChangesAsync();

        return new ProjectBudgetLineDto(
            line.Id,
            line.ProjectId,
            line.CostCode,
            line.Description,
            line.Quantity,
            line.UnitOfMeasureId,
            line.UnitPrice,
            line.TotalAmount,
            line.Category
        );
    }

    public async Task UpdateBudgetLineAsync(Guid tenantId, Guid projectId, Guid lineId, UpdateBudgetLineDto dto)
    {
        var line = await _context.ProjectBudgetLines
            .FirstOrDefaultAsync(x => x.Id == lineId && x.ProjectId == projectId && x.TenantId == tenantId && !x.IsDeleted);

        if (line == null) throw new KeyNotFoundException($"Budget Line {lineId} not found.");

        line.CostCode = dto.CostCode;
        line.Description = dto.Description;
        line.Quantity = dto.Quantity;
        line.UnitPrice = dto.UnitPrice;
        line.UnitOfMeasureId = dto.UnitOfMeasureId;
        line.Category = dto.Category;
        
        line.CalculateTotal();
        // line.SetUpdater(userId);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteBudgetLineAsync(Guid tenantId, Guid projectId, Guid lineId)
    {
        var line = await _context.ProjectBudgetLines
            .FirstOrDefaultAsync(x => x.Id == lineId && x.ProjectId == projectId && x.TenantId == tenantId && !x.IsDeleted);

        if (line == null) return;

        line.Delete(Guid.Empty); // TODO: Pass user ID
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
            p.BudgetLines.Select(b => new ProjectBudgetLineDto(
                b.Id,
                b.ProjectId,
                b.CostCode,
                b.Description,
                b.Quantity,
                b.UnitOfMeasureId,
                b.UnitPrice,
                b.TotalAmount,
                b.Category
            )).ToList(),
            p.GetTotalBudget()
        );
    }
}
