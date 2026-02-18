using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.AdvanceRequests.Queries;

public record GetAdvanceRequestsQuery(Guid? EmployeeId = null) : IRequest<IEnumerable<AdvanceRequestDto>>;
