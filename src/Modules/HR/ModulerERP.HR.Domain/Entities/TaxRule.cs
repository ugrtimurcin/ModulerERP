using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.HR.Domain.Entities;

public class TaxRule : BaseEntity
{
    public string Name { get; private set; } = string.Empty; // e.g., "KKTC 2026 Gelir Vergisi"
    public decimal LowerLimit { get; private set; } // 0
    public decimal UpperLimit { get; private set; } // 999999999999.99 for last bracket.
    public decimal Rate { get; private set; } // 0.10
    public int Order { get; private set; }
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }

    private const decimal MaxUpperLimit = 999999999999.99m; // Fits in (18,2)

    private TaxRule() { }

    public static TaxRule Create(Guid tenantId, Guid createdBy, string name, decimal lower, decimal? upper, decimal rate, int order, DateTime effectiveFrom)
    {
        var rule = new TaxRule
        {
            Name = name,
            LowerLimit = lower,
            UpperLimit = upper ?? MaxUpperLimit,
            Rate = rate,
            Order = order,
            EffectiveFrom = effectiveFrom
        };
        rule.SetTenant(tenantId);
        rule.SetCreator(createdBy);
        return rule;
    }

    public void Expire(DateTime effectiveTo)
    {
        EffectiveTo = effectiveTo;
    }

    public void Update(string name, decimal lower, decimal? upper, decimal rate, int order, DateTime effectiveFrom, DateTime? effectiveTo)
    {
        Name = name;
        LowerLimit = lower;
        UpperLimit = upper ?? MaxUpperLimit;
        Rate = rate;
        Order = order;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
    }
}
