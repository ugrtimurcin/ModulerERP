using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.Sales.Application.DTOs;
using ModulerERP.Sales.Infrastructure.Persistence;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.Sales.Infrastructure.Features.Quotes.Queries;

public record GetAllQuotesQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<QuoteDto>>;

public class GetAllQuotesQueryHandler : IRequestHandler<GetAllQuotesQuery, PagedResult<QuoteDto>>
{
    private readonly SalesDbContext _context;
    public GetAllQuotesQueryHandler(SalesDbContext context) => _context = context;

    public async Task<PagedResult<QuoteDto>> Handle(GetAllQuotesQuery request, CancellationToken ct)
    {
        var query = _context.Quotes.Include(q => q.Lines).AsNoTracking();

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var data = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(q => new QuoteDto(
                q.Id, q.QuoteNumber, q.RevisionNumber,
                q.PartnerId, "Partner", q.OpportunityId, q.Status,
                q.CurrencyId, "CUR", q.ExchangeRate,
                q.SentDate, q.ValidUntil, q.PaymentTerms, q.Notes,
                q.SubTotal, q.DiscountAmount, q.TaxAmount, q.TotalAmount,
                q.Lines.Select(l => new QuoteLineDto(
                    l.Id, l.ProductId, "Product", l.Description,
                    l.Quantity, l.UnitOfMeasureId, "UOM",
                    l.UnitPrice, l.DiscountPercent, l.DiscountAmount,
                    l.TaxPercent, l.LineTotal)).ToList(),
                q.CreatedAt, "System"))
            .ToListAsync(ct);

        return new PagedResult<QuoteDto>(data, request.Page, request.PageSize, totalCount, totalPages);
    }
}

// ── GetById ──
public record GetQuoteByIdQuery(Guid Id) : IRequest<QuoteDto?>;

public class GetQuoteByIdQueryHandler : IRequestHandler<GetQuoteByIdQuery, QuoteDto?>
{
    private readonly SalesDbContext _context;
    public GetQuoteByIdQueryHandler(SalesDbContext context) => _context = context;

    public async Task<QuoteDto?> Handle(GetQuoteByIdQuery request, CancellationToken ct)
    {
        var q = await _context.Quotes
            .Include(x => x.Lines)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (q is null) return null;

        return new QuoteDto(
            q.Id, q.QuoteNumber, q.RevisionNumber,
            q.PartnerId, "Partner", q.OpportunityId, q.Status,
            q.CurrencyId, "CUR", q.ExchangeRate,
            q.SentDate, q.ValidUntil, q.PaymentTerms, q.Notes,
            q.SubTotal, q.DiscountAmount, q.TaxAmount, q.TotalAmount,
            q.Lines.Select(l => new QuoteLineDto(
                l.Id, l.ProductId, "Product", l.Description,
                l.Quantity, l.UnitOfMeasureId, "UOM",
                l.UnitPrice, l.DiscountPercent, l.DiscountAmount,
                l.TaxPercent, l.LineTotal)).ToList(),
            q.CreatedAt, "System");
    }
}
