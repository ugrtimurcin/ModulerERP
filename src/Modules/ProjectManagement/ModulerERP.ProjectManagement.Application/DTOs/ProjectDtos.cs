using ModulerERP.ProjectManagement.Domain.Entities;
using ModulerERP.ProjectManagement.Domain.Enums;

namespace ModulerERP.ProjectManagement.Application.DTOs;

public record ProjectDto(
    Guid Id,
    string Code,
    string Name,
    string Description,
    Guid? CustomerId,
    Guid? ProjectManagerId,
    Guid ContractCurrencyId,
    decimal ContractAmount,
    DateTime StartDate,
    DateTime? TargetDate,
    DateTime? ActualFinishDate,
    ProjectStatus Status,
    decimal CompletionPercentage,
    // Budgeting V2
    List<ProjectBudgetLineDto> BudgetLines,
    decimal TotalBudget
);

public record CreateProjectDto(
    string Code,
    string Name,
    string Description,
    Guid? CustomerId,
    Guid? ProjectManagerId,
    Guid ContractCurrencyId,
    decimal ContractAmount,
    DateTime StartDate,
    DateTime? TargetDate
);

public record UpdateProjectDto(
    string Name,
    string Description,
    ProjectStatus Status,
    DateTime? ActualFinishDate
);

public record ProjectBudgetLineDto(
    Guid Id,
    Guid ProjectId,
    string CostCode,
    string Description,
    decimal Quantity,
    Guid UnitOfMeasureId,
    decimal UnitPrice,
    decimal TotalAmount,
    BudgetCategory Category
);

public record CreateBudgetLineDto(
    Guid ProjectId,
    string CostCode,
    string Description,
    decimal Quantity,
    Guid UnitOfMeasureId,
    decimal UnitPrice,
    BudgetCategory Category
);

public record UpdateBudgetLineDto(
    string CostCode,
    string Description,
    decimal Quantity,
    Guid UnitOfMeasureId,
    decimal UnitPrice,
    BudgetCategory Category
);
