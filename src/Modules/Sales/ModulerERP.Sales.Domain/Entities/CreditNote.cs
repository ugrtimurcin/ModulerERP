using ModulerERP.SharedKernel.Entities;
using ModulerERP.Sales.Domain.Enums;

namespace ModulerERP.Sales.Domain.Entities;

/// <summary>
/// Credit note issued against an invoice (İade Faturası).
/// Required for KKTC — creates a legal reversal document linked to Finance.
/// </summary>
public class CreditNote : BaseEntity
{
    public string CreditNoteNumber { get; private set; } = string.Empty;
    public Guid InvoiceId { get; private set; }
    public Guid? SalesReturnId { get; private set; }
    public Guid PartnerId { get; private set; }

    public CreditNoteStatus Status { get; private set; } = CreditNoteStatus.Draft;

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
    public DateTime CreditNoteDate { get; private set; }

    // ── Totals ──
    public decimal SubTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }

    public string? Notes { get; private set; }

    // Navigation
    public Invoice? Invoice { get; private set; }
    public SalesReturn? SalesReturn { get; private set; }
    public ICollection<CreditNoteLine> Lines { get; private set; } = new List<CreditNoteLine>();

    private CreditNote() { } // EF Core

    public static CreditNote Create(
        Guid tenantId,
        string creditNoteNumber,
        Guid invoiceId,
        Guid partnerId,
        Guid currencyId,
        decimal exchangeRate,
        DateTime creditNoteDate,
        Guid createdByUserId,
        Guid? salesReturnId = null,
        Guid? localCurrencyId = null,
        decimal localExchangeRate = 1)
    {
        var cn = new CreditNote
        {
            CreditNoteNumber = creditNoteNumber,
            InvoiceId = invoiceId,
            PartnerId = partnerId,
            CurrencyId = currencyId,
            ExchangeRate = exchangeRate,
            CreditNoteDate = creditNoteDate,
            SalesReturnId = salesReturnId,
            LocalCurrencyId = localCurrencyId,
            LocalExchangeRate = localExchangeRate
        };

        cn.SetTenant(tenantId);
        cn.SetCreator(createdByUserId);
        return cn;
    }

    // ── Status Transitions ──

    public void Issue()
    {
        if (Status != CreditNoteStatus.Draft)
            throw new InvalidOperationException($"Cannot issue credit note in '{Status}' status. Must be Draft.");
        Status = CreditNoteStatus.Issued;
    }

    public void Cancel()
    {
        if (Status != CreditNoteStatus.Draft && Status != CreditNoteStatus.Issued)
            throw new InvalidOperationException($"Cannot cancel credit note in '{Status}' status.");
        Status = CreditNoteStatus.Cancelled;
    }

    // ── Totals ──

    public void UpdateTotals(decimal subTotal, decimal discountAmount, decimal taxAmount)
    {
        SubTotal = subTotal;
        DiscountAmount = discountAmount;
        TaxAmount = taxAmount;
        TotalAmount = subTotal - discountAmount + taxAmount;

        LocalSubTotal = SubTotal * LocalExchangeRate;
        LocalTaxAmount = TaxAmount * LocalExchangeRate;
        LocalTotalAmount = TotalAmount * LocalExchangeRate;
    }

    public void SetNotes(string? notes) => Notes = notes;
}
