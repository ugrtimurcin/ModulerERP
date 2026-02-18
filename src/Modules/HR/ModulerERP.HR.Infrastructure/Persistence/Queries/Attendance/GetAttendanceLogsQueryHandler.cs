using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.Attendance.Queries;
using ModulerERP.HR.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.Attendance;

public class GetAttendanceLogsQueryHandler : IRequestHandler<GetAttendanceLogsQuery, IReadOnlyList<AttendanceLogDto>>
{
    private readonly HRDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAttendanceLogsQueryHandler(HRDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<AttendanceLogDto>> Handle(GetAttendanceLogsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;
        
        var query = from l in _context.AttendanceLogs
                    join e in _context.Employees on l.EmployeeId equals e.Id
                    where l.TenantId == tenantId
                    select new { Log = l, EmployeeName = e.FirstName + " " + e.LastName };

        if (request.EmployeeId.HasValue)
        {
            query = query.Where(x => x.Log.EmployeeId == request.EmployeeId);
        }

        if (request.Date.HasValue)
        {
            var start = request.Date.Value.Date;
            var end = start.AddDays(1);
            query = query.Where(x => x.Log.TransactionTime >= start && x.Log.TransactionTime < end);
        }

        var results = await query.ToListAsync(cancellationToken);

        return results.Select(x => new AttendanceLogDto(
            x.Log.Id, 
            x.Log.SupervisorId, 
            x.Log.EmployeeId, 
            x.EmployeeName,
            x.Log.TransactionTime, 
            x.Log.Type, 
            x.Log.LocationId, 
            x.Log.GpsCoordinates
        )).ToList();
    }
}
