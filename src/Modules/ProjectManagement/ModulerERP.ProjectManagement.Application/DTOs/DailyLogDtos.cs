using ModulerERP.ProjectManagement.Domain.Entities;

namespace ModulerERP.ProjectManagement.Application.DTOs;

public record DailyLogDto(
    Guid Id,
    Guid ProjectId,
    DateTime Date,
    string WeatherCondition,
    string SiteManagerNote,
    bool IsApproved,
    DateTime? ApprovalDate,
    Guid? ApprovedByUserId,
    List<DailyLogResourceUsageDto> ResourceUsages,
    List<DailyLogMaterialUsageDto> MaterialUsages
);

public record DailyLogResourceUsageDto(
    Guid Id,
    Guid ProjectResourceId,
    Guid? ProjectTaskId,
    decimal HoursWorked,
    string Description
);

public record DailyLogMaterialUsageDto(
    Guid Id,
    Guid ProductId,
    decimal Quantity,
    Guid UnitOfMeasureId,
    string Location
);

public record CreateDailyLogDto(
    Guid ProjectId,
    DateTime Date,
    string WeatherCondition,
    string SiteManagerNote,
    List<CreateResourceUsageDto> ResourceUsages,
    List<CreateMaterialUsageDto> MaterialUsages
);

public record CreateResourceUsageDto(
    Guid ProjectResourceId,
    Guid? ProjectTaskId,
    decimal HoursWorked,
    string Description
);

public record CreateMaterialUsageDto(
    Guid ProductId,
    decimal Quantity,
    Guid UnitOfMeasureId,
    string Location
);
