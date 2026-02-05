namespace ModulerERP.HR.Application.DTOs;

public record DepartmentDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? ManagerId,
    string? ManagerName
);

public record CreateDepartmentDto(
    string Name,
    string? Description,
    Guid? ManagerId
);

public record UpdateDepartmentDto(
    string Name,
    string? Description,
    Guid? ManagerId
);
