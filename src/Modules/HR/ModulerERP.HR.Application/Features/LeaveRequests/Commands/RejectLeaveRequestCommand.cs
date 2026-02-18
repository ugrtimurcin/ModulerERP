using MediatR;

namespace ModulerERP.HR.Application.Features.LeaveRequests.Commands;

public record RejectLeaveRequestCommand(Guid Id) : IRequest;
