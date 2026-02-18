using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.LeaveRequests.Queries;
using ModulerERP.HR.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.LeaveRequests;

public class GetLeaveRequestByIdQueryHandler : IRequestHandler<GetLeaveRequestByIdQuery, LeaveRequestDto?>
{
    private readonly HRDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetLeaveRequestByIdQueryHandler(HRDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<LeaveRequestDto?> Handle(GetLeaveRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;

        var l = await _context.LeaveRequests
            .AsNoTracking()
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.TenantId == tenantId && !x.IsDeleted, cancellationToken);

        if (l == null) return null;

        return new LeaveRequestDto(
            l.Id,
            l.EmployeeId,
            $"{l.Employee.FirstName} {l.Employee.LastName}",
            l.Type,
            l.StartDate,
            l.EndDate,
            l.DaysCount,
            l.Reason,
            l.Status,
            l.ApprovedByUserId,
            l.CreatedAt
        );
    }
}
