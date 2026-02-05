using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Finance.Domain.Entities;

/// <summary>
/// Tax configuration for different tax types.
/// </summary>
public class TaxRate : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>Tax percentage rate</summary>
    public decimal Rate { get; private set; }
    
    /// <summary>Is this rate active?</summary>
    public bool IsActive { get; private set; } = true;
    
    /// <summary>GL Account for tax collection</summary>
    public Guid? TaxAccountId { get; private set; }

    // Navigation
    public Account? TaxAccount { get; private set; }

    private TaxRate() { } // EF Core

    public static TaxRate Create(
        Guid tenantId,
        string code,
        string name,
        decimal rate,
        Guid createdByUserId,
        Guid? taxAccountId = null)
    {
        var taxRate = new TaxRate
        {
            Code = code,
            Name = name,
            Rate = rate,
            TaxAccountId = taxAccountId
        };

        taxRate.SetTenant(tenantId);
        taxRate.SetCreator(createdByUserId);
        return taxRate;
    }

    public void Update(string name, decimal rate)
    {
        Name = name;
        Rate = rate;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public decimal Calculate(decimal amount) => amount * (Rate / 100);
}
