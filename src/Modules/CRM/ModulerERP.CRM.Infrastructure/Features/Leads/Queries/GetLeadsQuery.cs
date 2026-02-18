using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Infrastructure.Persistence;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.CRM.Infrastructure.Features.Leads.Queries;

public record GetLeadsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    Guid? AssignedUserId = null) : IRequest<PagedResult<LeadListDto>>;

public class GetLeadsQueryHandler : IRequestHandler<GetLeadsQuery, PagedResult<LeadListDto>>
{
    private readonly CRMDbContext _context;

    public GetLeadsQueryHandler(CRMDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<LeadListDto>> Handle(GetLeadsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Leads.AsQueryable();

        if (!string.IsNullOrEmpty(request.Status)
            && Enum.TryParse<CRM.Domain.Enums.LeadStatus>(request.Status, out var parsedStatus))
            query = query.Where(l => l.Status == parsedStatus);

        if (request.AssignedUserId.HasValue)
            query = query.Where(l => l.AssignedUserId == request.AssignedUserId);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var data = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(l => new LeadListDto(
                l.Id,
                l.Title,
                l.FirstName,
                l.LastName,
                l.FirstName + " " + l.LastName,
                l.Company,
                l.Email,
                l.Phone,
                l.Status.ToString(),
                l.Source,
                l.AssignedUserId,
                null,
                l.IsActive,
                l.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<LeadListDto>(data, request.Page, request.PageSize, totalCount, totalPages);
    }
}
