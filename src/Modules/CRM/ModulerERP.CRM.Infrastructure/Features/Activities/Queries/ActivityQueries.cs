using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Infrastructure.Persistence;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.CRM.Infrastructure.Features.Activities.Queries;

public record GetActivitiesQuery(
    string? EntityType = null, Guid? EntityId = null,
    int Page = 1, int PageSize = 20) : IRequest<PagedResult<ActivityDto>>;

public class GetActivitiesQueryHandler : IRequestHandler<GetActivitiesQuery, PagedResult<ActivityDto>>
{
    private readonly CRMDbContext _context;
    public GetActivitiesQueryHandler(CRMDbContext context) => _context = context;

    public async Task<PagedResult<ActivityDto>> Handle(GetActivitiesQuery request, CancellationToken ct)
    {
        var query = _context.Activities.AsQueryable();
        if (!string.IsNullOrEmpty(request.EntityType)) query = query.Where(a => a.EntityType == request.EntityType);
        if (request.EntityId.HasValue) query = query.Where(a => a.EntityId == request.EntityId);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var data = await query
            .OrderByDescending(a => a.ActivityDate)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(a => new ActivityDto(a.Id, a.Type, a.Subject, a.Description,
                a.ActivityDate, a.EntityType, a.EntityId, a.IsScheduled, a.IsCompleted,
                a.CompletedAt, a.CreatedAt, a.CreatedBy))
            .ToListAsync(ct);

        return new PagedResult<ActivityDto>(data, request.Page, request.PageSize, totalCount, totalPages);
    }
}
