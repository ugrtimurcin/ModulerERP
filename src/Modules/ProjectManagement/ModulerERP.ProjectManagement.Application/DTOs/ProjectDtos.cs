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
    ProjectBudgetDto Budget
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

public record ProjectBudgetDto(
    decimal TotalBudget,
    decimal MaterialBudget,
    decimal LaborBudget,
    decimal SubcontractorBudget,
    decimal ExpenseBudget
);
