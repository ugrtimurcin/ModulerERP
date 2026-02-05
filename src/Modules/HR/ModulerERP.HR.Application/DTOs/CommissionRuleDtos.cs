using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Application.DTOs;

public record CommissionRuleDto(Guid Id, string Role, decimal MinTargetAmount, decimal Percentage, CommissionBasis Basis);
public record CreateCommissionRuleDto(string Role, decimal MinTargetAmount, decimal Percentage, CommissionBasis Basis);
