using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.CommissionRules.Queries;

public record GetCommissionRulesQuery : IRequest<IReadOnlyList<CommissionRuleDto>>;
