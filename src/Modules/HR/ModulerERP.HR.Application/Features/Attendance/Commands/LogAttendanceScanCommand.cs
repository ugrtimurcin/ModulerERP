using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.Attendance.Commands;

public record LogAttendanceScanCommand(CreateAttendanceLogDto Dto) : IRequest<Guid>;
