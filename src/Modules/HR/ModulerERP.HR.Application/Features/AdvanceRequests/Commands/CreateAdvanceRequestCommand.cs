using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.AdvanceRequests.Commands;

public record CreateAdvanceRequestCommand(Guid EmployeeId, decimal Amount, string Description) : IRequest<AdvanceRequestDto>;
