using ModulerERP.HR.Domain.Entities;

namespace ModulerERP.HR.Application.DTOs;

public record AdvanceRequestDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    DateTime RequestDate,
    decimal Amount,
    int Status, // Enum: Pending, Approved, Paid, Deducted
    string StatusName,
    string? Description
);

public record CreateAdvanceRequestDto(
    Guid EmployeeId,
    decimal Amount,
    string? Description
);
