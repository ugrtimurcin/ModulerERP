using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.AdvanceRequests.Queries;
using ModulerERP.HR.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.AdvanceRequests;

public class GetAdvanceRequestsQueryHandler : IRequestHandler<GetAdvanceRequestsQuery, IEnumerable<AdvanceRequestDto>>
{
    private readonly HRDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAdvanceRequestsQueryHandler(HRDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<AdvanceRequestDto>> Handle(GetAdvanceRequestsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;
        
        var query = from r in _context.AdvanceRequests
                    join e in _context.Employees on r.EmployeeId equals e.Id
                    where r.TenantId == tenantId
                    select new { Request = r, EmployeeName = e.FirstName + " " + e.LastName };

        if (request.EmployeeId.HasValue)
        {
            query = query.Where(x => x.Request.EmployeeId == request.EmployeeId);
        }

        var results = await query.ToListAsync(cancellationToken);

        return results.Select(x => new AdvanceRequestDto(
            x.Request.Id,
            x.Request.EmployeeId,
            x.EmployeeName,
            x.Request.RequestDate,
            x.Request.Amount,
            (int)x.Request.Status,
            x.Request.Status.ToString(),
            x.Request.Description
        ));
    }
}
