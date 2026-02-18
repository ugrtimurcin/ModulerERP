using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.TaxRules.Commands;

public record CreateTaxRuleCommand(string Name, decimal LowerLimit, decimal? UpperLimit, decimal Rate, int Order, DateTime EffectiveFrom) : IRequest<TaxRuleDto>;
