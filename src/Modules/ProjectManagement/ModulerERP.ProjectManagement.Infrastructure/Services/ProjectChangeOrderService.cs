using Microsoft.EntityFrameworkCore;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.ProjectManagement.Domain.Entities;
using ModulerERP.ProjectManagement.Domain.Enums;
using ModulerERP.ProjectManagement.Infrastructure.Persistence;

namespace ModulerERP.ProjectManagement.Infrastructure.Services;

public class ProjectChangeOrderService : IProjectChangeOrderService
{
    private readonly ProjectManagementDbContext _context;

    public ProjectChangeOrderService(ProjectManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectChangeOrderDto>> GetByProjectIdAsync(Guid tenantId, Guid projectId)
    {
        var orders = await _context.ProjectChangeOrders
            .Where(x => x.ProjectId == projectId && x.TenantId == tenantId)
            .OrderByDescending(x => x.OrderNo)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    public async Task<ProjectChangeOrderDto> CreateAsync(Guid tenantId, Guid userId, CreateChangeOrderDto dto)
    {
        // Get next order number
        var lastOrder = await _context.ProjectChangeOrders
            .Where(x => x.ProjectId == dto.ProjectId && x.TenantId == tenantId)
            .OrderByDescending(x => x.OrderNo)
            .FirstOrDefaultAsync();

        var nextOrderNo = (lastOrder?.OrderNo ?? 0) + 1;

        var entity = new ProjectChangeOrder
        {
            ProjectId = dto.ProjectId,
            OrderNo = nextOrderNo,
            Title = dto.Title,
            Description = dto.Description,
            AmountChange = dto.AmountChange,
            TimeExtensionDays = dto.TimeExtensionDays,
            Status = ChangeOrderStatus.Draft,
            RequestDate = DateTime.UtcNow
        };
        
        entity.SetTenant(tenantId);
        entity.SetCreator(userId);

        _context.ProjectChangeOrders.Add(entity);
        await _context.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task ApproveAsync(Guid tenantId, Guid userId, Guid changeOrderId)
    {
        var order = await _context.ProjectChangeOrders
            .FirstOrDefaultAsync(x => x.Id == changeOrderId && x.TenantId == tenantId);

        if (order == null) throw new KeyNotFoundException("Change order not found.");
        
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == order.ProjectId && x.TenantId == tenantId);

        if (project == null) throw new KeyNotFoundException("Project not found.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            order.Approve(userId);
            project.ApplyChangeOrder(order);
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task RejectAsync(Guid tenantId, Guid userId, Guid changeOrderId)
    {
        var order = await _context.ProjectChangeOrders
            .FirstOrDefaultAsync(x => x.Id == changeOrderId && x.TenantId == tenantId);

        if (order == null) throw new KeyNotFoundException("Change order not found.");

        order.Reject(userId);
        await _context.SaveChangesAsync();
    }

    private static ProjectChangeOrderDto MapToDto(ProjectChangeOrder entity)
    {
        return new ProjectChangeOrderDto(
            entity.Id,
            entity.ProjectId,
            entity.OrderNo,
            entity.Title,
            entity.Description,
            entity.AmountChange,
            entity.TimeExtensionDays,
            entity.Status,
            entity.RequestDate,
            entity.ApprovalDate,
            entity.ApproverId
        );
    }
}
