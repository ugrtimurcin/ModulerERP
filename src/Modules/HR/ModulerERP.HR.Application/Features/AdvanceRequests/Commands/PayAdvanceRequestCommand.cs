using MediatR;

namespace ModulerERP.HR.Application.Features.AdvanceRequests.Commands;

public record PayAdvanceRequestCommand(Guid Id) : IRequest;
