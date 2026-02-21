using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Infrastructure.Persistence;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.CRM.Infrastructure.Features.Activities.Queries;

public record GetActivitiesQuery(
    Guid? LeadId = null, Guid? OpportunityId = null, Guid? PartnerId = null,
    int Page = 1, int PageSize = 20) : IRequest<PagedResult<ActivityDto>>;

public class GetActivitiesQueryHandler : IRequestHandler<GetActivitiesQuery, PagedResult<ActivityDto>>
{
    private readonly CRMDbContext _context;
    public GetActivitiesQueryHandler(CRMDbContext context) => _context = context;

    public async Task<PagedResult<ActivityDto>> Handle(GetActivitiesQuery request, CancellationToken ct)
    {
        var query = _context.Activities.AsQueryable();
        if (request.LeadId.HasValue) query = query.Where(a => a.LeadId == request.LeadId);
        if (request.OpportunityId.HasValue) query = query.Where(a => a.OpportunityId == request.OpportunityId);
        if (request.PartnerId.HasValue) query = query.Where(a => a.PartnerId == request.PartnerId);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var data = await query
            .OrderByDescending(a => a.ActivityDate)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(a => new ActivityDto(a.Id, a.Type, a.Subject, a.Description,
                a.ActivityDate, a.LeadId, a.OpportunityId, a.PartnerId, a.IsScheduled, a.IsCompleted,
                a.CompletedAt, a.CreatedAt, a.CreatedBy))
            .ToListAsync(ct);

        return new PagedResult<ActivityDto>(data, request.Page, request.PageSize, totalCount, totalPages);
    }
}
