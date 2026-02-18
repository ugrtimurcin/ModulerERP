using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.LeaveRequests.Queries;

public record GetLeaveRequestByIdQuery(Guid Id) : IRequest<LeaveRequestDto?>;
