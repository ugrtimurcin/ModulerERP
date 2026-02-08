using ModulerERP.ProjectManagement.Domain.Enums;

namespace ModulerERP.ProjectManagement.Application.DTOs;

public record ProjectFinancialSummaryDto(
    Guid ProjectId,
    decimal ContractAmount,
    decimal TotalBilled,
    decimal TotalCost,
    decimal ProjectedProfit,
    string CurrencyCode,
    List<ProjectCostBreakdownDto> CostBreakdown
);

public record ProjectCostBreakdownDto(
    ProjectTransactionType Type,
    decimal Amount
);
