using ModulerERP.SharedKernel.Entities;
using ModulerERP.Sales.Domain.Enums;

namespace ModulerERP.Sales.Domain.Entities;

/// <summary>
/// Sales quotation with multi-currency and revision support.
/// TRNC Critical - supports price freezing in different currencies.
/// </summary>
public class Quote : BaseEntity
{
    /// <summary>Quote number with revision (e.g., 'QT-2026-001-R2')</summary>
    public string QuoteNumber { get; private set; } = string.Empty;
    
    /// <summary>Revision number for tracking changes</summary>
    public int RevisionNumber { get; private set; } = 1;
    
    public Guid PartnerId { get; private set; }
    public Guid? OpportunityId { get; private set; }
    
    public QuoteStatus Status { get; private set; } = QuoteStatus.Draft;
    
    /// <summary>Quote currency</summary>
    public Guid CurrencyId { get; private set; }
    
    /// <summary>Frozen exchange rate at quote time</summary>
    public decimal ExchangeRate { get; private set; } = 1;
    
    /// <summary>When the quote was sent to customer</summary>
    public DateTime? SentDate { get; private set; }
    
    /// <summary>Quote expiration date</summary>
    public DateTime? ValidUntil { get; private set; }
    
    /// <summary>Snapshot shipping address as JSON</summary>
    public string? ShippingAddress { get; private set; }
    
    /// <summary>Snapshot billing address as JSON</summary>
    public string? BillingAddress { get; private set; }
    
    /// <summary>Payment terms (e.g., 'Net 30')</summary>
    public string? PaymentTerms { get; private set; }
    
    public string? Notes { get; private set; }
    
    /// <summary>Subtotal before discounts/taxes</summary>
    public decimal SubTotal { get; private set; }
    
    /// <summary>Total discount amount</summary>
    public decimal DiscountAmount { get; private set; }
    
    /// <summary>Total tax amount</summary>
    public decimal TaxAmount { get; private set; }
    
    /// <summary>Grand total</summary>
    public decimal TotalAmount { get; private set; }

    // Navigation
    public ICollection<QuoteLine> Lines { get; private set; } = new List<QuoteLine>();

    private Quote() { } // EF Core

    public static Quote Create(
        Guid tenantId,
        string quoteNumber,
        Guid partnerId,
        Guid currencyId,
        decimal exchangeRate,
        Guid createdByUserId,
        Guid? opportunityId = null,
        DateTime? validUntil = null,
        string? paymentTerms = null)
    {
        var quote = new Quote
        {
            QuoteNumber = quoteNumber,
            PartnerId = partnerId,
            CurrencyId = currencyId,
            ExchangeRate = exchangeRate,
            OpportunityId = opportunityId,
            ValidUntil = validUntil,
            PaymentTerms = paymentTerms
        };

        quote.SetTenant(tenantId);
        quote.SetCreator(createdByUserId);
        return quote;
    }

    public void Send()
    {
        Status = QuoteStatus.Sent;
        SentDate = DateTime.UtcNow;
    }

    public void Accept() => Status = QuoteStatus.Accepted;
    public void Reject() => Status = QuoteStatus.Rejected;
    public void Expire() => Status = QuoteStatus.Expired;

    public void CreateRevision()
    {
        RevisionNumber++;
        Status = QuoteStatus.Draft;
    }

    public void UpdateTotals(decimal subTotal, decimal discountAmount, decimal taxAmount)
    {
        SubTotal = subTotal;
        DiscountAmount = discountAmount;
        TaxAmount = taxAmount;
        TotalAmount = subTotal - discountAmount + taxAmount;
    }

    public void SetAddresses(string? shippingAddress, string? billingAddress)
    {
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
    }
}
