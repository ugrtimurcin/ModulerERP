using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.HR.Domain.Entities;

public class MinimumWage : BaseEntity
{
    public decimal GrossAmount { get; private set; }
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }
    public string Note { get; private set; } = string.Empty;

    private MinimumWage() { }

    public static MinimumWage Create(Guid tenantId, Guid createdBy, decimal amount, DateTime effectiveFrom, string note = "")
    {
        var mw = new MinimumWage
        {
            GrossAmount = amount,
            EffectiveFrom = effectiveFrom,
            Note = note
        };
        mw.SetTenant(tenantId);
        mw.SetCreator(createdBy);
        return mw;
    }

    public void Expire(DateTime effectiveTo)
    {
        EffectiveTo = effectiveTo;
    }
}
