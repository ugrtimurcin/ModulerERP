using ModulerERP.HR.Domain.Entities;

namespace ModulerERP.HR.Application.DTOs;

public record LeaveRequestDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    string LeavePolicyId,
    DateTime StartDate,
    DateTime EndDate,
    int DaysCount,
    string? Reason,
    LeaveStatus Status,
    Guid? ApprovedByUserId,
    DateTime CreatedAt
);

public record CreateLeaveRequestDto(
    Guid EmployeeId,
    Guid LeavePolicyId,
    DateTime StartDate,
    DateTime EndDate,
    int DaysCount,
    string? Reason
);

public record ApproveRejectLeaveDto(
    string? Comment
);
