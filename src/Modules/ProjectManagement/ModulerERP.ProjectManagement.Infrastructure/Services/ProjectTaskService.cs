using Microsoft.EntityFrameworkCore;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.ProjectManagement.Domain.Entities;
using ModulerERP.ProjectManagement.Infrastructure.Persistence;

namespace ModulerERP.ProjectManagement.Infrastructure.Services;

public class ProjectTaskService : IProjectTaskService
{
    private readonly ProjectManagementDbContext _context;

    public ProjectTaskService(ProjectManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectTaskDto>> GetByProjectIdAsync(Guid tenantId, Guid projectId)
    {
        return await _context.ProjectTasks
            .Where(x => x.TenantId == tenantId && x.ProjectId == projectId && !x.IsDeleted)
            .Select(x => new ProjectTaskDto(
                x.Id,
                x.ProjectId,
                x.Name,
                x.ParentTaskId,
                x.StartDate,
                x.DueDate,
                x.CompletionPercentage,
                x.Status,
                x.AssignedEmployeeId,
                x.AssignedSubcontractorId
            ))
            .ToListAsync();
    }

    public async Task<ProjectTaskDto> CreateAsync(Guid tenantId, Guid userId, CreateProjectTaskDto dto)
    {
        var task = new ProjectTask
        {
            ProjectId = dto.ProjectId,
            Name = dto.Name,
            ParentTaskId = dto.ParentTaskId,
            StartDate = dto.StartDate,
            DueDate = dto.DueDate,
            CompletionPercentage = 0,
            Status = Domain.Enums.ProjectTaskStatus.Todo,
            AssignedEmployeeId = dto.AssignedEmployeeId,
            AssignedSubcontractorId = dto.AssignedSubcontractorId
        };
        
        task.SetTenant(tenantId);
        task.SetCreator(userId);

        _context.ProjectTasks.Add(task);
        await _context.SaveChangesAsync();

        return new ProjectTaskDto(
            task.Id, task.ProjectId, task.Name, task.ParentTaskId, task.StartDate, task.DueDate, 
            task.CompletionPercentage, task.Status, task.AssignedEmployeeId, task.AssignedSubcontractorId);
    }

    public async Task UpdateProgressAsync(Guid tenantId, UpdateProjectTaskProgressDto dto)
    {
        var task = await _context.ProjectTasks
            .FirstOrDefaultAsync(x => x.Id == dto.TaskId && x.TenantId == tenantId && !x.IsDeleted);

        if (task == null) throw new KeyNotFoundException($"Task {dto.TaskId} not found.");

        if (task.CompletionPercentage != dto.CompletionPercentage || task.Status != dto.Status)
        {
            task.CompletionPercentage = dto.CompletionPercentage;
            task.Status = dto.Status;
            
            // Recalculate Project Progress
            await RecalculateProjectProgress(tenantId, task.ProjectId);
            
            await _context.SaveChangesAsync();
        }
    }

    private async Task RecalculateProjectProgress(Guid tenantId, Guid projectId)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.TenantId == tenantId);

        if (project == null) return;

        var tasks = await _context.ProjectTasks
            .Where(t => t.ProjectId == projectId && t.TenantId == tenantId && !t.IsDeleted)
            .ToListAsync();

        if (tasks.Any())
        {
            // Simple average for now. Can be weighted by duration or cost later.
            var totalProgress = tasks.Sum(t => t.CompletionPercentage);
            var count = tasks.Count;
            project.CompletionPercentage = totalProgress / count;
        }
        else
        {
            project.CompletionPercentage = 0;
        }
    }
}
