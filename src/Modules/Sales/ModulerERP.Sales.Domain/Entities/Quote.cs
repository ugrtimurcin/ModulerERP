using ModulerERP.SharedKernel.Entities;
using ModulerERP.Sales.Domain.Enums;

namespace ModulerERP.Sales.Domain.Entities;

/// <summary>
/// Sales quotation with multi-currency and revision support.
/// KKTC Critical - supports price freezing in different currencies with local TRY equivalents.
/// </summary>
public class Quote : BaseEntity
{
    public string QuoteNumber { get; private set; } = string.Empty;
    public int RevisionNumber { get; private set; } = 1;

    public Guid PartnerId { get; private set; }
    public Guid? OpportunityId { get; private set; }

    public QuoteStatus Status { get; private set; } = QuoteStatus.Draft;

    // ── Transaction Currency ──
    public Guid CurrencyId { get; private set; }
    public decimal ExchangeRate { get; private set; } = 1;

    // ── Local Currency (KKTC: TRY) ──
    public Guid? LocalCurrencyId { get; private set; }
    public decimal LocalExchangeRate { get; private set; } = 1;
    public decimal LocalSubTotal { get; private set; }
    public decimal LocalTaxAmount { get; private set; }
    public decimal LocalTotalAmount { get; private set; }

    // ── Dates & Addresses ──
    public DateTime? SentDate { get; private set; }
    public DateTime? ValidUntil { get; private set; }
    public string? ShippingAddress { get; private set; }
    public string? BillingAddress { get; private set; }
    public string? PaymentTerms { get; private set; }
    public string? Notes { get; private set; }

    // ── Totals ──
    public decimal SubTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }

    // ── Document-level Discount & Withholding Tax (Stopaj) ──
    public decimal DocumentDiscountRate { get; private set; }
    public decimal DocumentDiscountAmount { get; private set; }
    public decimal WithholdingTaxRate { get; private set; }
    public decimal WithholdingTaxAmount { get; private set; }

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
        string? paymentTerms = null,
        Guid? localCurrencyId = null,
        decimal localExchangeRate = 1)
    {
        var quote = new Quote
        {
            QuoteNumber = quoteNumber,
            PartnerId = partnerId,
            CurrencyId = currencyId,
            ExchangeRate = exchangeRate,
            OpportunityId = opportunityId,
            ValidUntil = validUntil,
            PaymentTerms = paymentTerms,
            LocalCurrencyId = localCurrencyId,
            LocalExchangeRate = localExchangeRate
        };

        quote.SetTenant(tenantId);
        quote.SetCreator(createdByUserId);
        return quote;
    }

    // ── Status Transitions (with guards) ──

    public void Send()
    {
        if (Status != QuoteStatus.Draft)
            throw new InvalidOperationException($"Cannot send a quote in '{Status}' status. Must be Draft.");
        Status = QuoteStatus.Sent;
        SentDate = DateTime.UtcNow;
    }

    public void Accept()
    {
        if (Status != QuoteStatus.Sent)
            throw new InvalidOperationException($"Cannot accept a quote in '{Status}' status. Must be Sent.");
        Status = QuoteStatus.Accepted;
    }

    public void Reject()
    {
        if (Status != QuoteStatus.Sent)
            throw new InvalidOperationException($"Cannot reject a quote in '{Status}' status. Must be Sent.");
        Status = QuoteStatus.Rejected;
    }

    public void Expire() => Status = QuoteStatus.Expired;

    public void MarkConverted()
    {
        if (Status != QuoteStatus.Accepted)
            throw new InvalidOperationException($"Cannot convert a quote in '{Status}' status. Must be Accepted.");
        Status = QuoteStatus.Converted;
    }

    public void CreateRevision()
    {
        RevisionNumber++;
        Status = QuoteStatus.Draft;
    }

    // ── Totals ──

    public void UpdateTotals(decimal subTotal, decimal discountAmount, decimal taxAmount,
        decimal docDiscountRate = 0, decimal withholdingTaxRate = 0)
    {
        SubTotal = subTotal;
        DiscountAmount = discountAmount;
        TaxAmount = taxAmount;

        DocumentDiscountRate = docDiscountRate;
        DocumentDiscountAmount = (subTotal - discountAmount) * (docDiscountRate / 100);

        var netAfterDocDiscount = subTotal - discountAmount - DocumentDiscountAmount;
        WithholdingTaxRate = withholdingTaxRate;
        WithholdingTaxAmount = netAfterDocDiscount * (withholdingTaxRate / 100);

        TotalAmount = netAfterDocDiscount + taxAmount - WithholdingTaxAmount;

        // Local currency equivalents
        LocalSubTotal = SubTotal * LocalExchangeRate;
        LocalTaxAmount = TaxAmount * LocalExchangeRate;
        LocalTotalAmount = TotalAmount * LocalExchangeRate;
    }

    public void SetAddresses(string? shippingAddress, string? billingAddress)
    {
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
    }

    public void SetNotes(string? notes) => Notes = notes;
}
