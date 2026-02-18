using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Application.DTOs;

public record EmployeeDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string IdentityNumber,
    CitizenshipType Citizenship,
    SocialSecurityType SocialSecurityType,
    string? WorkPermitNumber,
    DateTime? WorkPermitExpiryDate,
    string JobTitle,
    Guid DepartmentId,
    string DepartmentName,
    Guid? SupervisorId,
    string? SupervisorName,
    decimal CurrentSalary,
    DateTime StartDate,
    EmploymentStatus Status,
    string? QrToken,
    MaritalStatus MaritalStatus,
    bool IsSpouseWorking,
    int ChildCount,
    bool IsPensioner
);

public record CreateEmployeeDto(
    string FirstName,
    string LastName,
    string Email,
    string IdentityNumber,
    CitizenshipType Citizenship,
    SocialSecurityType SocialSecurityType,
    string? WorkPermitNumber,
    DateTime? WorkPermitExpiryDate,
    string JobTitle,
    Guid DepartmentId,
    Guid? SupervisorId,
    decimal CurrentSalary,
    decimal TransportAmount,
    string? BankName,
    string? Iban,
    Guid? UserId,
    MaritalStatus MaritalStatus,
    bool IsSpouseWorking,
    int ChildCount,
    bool IsPensioner
);

public record UpdateEmployeeDto(
    string FirstName,
    string LastName,
    string Email,
    CitizenshipType Citizenship,
    SocialSecurityType SocialSecurityType,
    string? WorkPermitNumber,
    DateTime? WorkPermitExpiryDate,
    string JobTitle,
    Guid DepartmentId,
    Guid? SupervisorId,
    decimal CurrentSalary,
    decimal TransportAmount,
    string? BankName,
    string? Iban,
    EmploymentStatus Status,
    MaritalStatus MaritalStatus,
    bool IsSpouseWorking,
    int ChildCount,
    bool IsPensioner
);
