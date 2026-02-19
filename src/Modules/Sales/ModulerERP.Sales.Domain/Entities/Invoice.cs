using ModulerERP.SharedKernel.Entities;
using ModulerERP.Sales.Domain.Enums;

namespace ModulerERP.Sales.Domain.Entities;

/// <summary>
/// Sales invoice for receivables with KKTC dual-currency and Stopaj support.
/// Links to Finance module for ledger entries.
/// </summary>
public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; private set; } = string.Empty;
    public Guid? OrderId { get; private set; }
    public Guid PartnerId { get; private set; }

    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;

    // ── Transaction Currency ──
    public Guid CurrencyId { get; private set; }
    public decimal ExchangeRate { get; private set; } = 1;

    // ── Local Currency (KKTC: TRY) ──
    public Guid? LocalCurrencyId { get; private set; }
    public decimal LocalExchangeRate { get; private set; } = 1;
    public decimal LocalSubTotal { get; private set; }
    public decimal LocalTaxAmount { get; private set; }
    public decimal LocalTotalAmount { get; private set; }

    // ── Dates ──
    public DateTime InvoiceDate { get; private set; }
    public DateTime DueDate { get; private set; }

    // ── Addresses & Terms ──
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

    // ── Payment Tracking ──
    public decimal PaidAmount { get; private set; }
    public decimal BalanceDue => TotalAmount - PaidAmount;

    // Navigation
    public Order? Order { get; private set; }
    public ICollection<InvoiceLine> Lines { get; private set; } = new List<InvoiceLine>();

    private Invoice() { } // EF Core

    public static Invoice Create(
        Guid tenantId,
        string invoiceNumber,
        Guid partnerId,
        Guid currencyId,
        decimal exchangeRate,
        DateTime invoiceDate,
        DateTime dueDate,
        Guid createdByUserId,
        Guid? orderId = null,
        string? paymentTerms = null,
        Guid? localCurrencyId = null,
        decimal localExchangeRate = 1)
    {
        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            PartnerId = partnerId,
            CurrencyId = currencyId,
            ExchangeRate = exchangeRate,
            InvoiceDate = invoiceDate,
            DueDate = dueDate,
            OrderId = orderId,
            PaymentTerms = paymentTerms,
            LocalCurrencyId = localCurrencyId,
            LocalExchangeRate = localExchangeRate
        };

        invoice.SetTenant(tenantId);
        invoice.SetCreator(createdByUserId);
        return invoice;
    }

    // ── Status Transitions (with guards) ──

    public void Issue()
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException($"Cannot issue invoice in '{Status}' status. Must be Draft.");
        Status = InvoiceStatus.Issued;
    }

    public void RecordPayment(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be positive.", nameof(amount));

        PaidAmount += amount;
        if (PaidAmount >= TotalAmount)
            Status = InvoiceStatus.Paid;
        else if (PaidAmount > 0)
            Status = InvoiceStatus.PartiallyPaid;
    }

    public void MarkOverdue()
    {
        if (Status != InvoiceStatus.Issued && Status != InvoiceStatus.PartiallyPaid)
            throw new InvalidOperationException($"Cannot mark invoice as overdue in '{Status}' status.");
        Status = InvoiceStatus.Overdue;
    }

    public void Cancel()
    {
        if (Status != InvoiceStatus.Draft && Status != InvoiceStatus.Issued)
            throw new InvalidOperationException($"Cannot cancel invoice in '{Status}' status. Must be Draft or Issued.");
        Status = InvoiceStatus.Cancelled;
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
