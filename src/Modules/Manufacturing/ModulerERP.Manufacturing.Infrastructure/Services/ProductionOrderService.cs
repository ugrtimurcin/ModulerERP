using Microsoft.EntityFrameworkCore;
using ModulerERP.Manufacturing.Application.DTOs;
using ModulerERP.Manufacturing.Application.Interfaces;
using ModulerERP.Manufacturing.Domain.Entities;
using ModulerERP.Manufacturing.Domain.Enums;
using ModulerERP.Manufacturing.Infrastructure.Persistence;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.Manufacturing.Infrastructure.Services;

public class ProductionOrderService : IProductionOrderService
{
    private readonly ManufacturingDbContext _context;

    public ProductionOrderService(ManufacturingDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProductionOrderListDto>> GetOrdersAsync(Guid tenantId, int page, int pageSize, int? status = null, CancellationToken ct = default)
    {
        var query = _context.ProductionOrders
            .Where(o => o.TenantId == tenantId);

        if (status.HasValue)
            query = query.Where(o => (int)o.Status == status.Value);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var data = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new ProductionOrderListDto(
                o.Id,
                o.OrderNumber,
                o.ProductId,
                null,
                (int)o.Status,
                o.Status.ToString(),
                o.PlannedQuantity,
                o.ProducedQuantity,
                o.PlannedStartDate,
                o.PlannedEndDate,
                o.Priority,
                o.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<ProductionOrderListDto>(data, page, pageSize, totalCount, totalPages);
    }

    public async Task<ProductionOrderDetailDto?> GetOrderByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var order = await _context.ProductionOrders
            .Include(o => o.Bom)
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == id, ct);

        if (order == null) return null;

        return MapToDetailDto(order);
    }

    public async Task<ProductionOrderDetailDto> CreateOrderAsync(Guid tenantId, CreateProductionOrderDto dto, Guid userId, CancellationToken ct = default)
    {
        var order = ProductionOrder.Create(
            tenantId,
            dto.OrderNumber,
            dto.BomId,
            dto.ProductId,
            dto.PlannedQuantity,
            dto.WarehouseId,
            userId,
            dto.PlannedStartDate,
            dto.PlannedEndDate,
            dto.Priority);

        // Generate lines from BOM components
        var bom = await _context.BillOfMaterials
            .Include(b => b.Components)
            .FirstOrDefaultAsync(b => b.Id == dto.BomId, ct);

        if (bom != null)
        {
            foreach (var component in bom.Components)
            {
                var requiredQty = component.Quantity * (dto.PlannedQuantity / bom.Quantity);
                var line = ProductionOrderLine.Create(
                    order.Id,
                    component.ProductId,
                    requiredQty,
                    component.UnitOfMeasureId);
                order.Lines.Add(line);
            }
        }

        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync(ct);

        return (await GetOrderByIdAsync(tenantId, order.Id, ct))!;
    }

    public async Task<ProductionOrderDetailDto> UpdateOrderAsync(Guid tenantId, Guid id, UpdateProductionOrderDto dto, CancellationToken ct = default)
    {
        var order = await _context.ProductionOrders
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == id, ct)
            ?? throw new KeyNotFoundException("Production order not found");

        await _context.SaveChangesAsync(ct);
        return (await GetOrderByIdAsync(tenantId, id, ct))!;
    }

    public async Task DeleteOrderAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct = default)
    {
        var order = await _context.ProductionOrders
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == id, ct)
            ?? throw new KeyNotFoundException("Production order not found");

        order.Delete(userId);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<ProductionOrderDetailDto> PlanOrderAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var order = await GetOrder(tenantId, id, ct);
        order.Plan();
        await _context.SaveChangesAsync(ct);
        return (await GetOrderByIdAsync(tenantId, id, ct))!;
    }

    public async Task<ProductionOrderDetailDto> ReleaseOrderAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var order = await GetOrder(tenantId, id, ct);
        order.Release();
        await _context.SaveChangesAsync(ct);
        return (await GetOrderByIdAsync(tenantId, id, ct))!;
    }

    public async Task<ProductionOrderDetailDto> StartOrderAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var order = await GetOrder(tenantId, id, ct);
        order.Start();
        await _context.SaveChangesAsync(ct);
        return (await GetOrderByIdAsync(tenantId, id, ct))!;
    }

    public async Task<ProductionOrderDetailDto> CompleteOrderAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var order = await GetOrder(tenantId, id, ct);
        order.Complete();
        await _context.SaveChangesAsync(ct);
        return (await GetOrderByIdAsync(tenantId, id, ct))!;
    }

    public async Task<ProductionOrderDetailDto> CancelOrderAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var order = await GetOrder(tenantId, id, ct);
        order.Cancel();
        await _context.SaveChangesAsync(ct);
        return (await GetOrderByIdAsync(tenantId, id, ct))!;
    }

    public async Task<ProductionOrderDetailDto> RecordProductionAsync(Guid tenantId, Guid id, decimal quantity, CancellationToken ct = default)
    {
        var order = await GetOrder(tenantId, id, ct);
        order.RecordProduction(quantity);
        await _context.SaveChangesAsync(ct);
        return (await GetOrderByIdAsync(tenantId, id, ct))!;
    }

    private async Task<ProductionOrder> GetOrder(Guid tenantId, Guid id, CancellationToken ct)
    {
        return await _context.ProductionOrders
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == id, ct)
            ?? throw new KeyNotFoundException("Production order not found");
    }

    private ProductionOrderDetailDto MapToDetailDto(ProductionOrder order)
    {
        var lineNum = 0;
        return new ProductionOrderDetailDto(
            order.Id,
            order.OrderNumber,
            order.BomId,
            order.Bom?.Name,
            order.ProductId,
            null,
            order.WarehouseId,
            null,
            (int)order.Status,
            order.Status.ToString(),
            order.PlannedQuantity,
            order.ProducedQuantity,
            order.PlannedStartDate,
            order.PlannedEndDate,
            order.ActualStartDate,
            order.ActualEndDate,
            order.Priority,
            order.Notes,
            order.Lines.Select(l => new ProductionOrderLineDto(l.Id, l.ProductId, null, l.RequiredQuantity, l.IssuedQuantity, ++lineNum)),
            order.CreatedAt);
    }
}
