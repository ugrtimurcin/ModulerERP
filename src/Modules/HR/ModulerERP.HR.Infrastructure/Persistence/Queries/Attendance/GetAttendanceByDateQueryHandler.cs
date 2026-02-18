using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.Attendance.Queries;
using ModulerERP.HR.Infrastructure.Persistence; // Direct context access for Query
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.Attendance; // Infrastructure namespace

public class GetAttendanceByDateQueryHandler : IRequestHandler<GetAttendanceByDateQuery, IEnumerable<DailyAttendanceDto>>
{
    private readonly HRDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAttendanceByDateQueryHandler(HRDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<DailyAttendanceDto>> Handle(GetAttendanceByDateQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;
        var dateOnly = request.Date.Date;

        // Optimized Query with Join/Select
        var query = from a in _context.DailyAttendances
                    join e in _context.Employees on a.EmployeeId equals e.Id
                    where a.TenantId == tenantId && a.Date == dateOnly
                    select new DailyAttendanceDto(
                        a.Id,
                        a.EmployeeId,
                        e.FirstName + " " + e.LastName,
                        a.Date,
                        a.CheckInTime,
                        a.CheckOutTime,
                        a.TotalWorkedMins,
                        a.Overtime1xMins + a.Overtime2xMins,
                        a.Status
                    );

        return await query.ToListAsync(cancellationToken);
    }
}
