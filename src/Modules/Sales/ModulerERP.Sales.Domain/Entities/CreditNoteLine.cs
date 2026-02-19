namespace ModulerERP.Sales.Domain.Entities;

/// <summary>
/// Credit note line item — reverses a specific invoice line.
/// </summary>
public class CreditNoteLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CreditNoteId { get; private set; }
    public Guid? InvoiceLineId { get; private set; }
    public Guid ProductId { get; private set; }

    public int LineNumber { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxPercent { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal LineTotal { get; private set; }

    // Navigation
    public CreditNote? CreditNote { get; private set; }

    private CreditNoteLine() { } // EF Core

    public static CreditNoteLine Create(
        Guid creditNoteId,
        Guid productId,
        string description,
        decimal quantity,
        Guid unitOfMeasureId,
        decimal unitPrice,
        int lineNumber,
        decimal discountPercent = 0,
        decimal taxPercent = 0,
        Guid? invoiceLineId = null)
    {
        var line = new CreditNoteLine
        {
            CreditNoteId = creditNoteId,
            ProductId = productId,
            Description = description,
            Quantity = quantity,
            UnitOfMeasureId = unitOfMeasureId,
            UnitPrice = unitPrice,
            LineNumber = lineNumber,
            DiscountPercent = discountPercent,
            TaxPercent = taxPercent,
            InvoiceLineId = invoiceLineId
        };

        line.CalculateTotals();
        return line;
    }

    private void CalculateTotals()
    {
        var grossTotal = Quantity * UnitPrice;
        DiscountAmount = grossTotal * (DiscountPercent / 100);
        var netAmount = grossTotal - DiscountAmount;
        TaxAmount = netAmount * (TaxPercent / 100);
        LineTotal = netAmount;
    }
}
