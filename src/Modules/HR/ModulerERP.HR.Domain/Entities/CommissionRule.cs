using ModulerERP.SharedKernel.Entities;
using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Domain.Entities;

public class CommissionRule : BaseEntity
{
    public string Role { get; private set; } = string.Empty; // e.g., 'SalesRepresentative'
    public decimal MinTargetAmount { get; private set; }
    public decimal Percentage { get; private set; }
    public CommissionBasis Basis { get; private set; }

    private CommissionRule() { }

    public static CommissionRule Create(Guid tenantId, Guid createdBy, string role, decimal minTarget, decimal percentage, CommissionBasis basis)
    {
        var rule = new CommissionRule
        {
            Role = role,
            MinTargetAmount = minTarget,
            Percentage = percentage,
            Basis = basis
        };
        rule.SetTenant(tenantId);
        rule.SetCreator(createdBy);
        return rule;
    }
}
