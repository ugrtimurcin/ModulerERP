namespace ModulerERP.Sales.Domain.Entities;

/// <summary>
/// Invoice line items.
/// </summary>
public class InvoiceLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid InvoiceId { get; private set; }
    public Guid? OrderLineId { get; private set; }
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
    public Guid? TaxRuleId { get; private set; }
    public decimal LineTotal { get; private set; }
    
    public string? Notes { get; private set; }

    // Navigation
    public Invoice? Invoice { get; private set; }

    private InvoiceLine() { } // EF Core

    public static InvoiceLine Create(
        Guid invoiceId,
        Guid productId,
        string description,
        decimal quantity,
        Guid unitOfMeasureId,
        decimal unitPrice,
        int lineNumber,
        decimal discountPercent = 0,
        decimal taxPercent = 0,
        Guid? orderLineId = null,
        string? notes = null)
    {
        var line = new InvoiceLine
        {
            InvoiceId = invoiceId,
            ProductId = productId,
            Description = description,
            Quantity = quantity,
            UnitOfMeasureId = unitOfMeasureId,
            UnitPrice = unitPrice,
            LineNumber = lineNumber,
            DiscountPercent = discountPercent,
            TaxPercent = taxPercent,
            OrderLineId = orderLineId,
            Notes = notes
        };

        line.CalculateTotals();
        return line;
    }

    private void CalculateTotals()
    {
        var grossTotal = Quantity * UnitPrice;
        DiscountAmount = grossTotal * (DiscountPercent / 100);
        var netTotal = grossTotal - DiscountAmount;
        TaxAmount = netTotal * (TaxPercent / 100);
        LineTotal = netTotal + TaxAmount;
    }
}
