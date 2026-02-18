using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.Attendance.Commands;

public record CheckInCommand(Guid EmployeeId, DateTime? Time = null) : IRequest<DailyAttendanceDto>;
