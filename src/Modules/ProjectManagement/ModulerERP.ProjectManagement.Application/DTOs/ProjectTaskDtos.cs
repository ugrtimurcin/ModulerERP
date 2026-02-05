using ModulerERP.ProjectManagement.Domain.Enums;

namespace ModulerERP.ProjectManagement.Application.DTOs;

public record ProjectTaskDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    Guid? ParentTaskId,
    DateTime StartDate,
    DateTime DueDate,
    decimal CompletionPercentage,
    ProjectTaskStatus Status,
    Guid? AssignedEmployeeId,
    Guid? AssignedSubcontractorId
);

public record CreateProjectTaskDto(
    Guid ProjectId,
    string Name,
    Guid? ParentTaskId,
    DateTime StartDate,
    DateTime DueDate,
    Guid? AssignedEmployeeId,
    Guid? AssignedSubcontractorId
);

public record UpdateProjectTaskProgressDto(
    Guid TaskId,
    decimal CompletionPercentage,
    ProjectTaskStatus Status
);
