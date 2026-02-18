using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Infrastructure.Persistence;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.CRM.Infrastructure.Features.SupportTickets.Queries;

public record GetSupportTicketsQuery(
    int Page = 1, int PageSize = 20, Guid? PartnerId = null) : IRequest<PagedResult<SupportTicketListDto>>;

public class GetSupportTicketsQueryHandler : IRequestHandler<GetSupportTicketsQuery, PagedResult<SupportTicketListDto>>
{
    private readonly CRMDbContext _context;
    public GetSupportTicketsQueryHandler(CRMDbContext context) => _context = context;

    public async Task<PagedResult<SupportTicketListDto>> Handle(GetSupportTicketsQuery r, CancellationToken ct)
    {
        var query = _context.SupportTickets.Include(t => t.Partner).AsQueryable();
        if (r.PartnerId.HasValue) query = query.Where(t => t.PartnerId == r.PartnerId);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)r.PageSize);

        var data = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((r.Page - 1) * r.PageSize).Take(r.PageSize)
            .Select(t => new SupportTicketListDto(
                t.Id, t.Title, (int)t.Priority, t.Priority.ToString(),
                (int)t.Status, t.Status.ToString(), t.PartnerId,
                t.Partner != null ? t.Partner.Name : null,
                t.AssignedUserId, t.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<SupportTicketListDto>(data, r.Page, r.PageSize, totalCount, totalPages);
    }
}

public record GetSupportTicketByIdQuery(Guid Id) : IRequest<SupportTicketDetailDto?>;

public class GetSupportTicketByIdQueryHandler : IRequestHandler<GetSupportTicketByIdQuery, SupportTicketDetailDto?>
{
    private readonly CRMDbContext _context;
    public GetSupportTicketByIdQueryHandler(CRMDbContext context) => _context = context;

    public async Task<SupportTicketDetailDto?> Handle(GetSupportTicketByIdQuery r, CancellationToken ct)
    {
        var t = await _context.SupportTickets.Include(t => t.Partner)
            .FirstOrDefaultAsync(t => t.Id == r.Id, ct);
        if (t == null) return null;

        return new SupportTicketDetailDto(t.Id, t.Title, t.Description,
            (int)t.Priority, t.Priority.ToString(), (int)t.Status, t.Status.ToString(),
            t.PartnerId, t.Partner?.Name, t.AssignedUserId, t.Resolution, t.ResolvedAt, t.ClosedAt, t.CreatedAt);
    }
}
