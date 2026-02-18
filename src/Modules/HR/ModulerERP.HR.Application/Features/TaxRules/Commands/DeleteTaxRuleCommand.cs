using MediatR;

namespace ModulerERP.HR.Application.Features.TaxRules.Commands;

public record DeleteTaxRuleCommand(Guid Id) : IRequest;
