using MediatR;

namespace ModulerERP.HR.Application.Features.Attendance.Commands;

public record ApproveAttendanceCommand(Guid Id) : IRequest;
