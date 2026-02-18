using MediatR;

namespace ModulerERP.HR.Application.Features.LeaveRequests.Commands;

public record ApproveLeaveRequestCommand(Guid Id) : IRequest;
