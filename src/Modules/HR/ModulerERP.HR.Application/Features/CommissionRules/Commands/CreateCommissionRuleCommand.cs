using MediatR;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Application.Features.CommissionRules.Commands;

public record CreateCommissionRuleCommand(string Role, decimal MinTargetAmount, decimal Percentage, CommissionBasis Basis) : IRequest<Guid>;
