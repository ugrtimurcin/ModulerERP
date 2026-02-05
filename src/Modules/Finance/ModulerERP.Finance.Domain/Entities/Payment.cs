using ModulerERP.SharedKernel.Entities;
using ModulerERP.Finance.Domain.Enums;

namespace ModulerERP.Finance.Domain.Entities;

/// <summary>
/// Payment records (incoming and outgoing).
/// </summary>
public class Payment : BaseEntity
{
    /// <summary>Payment number (e.g., 'PAY-2026-001')</summary>
    public string PaymentNumber { get; private set; } = string.Empty;
    
    public PaymentDirection Direction { get; private set; }
    public PaymentMethod Method { get; private set; }
    
    public Guid PartnerId { get; private set; }
    public Guid CurrencyId { get; private set; }
    public decimal ExchangeRate { get; private set; } = 1;
    
    /// <summary>Payment amount in currency</summary>
    public decimal Amount { get; private set; }
    
    /// <summary>Bank/Cash account used</summary>
    public Guid AccountId { get; private set; }
    
    public DateTime PaymentDate { get; private set; }
    
    /// <summary>Check/Transfer reference number</summary>
    public string? ReferenceNumber { get; private set; }
    
    /// <summary>Linked journal entry</summary>
    public Guid? JournalEntryId { get; private set; }
    
    public string? Notes { get; private set; }

    // Navigation
    public Account? Account { get; private set; }
    public JournalEntry? JournalEntry { get; private set; }
    public ICollection<PaymentAllocation> Allocations { get; private set; } = new List<PaymentAllocation>();

    private Payment() { } // EF Core

    public static Payment Create(
        Guid tenantId,
        string paymentNumber,
        PaymentDirection direction,
        PaymentMethod method,
        Guid partnerId,
        Guid currencyId,
        decimal exchangeRate,
        decimal amount,
        Guid accountId,
        DateTime paymentDate,
        Guid createdByUserId,
        string? referenceNumber = null)
    {
        var payment = new Payment
        {
            PaymentNumber = paymentNumber,
            Direction = direction,
            Method = method,
            PartnerId = partnerId,
            CurrencyId = currencyId,
            ExchangeRate = exchangeRate,
            Amount = amount,
            AccountId = accountId,
            PaymentDate = paymentDate,
            ReferenceNumber = referenceNumber
        };

        payment.SetTenant(tenantId);
        payment.SetCreator(createdByUserId);
        return payment;
    }

    public void LinkJournalEntry(Guid journalEntryId) => JournalEntryId = journalEntryId;
}
