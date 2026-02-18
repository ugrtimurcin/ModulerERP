namespace ModulerERP.HR.Application.DTOs;

public record TaxRuleDto(
    Guid Id,
    string Name,
    decimal LowerLimit,
    decimal UpperLimit,
    decimal Rate,
    int Order,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo
);

public record CreateTaxRuleDto(
    string Name,
    decimal LowerLimit,
    decimal? UpperLimit,
    decimal Rate,
    int Order,
    DateTime EffectiveFrom
);

public record UpdateTaxRuleDto(
    Guid Id,
    string Name,
    decimal LowerLimit,
    decimal? UpperLimit,
    decimal Rate,
    int Order,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo
);
