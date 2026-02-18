using MediatR;

namespace ModulerERP.HR.Application.Features.Attendance.Commands;

public record CheckOutCommand(Guid EmployeeId, DateTime? Time = null) : IRequest;
