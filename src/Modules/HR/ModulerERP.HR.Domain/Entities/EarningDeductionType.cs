using ModulerERP.SharedKernel.Entities;
using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Domain.Entities;

public class EarningDeductionType : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public EarningDeductionCategory Category { get; private set; }
    public bool IsTaxable { get; private set; }
    public bool IsSgkExempt { get; private set; }
    public decimal? MaxExemptAmount { get; private set; } // If only partially exempt up to an amount
    public bool IsActive { get; private set; } = true;

    private EarningDeductionType() { }

    public static EarningDeductionType Create(Guid tenantId, Guid createdBy, string name, EarningDeductionCategory category, bool isTaxable, bool isSgkExempt, decimal? maxExemptAmount = null)
    {
        var edType = new EarningDeductionType
        {
            Name = name,
            Category = category,
            IsTaxable = isTaxable,
            IsSgkExempt = isSgkExempt,
            MaxExemptAmount = maxExemptAmount
        };
        edType.SetTenant(tenantId);
        edType.SetCreator(createdBy);
        return edType;
    }

    public void Update(string name, EarningDeductionCategory category, bool isTaxable, bool isSgkExempt, decimal? maxExemptAmount, bool isActive)
    {
        Name = name;
        Category = category;
        IsTaxable = isTaxable;
        IsSgkExempt = isSgkExempt;
        MaxExemptAmount = maxExemptAmount;
        IsActive = isActive;
    }
}
