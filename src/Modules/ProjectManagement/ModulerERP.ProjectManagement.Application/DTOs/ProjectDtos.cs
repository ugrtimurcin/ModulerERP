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
    // Budgeting V2 (BoQ)
    List<BillOfQuantitiesItemDto> BoQItems,
    decimal TotalContractAmount, // Calculated from BoQ
    decimal TotalEstimatedCost   // Calculated from BoQ
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

public record BillOfQuantitiesItemDto(
    Guid Id,
    Guid ProjectId,
    Guid? ParentId,
    string ItemCode,
    string Description,
    decimal Quantity,
    Guid UnitOfMeasureId,
    decimal ContractUnitPrice,
    decimal EstimatedUnitCost,
    decimal TotalContractAmount,
    decimal TotalEstimatedCost,
    BudgetCategory Category
);

public record CreateBoQItemDto(
    Guid ProjectId,
    Guid? ParentId,
    string ItemCode,
    string Description,
    decimal Quantity,
    Guid UnitOfMeasureId,
    decimal ContractUnitPrice,
    decimal EstimatedUnitCost,
    BudgetCategory Category
);

public record UpdateBoQItemDto(
    string ItemCode,
    string Description,
    decimal Quantity,
    Guid UnitOfMeasureId,
    decimal ContractUnitPrice,
    decimal EstimatedUnitCost,
    BudgetCategory Category
);
