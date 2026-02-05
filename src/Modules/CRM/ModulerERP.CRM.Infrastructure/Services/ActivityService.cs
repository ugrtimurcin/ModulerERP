using Microsoft.EntityFrameworkCore;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.CRM.Infrastructure.Persistence;
using ModulerERP.SharedKernel.DTOs;
using ModulerERP.CRM.Domain.Enums;
using ModulerERP.SharedKernel.Entities;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ModulerERP.CRM.Infrastructure.Services;

public class ActivityService : IActivityService
{
    private readonly CRMDbContext _context;

    public ActivityService(CRMDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ActivityDto>> GetActivitiesAsync(Guid tenantId, Guid? entityId, string? entityType, int page, int pageSize)
    {
        var query = _context.Activities
            .Where(a => a.TenantId == tenantId);

        if (entityId.HasValue)
            query = query.Where(a => a.EntityId == entityId);

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var data = await query
            .OrderByDescending(a => a.ActivityDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ActivityDto(
                a.Id,
                a.Type,
                a.Subject,
                a.Description,
                a.ActivityDate,
                a.EntityType,
                a.EntityId,
                a.IsScheduled,
                a.IsCompleted,
                a.CompletedAt,
                a.CreatedAt,
                a.CreatedBy))
            .ToListAsync();

        return new PagedResult<ActivityDto>(data, page, pageSize, totalCount, totalPages);
    }

    public async Task<ActivityDto?> GetActivityByIdAsync(Guid tenantId, Guid id)
    {
        var a = await _context.Activities
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == id);

        if (a == null) return null;

        return new ActivityDto(
            a.Id,
            a.Type,
            a.Subject,
            a.Description,
            a.ActivityDate,
            a.EntityType,
            a.EntityId,
            a.IsScheduled,
            a.IsCompleted,
            a.CompletedAt,
            a.CreatedAt,
            a.CreatedBy);
    }

    public async Task<ActivityDto> CreateActivityAsync(Guid tenantId, CreateActivityDto dto, Guid createdByUserId)
    {
        var activity = ModulerERP.CRM.Domain.Entities.Activity.Create(
            tenantId,
            dto.Type,
            dto.Subject,
            dto.EntityType,
            dto.EntityId,
            dto.ActivityDate,
            createdByUserId,
            dto.Description,
            dto.IsScheduled);

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        return (await GetActivityByIdAsync(tenantId, activity.Id))!;
    }

    public async Task<ActivityDto> UpdateActivityAsync(Guid tenantId, Guid id, UpdateActivityDto dto)
    {
        var activity = await _context.Activities
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == id)
            ?? throw new KeyNotFoundException("Activity not found");

        activity.Update(
            dto.Subject,
            dto.Description,
            dto.ActivityDate);

        if (dto.IsCompleted.HasValue && dto.IsCompleted.Value && !activity.IsCompleted)
        {
            activity.MarkAsCompleted();
        }

        await _context.SaveChangesAsync();
        return (await GetActivityByIdAsync(tenantId, id))!;
    }

    public async Task DeleteActivityAsync(Guid tenantId, Guid id, Guid deletedByUserId)
    {
        var activity = await _context.Activities
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == id)
            ?? throw new KeyNotFoundException("Activity not found");

        activity.Delete(deletedByUserId);
        await _context.SaveChangesAsync();
    }

    public async Task MarkAsCompletedAsync(Guid tenantId, Guid id)
    {
        var activity = await _context.Activities
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == id)
            ?? throw new KeyNotFoundException("Activity not found");

        activity.MarkAsCompleted();
        await _context.SaveChangesAsync();
    }
}
