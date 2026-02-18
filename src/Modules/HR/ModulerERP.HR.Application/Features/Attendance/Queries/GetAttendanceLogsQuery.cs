using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.Attendance.Queries;

public record GetAttendanceLogsQuery(Guid? EmployeeId, DateTime? Date) : IRequest<IReadOnlyList<AttendanceLogDto>>;
