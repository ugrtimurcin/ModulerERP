using ModulerERP.SharedKernel.Entities;
using ModulerERP.Sales.Domain.Enums;

namespace ModulerERP.Sales.Domain.Entities;

/// <summary>
/// Sales invoice for receivables.
/// Links to Finance module for ledger entries.
/// </summary>
public class Invoice : BaseEntity
{
    /// <summary>Invoice number (e.g., 'INV-2026-001')</summary>
    public string InvoiceNumber { get; private set; } = string.Empty;
    
    public Guid? OrderId { get; private set; }
    public Guid PartnerId { get; private set; }
    
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;
    
    public Guid CurrencyId { get; private set; }
    public decimal ExchangeRate { get; private set; } = 1;
    
    /// <summary>Invoice issue date</summary>
    public DateTime InvoiceDate { get; private set; }
    
    /// <summary>Payment due date</summary>
    public DateTime DueDate { get; private set; }
    
    public string? ShippingAddress { get; private set; }
    public string? BillingAddress { get; private set; }
    
    public string? PaymentTerms { get; private set; }
    public string? Notes { get; private set; }
    
    public decimal SubTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    
    /// <summary>Amount already paid</summary>
    public decimal PaidAmount { get; private set; }
    
    /// <summary>Remaining balance</summary>
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
        string? paymentTerms = null)
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
            PaymentTerms = paymentTerms
        };

        invoice.SetTenant(tenantId);
        invoice.SetCreator(createdByUserId);
        return invoice;
    }

    public void Issue()
    {
        Status = InvoiceStatus.Issued;
    }

    public void RecordPayment(decimal amount)
    {
        PaidAmount += amount;
        if (PaidAmount >= TotalAmount)
            Status = InvoiceStatus.Paid;
        else if (PaidAmount > 0)
            Status = InvoiceStatus.PartiallyPaid;
    }

    public void MarkOverdue() => Status = InvoiceStatus.Overdue;
    public void Cancel() => Status = InvoiceStatus.Cancelled;

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
