namespace ModulerERP.ProjectManagement.Application.DTOs;

public record ResourceRateCardDto(
    Guid Id,
    Guid? ProjectId,
    Guid? EmployeeId,
    string? EmployeeName,
    Guid? AssetId,
    string? AssetName,
    decimal HourlyRate,
    decimal OvertimeRate,
    Guid CurrencyId,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo
);

public record CreateResourceRateCardDto(
    Guid? ProjectId,
    Guid? EmployeeId,
    Guid? AssetId,
    decimal HourlyRate,
    decimal OvertimeRate,
    Guid CurrencyId,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo
);

public record UpdateResourceRateCardDto(
    decimal HourlyRate,
    decimal OvertimeRate,
    Guid CurrencyId,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo
);
