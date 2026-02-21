using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Infrastructure.Persistence;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.CRM.Infrastructure.Features.Opportunities.Queries;

// ── Get All ──
public record GetOpportunitiesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Stage = null,
    Guid? AssignedUserId = null) : IRequest<PagedResult<OpportunityListDto>>;

public class GetOpportunitiesQueryHandler : IRequestHandler<GetOpportunitiesQuery, PagedResult<OpportunityListDto>>
{
    private readonly CRMDbContext _context;
    public GetOpportunitiesQueryHandler(CRMDbContext context) => _context = context;

    public async Task<PagedResult<OpportunityListDto>> Handle(GetOpportunitiesQuery request, CancellationToken ct)
    {
        var query = _context.Opportunities
            .Include(o => o.Partner).Include(o => o.Lead).AsQueryable();

        if (!string.IsNullOrEmpty(request.Stage)
            && Enum.TryParse<CRM.Domain.Enums.OpportunityStage>(request.Stage, out var parsedStage))
            query = query.Where(o => o.Stage == parsedStage);

        if (request.AssignedUserId.HasValue)
            query = query.Where(o => o.AssignedUserId == request.AssignedUserId);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var data = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OpportunityListDto(
                o.Id, o.Title, o.PartnerId,
                o.Partner != null ? o.Partner.Name : null,
                o.LeadId,
                o.Lead != null ? (o.Lead.FirstName + " " + o.Lead.LastName) : null,
                o.EstimatedValue.Amount, o.EstimatedValue.CurrencyCode,
                o.Stage.ToString(), o.Probability, o.WeightedValue,
                o.ExpectedCloseDate, o.AssignedUserId, null, o.TerritoryId, o.IsActive, o.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<OpportunityListDto>(data, request.Page, request.PageSize, totalCount, totalPages);
    }
}

// ── Get By Id ──
public record GetOpportunityByIdQuery(Guid Id) : IRequest<OpportunityDetailDto?>;

public class GetOpportunityByIdQueryHandler : IRequestHandler<GetOpportunityByIdQuery, OpportunityDetailDto?>
{
    private readonly CRMDbContext _context;
    public GetOpportunityByIdQueryHandler(CRMDbContext context) => _context = context;

    public async Task<OpportunityDetailDto?> Handle(GetOpportunityByIdQuery request, CancellationToken ct)
    {
        var o = await _context.Opportunities
            .Include(o => o.Partner).Include(o => o.Lead)
            .FirstOrDefaultAsync(o => o.Id == request.Id, ct);

        if (o == null) return null;

        return new OpportunityDetailDto(
            o.Id, o.Title, o.LeadId,
            o.Lead != null ? (o.Lead.FirstName + " " + o.Lead.LastName) : null,
            o.PartnerId, o.Partner?.Name,
            o.EstimatedValue.Amount, o.CurrencyId, o.EstimatedValue.CurrencyCode,
            o.Stage.ToString(), o.Probability, o.WeightedValue,
            o.ExpectedCloseDate, o.AssignedUserId, null, o.TerritoryId, o.CompetitorId, o.LossReasonId, o.IsActive, o.CreatedAt);
    }
}
