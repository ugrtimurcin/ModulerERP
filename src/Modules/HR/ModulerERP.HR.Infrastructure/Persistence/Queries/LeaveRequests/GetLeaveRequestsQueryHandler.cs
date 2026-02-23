using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.LeaveRequests.Queries;
using ModulerERP.HR.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.LeaveRequests;

public class GetLeaveRequestsQueryHandler : IRequestHandler<GetLeaveRequestsQuery, List<LeaveRequestDto>>
{
    private readonly HRDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetLeaveRequestsQueryHandler(HRDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<LeaveRequestDto>> Handle(GetLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;

        var query = _context.LeaveRequests
            .AsNoTracking()
            .Include(l => l.Employee)
            .Where(l => l.TenantId == tenantId && !l.IsDeleted);

        if (request.EmployeeId.HasValue)
        {
            query = query.Where(l => l.EmployeeId == request.EmployeeId.Value);
        }

        return await query
            .OrderByDescending(l => l.StartDate)
            .Select(l => new LeaveRequestDto(
                l.Id,
                l.EmployeeId,
                $"{l.Employee.FirstName} {l.Employee.LastName}",
                l.LeavePolicyId.ToString(),
                l.StartDate,
                l.EndDate,
                l.DaysCount,
                l.Reason,
                l.Status,
                l.ApprovedByUserId,
                l.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }
}
