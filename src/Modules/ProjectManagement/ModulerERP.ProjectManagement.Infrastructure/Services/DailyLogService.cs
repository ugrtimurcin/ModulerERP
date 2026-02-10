using Microsoft.EntityFrameworkCore;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.ProjectManagement.Domain.Entities;
using ModulerERP.ProjectManagement.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.IntegrationEvents; // Will create this next

namespace ModulerERP.ProjectManagement.Infrastructure.Services;

using MediatR; // for IPublisher

public class DailyLogService : IDailyLogService
{
    private readonly ProjectManagementDbContext _context;
    private readonly IPublisher _publisher; 

    public DailyLogService(ProjectManagementDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<List<DailyLogDto>> GetByProjectIdAsync(Guid tenantId, Guid projectId)
    {
        return await _context.DailyLogs
            .Include(x => x.ResourceUsages)
            .Include(x => x.MaterialUsages)
            .Where(x => x.ProjectId == projectId && x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.Date)
            .Select(x => MapToDto(x))
            .ToListAsync();
    }

    public async Task<DailyLogDto> GetByIdAsync(Guid tenantId, Guid id)
    {
        var log = await _context.DailyLogs
            .Include(x => x.ResourceUsages)
            .Include(x => x.MaterialUsages)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);

        if (log == null) throw new KeyNotFoundException($"Daily Log {id} not found.");

        return MapToDto(log);
    }

    public async Task<DailyLogDto> CreateAsync(Guid tenantId, Guid userId, CreateDailyLogDto dto)
    {
        // Validation: Check if log already exists for this date? (Optional, maybe allow multiple shifts)
        
        var log = new DailyLog
        {
            ProjectId = dto.ProjectId,
            Date = DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc),
            WeatherCondition = dto.WeatherCondition,
            SiteManagerNote = dto.SiteManagerNote,
            IsApproved = false
        };

        foreach (var r in dto.ResourceUsages)
        {
            log.ResourceUsages.Add(new DailyLogResourceUsage
            {
                ProjectResourceId = r.ProjectResourceId,
                ProjectTaskId = r.ProjectTaskId,
                HoursWorked = r.HoursWorked,
                Description = r.Description
            });
        }

        foreach (var m in dto.MaterialUsages)
        {
            log.MaterialUsages.Add(new DailyLogMaterialUsage
            {
                ProductId = m.ProductId,
                Quantity = m.Quantity,
                UnitOfMeasureId = m.UnitOfMeasureId,
                Location = m.Location
            });
        }
        
        log.SetTenant(tenantId);
        log.SetCreator(userId);

        _context.DailyLogs.Add(log);
        await _context.SaveChangesAsync();

        return MapToDto(log);
    }

    public async Task ApproveAsync(Guid tenantId, Guid userId, Guid id)
    {
        var log = await _context.DailyLogs
            .Include(x => x.MaterialUsages)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);

        if (log == null) throw new KeyNotFoundException($"Daily Log {id} not found.");
        
        if (log.IsApproved) return; // Already approved

        // Fetch Project to get VirtualWarehouseId
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == log.ProjectId);
        if (project == null) throw new InvalidOperationException("Project not found.");

        log.IsApproved = true;
        log.ApprovalDate = DateTime.UtcNow;
        log.ApprovedByUserId = userId;
        // log.SetUpdater(userId);

        await _context.SaveChangesAsync();

        // Publish Event for Inventory Integration
        if (log.MaterialUsages.Any() && project.VirtualWarehouseId.HasValue)
        {
            var evt = new DailyLogApprovedEvent(
                tenantId,
                log.ProjectId,
                project.VirtualWarehouseId.Value,
                log.Id,
                log.Date,
                log.MaterialUsages.Select(m => new DailyLogMaterialUsageItem(
                    m.ProductId,
                    m.Quantity,
                    m.UnitOfMeasureId
                )).ToList()
            );
            
            await _publisher.Publish(evt);
        }
    }

    private static DailyLogDto MapToDto(DailyLog log)
    {
        return new DailyLogDto(
            log.Id,
            log.ProjectId,
            log.Date,
            log.WeatherCondition,
            log.SiteManagerNote,
            log.IsApproved,
            log.ApprovalDate,
            log.ApprovedByUserId,
            log.ResourceUsages.Select(r => new DailyLogResourceUsageDto(
                r.Id,
                r.ProjectResourceId,
                r.ProjectTaskId,
                r.HoursWorked,
                r.Description
            )).ToList(),
            log.MaterialUsages.Select(m => new DailyLogMaterialUsageDto(
                m.Id,
                m.ProductId,
                m.Quantity,
                m.UnitOfMeasureId,
                m.Location
            )).ToList()
        );
    }
}
