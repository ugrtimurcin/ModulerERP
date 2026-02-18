using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.Attendance.Queries;

public record GetAttendanceByDateQuery(DateTime Date) : IRequest<IEnumerable<DailyAttendanceDto>>;
