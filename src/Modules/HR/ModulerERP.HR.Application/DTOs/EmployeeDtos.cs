using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Application.DTOs;

public record EmployeeDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string IdentityNumber,
    string JobTitle,
    Guid DepartmentId,
    string DepartmentName, // Flattened for display
    Guid? SupervisorId,
    string? SupervisorName,
    decimal CurrentSalary,
    DateTime? StartDate,
    EmploymentStatus Status,
    string? QrToken
);

public record CreateEmployeeDto(
    string FirstName,
    string LastName,
    string Email,
    string IdentityNumber,
    string JobTitle,
    Guid DepartmentId,
    Guid? SupervisorId,
    decimal CurrentSalary,
    Guid? UserId // Optional linkage
);

public record UpdateEmployeeDto(
    string FirstName,
    string LastName,
    string Email,
    string JobTitle,
    Guid DepartmentId,
    Guid? SupervisorId,
    decimal CurrentSalary,
    EmploymentStatus Status
);
