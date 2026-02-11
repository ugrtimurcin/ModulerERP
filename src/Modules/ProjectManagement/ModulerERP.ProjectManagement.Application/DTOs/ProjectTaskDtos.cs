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
    List<TaskResourceDto> AssignedResources
);

public record TaskResourceDto(
    Guid ProjectResourceId,
    string Role,
    decimal AllocationPercent
);

public record CreateProjectTaskDto(
    Guid ProjectId,
    string Name,
    Guid? ParentTaskId,
    DateTime StartDate,
    DateTime DueDate,
    List<Guid> AssignedResourceIds
);

public record UpdateProjectTaskProgressDto(
    Guid TaskId,
    decimal CompletionPercentage,
    ProjectTaskStatus Status
);

public record UpdateProjectTaskDto(
    Guid Id,
    string Name,
    Guid? ParentTaskId,
    DateTime StartDate,
    DateTime DueDate,
    List<Guid> AssignedResourceIds
);
