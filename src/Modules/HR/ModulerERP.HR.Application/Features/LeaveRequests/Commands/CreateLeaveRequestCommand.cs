using MediatR;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Domain.Entities;

namespace ModulerERP.HR.Application.Features.LeaveRequests.Commands;

public record CreateLeaveRequestCommand(
    Guid EmployeeId,
    LeaveType Type,
    DateTime StartDate,
    DateTime EndDate,
    int DaysCount,
    string? Reason
) : IRequest<LeaveRequestDto>;
