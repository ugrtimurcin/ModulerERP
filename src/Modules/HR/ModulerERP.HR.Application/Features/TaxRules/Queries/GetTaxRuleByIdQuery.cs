using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.TaxRules.Queries;

public record GetTaxRuleByIdQuery(Guid Id) : IRequest<TaxRuleDto?>;
