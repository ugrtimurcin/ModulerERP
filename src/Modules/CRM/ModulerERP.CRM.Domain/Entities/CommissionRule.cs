using ModulerERP.SharedKernel.Entities;
using ModulerERP.CRM.Domain.Enums;

namespace ModulerERP.CRM.Domain.Entities;

/// <summary>
/// Commission rules for sales staff.
/// Calculated by Finance module based on these rules.
/// </summary>
public class CommissionRule : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>Applicable user/sales rep</summary>
    public Guid? UserId { get; private set; }
    
    /// <summary>Applicable partner group</summary>
    public Guid? PartnerGroupId { get; private set; }
    
    /// <summary>Applicable product category</summary>
    public Guid? ProductCategoryId { get; private set; }
    
    /// <summary>Calculation basis: Invoiced, Collected, or GrossProfit</summary>
    public CommissionBasis Basis { get; private set; } = CommissionBasis.InvoicedAmount;
    
    /// <summary>Commission percentage</summary>
    public decimal Rate { get; private set; }
    
    /// <summary>Priority for overlapping rules (higher = priority)</summary>
    public int Priority { get; private set; }
    
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }

    // Navigation
    public BusinessPartnerGroup? PartnerGroup { get; private set; }

    private CommissionRule() { } // EF Core

    public static CommissionRule Create(
        Guid tenantId,
        string name,
        CommissionBasis basis,
        decimal rate,
        Guid createdByUserId,
        Guid? userId = null,
        Guid? partnerGroupId = null,
        Guid? productCategoryId = null,
        int priority = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (rate < 0 || rate > 100)
            throw new ArgumentException("Rate must be between 0 and 100");

        var rule = new CommissionRule
        {
            Name = name,
            UserId = userId,
            PartnerGroupId = partnerGroupId,
            ProductCategoryId = productCategoryId,
            Basis = basis,
            Rate = rate,
            Priority = priority
        };

        rule.SetTenant(tenantId);
        rule.SetCreator(createdByUserId);
        return rule;
    }

    public void SetValidityPeriod(DateTime? from, DateTime? to)
    {
        ValidFrom = from;
        ValidTo = to;
    }

    public void UpdateRate(decimal rate)
    {
        if (rate < 0 || rate > 100)
            throw new ArgumentException("Rate must be between 0 and 100");
        Rate = rate;
    }

    public bool IsValidAt(DateTime date)
    {
        if (ValidFrom.HasValue && date < ValidFrom.Value) return false;
        if (ValidTo.HasValue && date > ValidTo.Value) return false;
        return true;
    }
}
