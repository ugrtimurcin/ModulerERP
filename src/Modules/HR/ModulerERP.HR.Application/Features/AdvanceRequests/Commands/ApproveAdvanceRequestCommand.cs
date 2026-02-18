using MediatR;

namespace ModulerERP.HR.Application.Features.AdvanceRequests.Commands;

public record ApproveAdvanceRequestCommand(Guid Id) : IRequest;
