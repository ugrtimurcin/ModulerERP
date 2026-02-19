using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.Sales.Application.DTOs;
using ModulerERP.Sales.Infrastructure.Persistence;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.Sales.Infrastructure.Features.Shipments.Queries;

public record GetAllShipmentsQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<ShipmentDto>>;

public class GetAllShipmentsQueryHandler : IRequestHandler<GetAllShipmentsQuery, PagedResult<ShipmentDto>>
{
    private readonly SalesDbContext _context;
    public GetAllShipmentsQueryHandler(SalesDbContext context) => _context = context;

    public async Task<PagedResult<ShipmentDto>> Handle(GetAllShipmentsQuery request, CancellationToken ct)
    {
        var query = _context.Shipments.Include(s => s.Lines).AsNoTracking();

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var data = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(s => new ShipmentDto
            {
                Id = s.Id,
                ShipmentNumber = s.ShipmentNumber,
                OrderId = s.OrderId,
                OrderNumber = "",
                WarehouseId = s.WarehouseId,
                WarehouseName = "",
                Status = s.Status,
                Carrier = s.Carrier,
                TrackingNumber = s.TrackingNumber,
                EstimatedDeliveryDate = s.EstimatedDeliveryDate,
                ShippedDate = s.ShippedDate,
                DeliveredDate = s.DeliveredDate,
                ShippingAddress = s.ShippingAddress,
                Notes = s.Notes,
                CreatedAt = s.CreatedAt,
                Lines = s.Lines.Select(l => new ShipmentLineDto
                {
                    Id = l.Id,
                    OrderLineId = l.OrderLineId,
                    ProductId = l.ProductId,
                    ProductName = "",
                    ProductSku = "",
                    Quantity = l.Quantity,
                    LotNumber = l.LotNumber,
                    SerialNumbers = l.SerialNumbers
                }).ToList()
            })
            .ToListAsync(ct);

        return new PagedResult<ShipmentDto>(data, request.Page, request.PageSize, totalCount, totalPages);
    }
}

// ── GetById ──
public record GetShipmentByIdQuery(Guid Id) : IRequest<ShipmentDto?>;

public class GetShipmentByIdQueryHandler : IRequestHandler<GetShipmentByIdQuery, ShipmentDto?>
{
    private readonly SalesDbContext _context;
    public GetShipmentByIdQueryHandler(SalesDbContext context) => _context = context;

    public async Task<ShipmentDto?> Handle(GetShipmentByIdQuery request, CancellationToken ct)
    {
        var s = await _context.Shipments
            .Include(x => x.Lines)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (s is null) return null;

        return new ShipmentDto
        {
            Id = s.Id,
            ShipmentNumber = s.ShipmentNumber,
            OrderId = s.OrderId,
            OrderNumber = "",
            WarehouseId = s.WarehouseId,
            WarehouseName = "",
            Status = s.Status,
            Carrier = s.Carrier,
            TrackingNumber = s.TrackingNumber,
            EstimatedDeliveryDate = s.EstimatedDeliveryDate,
            ShippedDate = s.ShippedDate,
            DeliveredDate = s.DeliveredDate,
            ShippingAddress = s.ShippingAddress,
            Notes = s.Notes,
            CreatedAt = s.CreatedAt,
            Lines = s.Lines.Select(l => new ShipmentLineDto
            {
                Id = l.Id,
                OrderLineId = l.OrderLineId,
                ProductId = l.ProductId,
                ProductName = "",
                ProductSku = "",
                Quantity = l.Quantity,
                LotNumber = l.LotNumber,
                SerialNumbers = l.SerialNumbers
            }).ToList()
        };
    }
}
