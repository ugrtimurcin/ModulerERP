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
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapToDto(x))
            .ToListAsync();
    }

    public async Task<ProjectDto> GetByIdAsync(Guid tenantId, Guid id)
    {
        var project = await _context.Projects
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
            StartDate = dto.StartDate,
            TargetDate = dto.TargetDate,
            Status = Domain.Enums.ProjectStatus.Planning,
            CompletionPercentage = 0,
            Budget = new ProjectBudget() 
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
        project.ActualFinishDate = dto.ActualFinishDate;

        await _context.SaveChangesAsync();
    }

    public async Task UpdateBudgetAsync(Guid tenantId, Guid id, ProjectBudgetDto budgetDto)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);

        if (project == null) throw new KeyNotFoundException($"Project {id} not found.");

        project.Budget = new ProjectBudget
        {
            TotalBudget = budgetDto.TotalBudget,
            MaterialBudget = budgetDto.MaterialBudget,
            LaborBudget = budgetDto.LaborBudget,
            SubcontractorBudget = budgetDto.SubcontractorBudget,
            ExpenseBudget = budgetDto.ExpenseBudget
        };

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
            new ProjectBudgetDto(
                p.Budget.TotalBudget,
                p.Budget.MaterialBudget,
                p.Budget.LaborBudget,
                p.Budget.SubcontractorBudget,
                p.Budget.ExpenseBudget
            )
        );
    }
}
