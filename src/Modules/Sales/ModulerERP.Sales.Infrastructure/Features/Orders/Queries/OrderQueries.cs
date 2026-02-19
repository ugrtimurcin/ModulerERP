using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.Sales.Application.DTOs;
using ModulerERP.Sales.Infrastructure.Persistence;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.Sales.Infrastructure.Features.Orders.Queries;

public record GetAllOrdersQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<OrderDto>>;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, PagedResult<OrderDto>>
{
    private readonly SalesDbContext _context;
    public GetAllOrdersQueryHandler(SalesDbContext context) => _context = context;

    public async Task<PagedResult<OrderDto>> Handle(GetAllOrdersQuery request, CancellationToken ct)
    {
        var query = _context.Orders.Include(o => o.Lines).AsNoTracking();

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var data = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(o => new OrderDto(
                o.Id, o.OrderNumber, o.QuoteId,
                o.PartnerId, "Partner", o.Status,
                o.CurrencyId, "CUR", o.ExchangeRate,
                o.RequestedDeliveryDate, o.ShippingAddress, o.BillingAddress,
                o.PaymentTerms, o.Notes,
                o.SubTotal, o.DiscountAmount, o.TaxAmount, o.TotalAmount,
                o.Lines.Select(l => new OrderLineDto(
                    l.Id, l.ProductId, "Product", l.Description,
                    l.Quantity, l.UnitOfMeasureId, "UOM",
                    l.UnitPrice, l.DiscountPercent, l.DiscountAmount,
                    l.TaxPercent, l.LineTotal,
                    l.ShippedQuantity, l.InvoicedQuantity)).ToList(),
                o.CreatedAt, "System"))
            .ToListAsync(ct);

        return new PagedResult<OrderDto>(data, request.Page, request.PageSize, totalCount, totalPages);
    }
}

// ── GetById ──
public record GetOrderByIdQuery(Guid Id) : IRequest<OrderDto?>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly SalesDbContext _context;
    public GetOrderByIdQueryHandler(SalesDbContext context) => _context = context;

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        var o = await _context.Orders
            .Include(x => x.Lines)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (o is null) return null;

        return new OrderDto(
            o.Id, o.OrderNumber, o.QuoteId,
            o.PartnerId, "Partner", o.Status,
            o.CurrencyId, "CUR", o.ExchangeRate,
            o.RequestedDeliveryDate, o.ShippingAddress, o.BillingAddress,
            o.PaymentTerms, o.Notes,
            o.SubTotal, o.DiscountAmount, o.TaxAmount, o.TotalAmount,
            o.Lines.Select(l => new OrderLineDto(
                l.Id, l.ProductId, "Product", l.Description,
                l.Quantity, l.UnitOfMeasureId, "UOM",
                l.UnitPrice, l.DiscountPercent, l.DiscountAmount,
                l.TaxPercent, l.LineTotal,
                l.ShippedQuantity, l.InvoicedQuantity)).ToList(),
            o.CreatedAt, "System");
    }
}
