using MediatR;

namespace ModulerERP.HR.Application.Features.LeaveRequests.Commands;

public record CancelLeaveRequestCommand(Guid Id) : IRequest;
