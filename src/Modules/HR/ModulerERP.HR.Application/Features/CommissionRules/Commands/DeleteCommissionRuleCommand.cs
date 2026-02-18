using MediatR;

namespace ModulerERP.HR.Application.Features.CommissionRules.Commands;

public record DeleteCommissionRuleCommand(Guid Id) : IRequest;
