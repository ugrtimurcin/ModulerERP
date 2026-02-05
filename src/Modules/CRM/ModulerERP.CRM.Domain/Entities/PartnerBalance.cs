namespace ModulerERP.CRM.Domain.Entities;

/// <summary>
/// Multi-currency balance cache per partner.
/// TRNC Critical - tracks debt/credit in TRY, GBP, EUR, USD simultaneously.
/// Updated via triggers/background jobs on LedgerTransactions.
/// </summary>
public class PartnerBalance
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TenantId { get; private set; }
    public Guid PartnerId { get; private set; }
    
    /// <summary>Currency (TRY, GBP, USD, EUR)</summary>
    public Guid CurrencyId { get; private set; }
    
    /// <summary>Positive = Partner owes us (Debit). Negative = We owe partner (Credit).</summary>
    public decimal Balance { get; private set; }
    
    /// <summary>For cache invalidation logic</summary>
    public DateTime LastUpdated { get; private set; } = DateTime.UtcNow;

    // Navigation
    public BusinessPartner? Partner { get; private set; }

    private PartnerBalance() { } // EF Core

    public static PartnerBalance Create(Guid tenantId, Guid partnerId, Guid currencyId)
    {
        return new PartnerBalance
        {
            TenantId = tenantId,
            PartnerId = partnerId,
            CurrencyId = currencyId,
            Balance = 0
        };
    }

    public void UpdateBalance(decimal newBalance)
    {
        Balance = newBalance;
        LastUpdated = DateTime.UtcNow;
    }

    public void AddToBalance(decimal amount)
    {
        Balance += amount;
        LastUpdated = DateTime.UtcNow;
    }

    public void SubtractFromBalance(decimal amount)
    {
        Balance -= amount;
        LastUpdated = DateTime.UtcNow;
    }
}
