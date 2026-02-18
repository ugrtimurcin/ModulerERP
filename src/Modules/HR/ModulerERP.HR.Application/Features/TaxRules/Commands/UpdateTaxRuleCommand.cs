using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.TaxRules.Commands;

public record UpdateTaxRuleCommand(Guid Id, string Name, decimal LowerLimit, decimal? UpperLimit, decimal Rate, int Order, DateTime EffectiveFrom, DateTime? EffectiveTo) : IRequest<TaxRuleDto>;
