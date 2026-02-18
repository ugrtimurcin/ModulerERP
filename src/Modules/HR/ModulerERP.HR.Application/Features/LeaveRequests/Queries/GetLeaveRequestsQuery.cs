using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.LeaveRequests.Queries;

public record GetLeaveRequestsQuery(Guid? EmployeeId) : IRequest<List<LeaveRequestDto>>;
