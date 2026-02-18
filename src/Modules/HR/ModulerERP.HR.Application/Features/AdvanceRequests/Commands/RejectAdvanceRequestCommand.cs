using MediatR;

namespace ModulerERP.HR.Application.Features.AdvanceRequests.Commands;

public record RejectAdvanceRequestCommand(Guid Id) : IRequest;
