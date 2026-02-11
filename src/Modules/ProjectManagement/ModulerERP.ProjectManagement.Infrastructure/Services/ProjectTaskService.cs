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
            .Include(t => t.Resources)
            .ThenInclude(tr => tr.ProjectResource)
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
                x.Resources.Select(r => new TaskResourceDto(
                    r.ProjectResourceId, 
                    r.ProjectResource.Role, 
                    r.AllocationPercent
                )).ToList()
            ))
            .ToListAsync();
    }

    public async Task<ProjectTaskDto> CreateAsync(Guid tenantId, Guid userId, CreateProjectTaskDto dto)
    {
        // Validation: Ensure resources belong to project
        var validResources = await _context.ProjectResources
            .Where(r => r.ProjectId == dto.ProjectId && dto.AssignedResourceIds.Contains(r.Id))
            .ToListAsync();

        if (validResources.Count != dto.AssignedResourceIds.Distinct().Count())
            throw new ArgumentException("One or more selected resources are invalid.");

        // Check Availability
        var taskStartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc);
        var taskDueDate = DateTime.SpecifyKind(dto.DueDate, DateTimeKind.Utc);
        await CheckResourceAvailability(tenantId, taskStartDate, taskDueDate, dto.AssignedResourceIds);

        var task = new ProjectTask
        {
            ProjectId = dto.ProjectId,
            Name = dto.Name,
            ParentTaskId = dto.ParentTaskId,
            StartDate = taskStartDate,
            DueDate = taskDueDate,
            CompletionPercentage = 0,
            Status = Domain.Enums.ProjectTaskStatus.Todo
        };

        foreach(var res in validResources)
        {
            task.Resources.Add(new ProjectTaskResource { ProjectResourceId = res.Id, ProjectTaskId = task.Id });
        }
        
        task.SetTenant(tenantId);
        task.SetCreator(userId);

        _context.ProjectTasks.Add(task);
        await _context.SaveChangesAsync();

        return new ProjectTaskDto(
            task.Id, task.ProjectId, task.Name, task.ParentTaskId, task.StartDate, task.DueDate, 
            task.CompletionPercentage, task.Status, 
            task.Resources.Select(r => new TaskResourceDto(
                    r.ProjectResourceId, 
                    // We need to fetch the role from the validResources list since task.Resources[i].ProjectResource is not loaded yet
                    validResources.First(vr => vr.Id == r.ProjectResourceId).Role, 
                    r.AllocationPercent
                )).ToList());
    }

    public async Task UpdateTaskAsync(Guid tenantId, Guid taskId, UpdateProjectTaskDto dto)
    {
        var task = await _context.ProjectTasks
            .Include(t => t.Resources)
            .FirstOrDefaultAsync(x => x.Id == taskId && x.TenantId == tenantId && !x.IsDeleted);

        if (task == null) throw new KeyNotFoundException($"Task {taskId} not found.");

        // Validation: Ensure resources belong to project
        // Note: ProjectId is on the task, we trust the task belongs to the project.
        var validResources = await _context.ProjectResources
            .Where(r => r.ProjectId == task.ProjectId && dto.AssignedResourceIds.Contains(r.Id))
            .ToListAsync();
            
        if (validResources.Count != dto.AssignedResourceIds.Distinct().Count())
             throw new ArgumentException("One or more selected resources are invalid.");

        task.Name = dto.Name;
        task.ParentTaskId = dto.ParentTaskId;
        task.StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc);
        task.DueDate = DateTime.SpecifyKind(dto.DueDate, DateTimeKind.Utc);

        // Check Availability for the NEW set of resources and NEW dates
        // We need to check all proposed resources (existing ones that remain + new ones)
        await CheckResourceAvailability(tenantId, task.StartDate, task.DueDate, dto.AssignedResourceIds, task.Id);

        // Update Resources
        // 1. Remove resources not in DTO
        var toRemove = task.Resources.Where(r => !dto.AssignedResourceIds.Contains(r.ProjectResourceId)).ToList();
        foreach(var rem in toRemove)
        {
            task.Resources.Remove(rem);
        }

        // 2. Add resources in DTO but not in Task
        var existingIds = task.Resources.Select(r => r.ProjectResourceId).ToList();
        var toAdd = dto.AssignedResourceIds.Where(id => !existingIds.Contains(id)).ToList();

        foreach(var addId in toAdd)
        {
            task.Resources.Add(new ProjectTaskResource { ProjectResourceId = addId, ProjectTaskId = task.Id });
        }

        // Recalculate Project Progress (in case duration/weighting changes in future, or if we move tasks)
        await RecalculateProjectProgress(tenantId, task.ProjectId);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteTaskAsync(Guid tenantId, Guid userId, Guid taskId)
    {
        var task = await _context.ProjectTasks
            .FirstOrDefaultAsync(x => x.Id == taskId && x.TenantId == tenantId && !x.IsDeleted);

        if (task == null) throw new KeyNotFoundException($"Task {taskId} not found.");

        // Soft delete using BaseEntity method
        task.Delete(userId);
        
        // Also soft delete children
        var children = await _context.ProjectTasks
            .Where(x => x.ParentTaskId == taskId && x.TenantId == tenantId && !x.IsDeleted)
            .ToListAsync();
            
        foreach(var child in children)
        {
            child.Delete(userId);
        }

        await RecalculateProjectProgress(tenantId, task.ProjectId);
        await _context.SaveChangesAsync();
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

    private async Task CheckResourceAvailability(Guid tenantId, DateTime startDate, DateTime dueDate, List<Guid> projectResourceIds, Guid? currentTaskId = null)
    {
        // 1. Get the EmployeeIds and AssetIds for the requested ProjectResources
        var resourcesToCheck = await _context.ProjectResources
            .Where(x => projectResourceIds.Contains(x.Id))
            .Select(x => new { x.Id, x.EmployeeId, x.AssetId })
            .ToListAsync();

        var employeeIds = resourcesToCheck.Where(x => x.EmployeeId.HasValue).Select(x => x.EmployeeId.Value).ToList();
        var assetIds = resourcesToCheck.Where(x => x.AssetId.HasValue).Select(x => x.AssetId.Value).ToList();

        if (!employeeIds.Any() && !assetIds.Any()) return;

        // 2. Find overlapping tasks across ALL projects for this tenant
        // Overlap Logic: (StartA < EndB) && (EndA > StartB)
        
        var query = _context.ProjectTasks
            .Include(t => t.Resources)
                .ThenInclude(tr => tr.ProjectResource)
            .Where(t => 
                t.TenantId == tenantId && 
                !t.IsDeleted &&
                (currentTaskId == null || t.Id != currentTaskId) &&
                t.StartDate < dueDate && t.DueDate > startDate
            );

        // Optimization: Filter at DB level to only tasks that have matching resource types
        var conflictingTasks = await query
            .Where(t => t.Resources.Any(tr => 
                (tr.ProjectResource.EmployeeId.HasValue && employeeIds.Contains(tr.ProjectResource.EmployeeId.Value)) ||
                (tr.ProjectResource.AssetId.HasValue && assetIds.Contains(tr.ProjectResource.AssetId.Value))
            ))
            .Select(t => new 
            { 
                TaskName = t.Name,
                StartDate = t.StartDate,
                DueDate = t.DueDate
            })
            .ToListAsync();

        if (conflictingTasks.Any())
        {
            var conflict = conflictingTasks.First();
            throw new InvalidOperationException($"Resource conflict detected. One or more resources are already assigned to task '{conflict.TaskName}' from {conflict.StartDate:d} to {conflict.DueDate:d}.");
        }
    }
}
