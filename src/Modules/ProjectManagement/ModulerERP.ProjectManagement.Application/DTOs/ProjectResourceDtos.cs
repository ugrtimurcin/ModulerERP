using ModulerERP.SharedKernel.Enums;

namespace ModulerERP.ProjectManagement.Application.DTOs;

public record ProjectResourceDto(
    Guid Id,
    Guid ProjectId,
    Guid? EmployeeId,
    string? EmployeeName, // Joined from HR
    Guid? AssetId,
    string? AssetName, // Joined from Fixed Assets
    string Role,
    decimal HourlyCost,
    Guid CurrencyId
);

public record CreateProjectResourceDto(
    Guid ProjectId,
    Guid? EmployeeId,
    Guid? AssetId,
    string Role,
    decimal HourlyCost,
    Guid CurrencyId
);
