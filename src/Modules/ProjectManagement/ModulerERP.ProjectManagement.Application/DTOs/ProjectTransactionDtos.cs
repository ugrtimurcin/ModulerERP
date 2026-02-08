using ModulerERP.ProjectManagement.Domain.Enums;

namespace ModulerERP.ProjectManagement.Application.DTOs;

public record ProjectTransactionDto(
    Guid Id,
    Guid ProjectId,
    Guid? ProjectTaskId,
    string SourceModule,
    Guid SourceRecordId,
    string Description,
    decimal Amount,
    Guid CurrencyId,
    decimal ExchangeRate,
    decimal AmountReporting,
    ProjectTransactionType Type,
    DateTime Date // BaseEntity CreatedAt
);

public record CreateProjectTransactionDto(
    Guid ProjectId,
    Guid? ProjectTaskId,
    string SourceModule,
    Guid SourceRecordId,
    string Description,
    decimal Amount,
    Guid CurrencyId,
    decimal ExchangeRate,
    ProjectTransactionType Type,
    DateTime Date
);
