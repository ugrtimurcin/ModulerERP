using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.Sales.Application.DTOs;
using ModulerERP.Sales.Infrastructure.Persistence;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.Sales.Infrastructure.Features.Invoices.Queries;

public record GetAllInvoicesQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<InvoiceDto>>;

public class GetAllInvoicesQueryHandler : IRequestHandler<GetAllInvoicesQuery, PagedResult<InvoiceDto>>
{
    private readonly SalesDbContext _context;
    public GetAllInvoicesQueryHandler(SalesDbContext context) => _context = context;

    public async Task<PagedResult<InvoiceDto>> Handle(GetAllInvoicesQuery request, CancellationToken ct)
    {
        var query = _context.Invoices.Include(i => i.Lines).AsNoTracking();

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var data = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(i => new InvoiceDto(
                i.Id, i.InvoiceNumber, i.OrderId, null,
                i.PartnerId, "Partner", i.Status,
                i.CurrencyId, "CUR", i.ExchangeRate,
                i.InvoiceDate, i.DueDate,
                i.ShippingAddress, i.BillingAddress, i.PaymentTerms, i.Notes,
                i.SubTotal, i.DiscountAmount, i.TaxAmount, i.TotalAmount,
                i.PaidAmount, i.BalanceDue,
                i.Lines.Select(l => new InvoiceLineDto(
                    l.Id, l.ProductId, "Product",
                    l.Description, l.Quantity, l.UnitOfMeasureId, "UOM",
                    l.UnitPrice, l.DiscountPercent, l.DiscountAmount,
                    l.TaxPercent, l.LineTotal)).ToList(),
                i.CreatedAt, "System"))
            .ToListAsync(ct);

        return new PagedResult<InvoiceDto>(data, request.Page, request.PageSize, totalCount, totalPages);
    }
}

// ── GetById ──
public record GetInvoiceByIdQuery(Guid Id) : IRequest<InvoiceDto?>;

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto?>
{
    private readonly SalesDbContext _context;
    public GetInvoiceByIdQueryHandler(SalesDbContext context) => _context = context;

    public async Task<InvoiceDto?> Handle(GetInvoiceByIdQuery request, CancellationToken ct)
    {
        var i = await _context.Invoices
            .Include(x => x.Lines)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (i is null) return null;

        return new InvoiceDto(
            i.Id, i.InvoiceNumber, i.OrderId, null,
            i.PartnerId, "Partner", i.Status,
            i.CurrencyId, "CUR", i.ExchangeRate,
            i.InvoiceDate, i.DueDate,
            i.ShippingAddress, i.BillingAddress, i.PaymentTerms, i.Notes,
            i.SubTotal, i.DiscountAmount, i.TaxAmount, i.TotalAmount,
            i.PaidAmount, i.BalanceDue,
            i.Lines.Select(l => new InvoiceLineDto(
                l.Id, l.ProductId, "Product",
                l.Description, l.Quantity, l.UnitOfMeasureId, "UOM",
                l.UnitPrice, l.DiscountPercent, l.DiscountAmount,
                l.TaxPercent, l.LineTotal)).ToList(),
            i.CreatedAt, "System");
    }
}
