namespace ModulerERP.Sales.Domain.Entities;

/// <summary>
/// Quote line items with snapshot pricing.
/// </summary>
public class QuoteLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid QuoteId { get; private set; }
    public Guid ProductId { get; private set; }
    
    /// <summary>Display order</summary>
    public int LineNumber { get; private set; }
    
    /// <summary>Product description at quote time (snapshot)</summary>
    public string Description { get; private set; } = string.Empty;
    
    public decimal Quantity { get; private set; }
    
    /// <summary>Unit of measure at quote time</summary>
    public Guid UnitOfMeasureId { get; private set; }
    
    /// <summary>Frozen unit price</summary>
    public decimal UnitPrice { get; private set; }
    
    /// <summary>Line discount percentage</summary>
    public decimal DiscountPercent { get; private set; }
    
    /// <summary>Line discount amount</summary>
    public decimal DiscountAmount { get; private set; }
    
    /// <summary>Tax percentage for this line</summary>
    public decimal TaxPercent { get; private set; }
    
    /// <summary>Line total before tax</summary>
    public decimal LineTotal { get; private set; }
    
    public string? Notes { get; private set; }

    // Navigation
    public Quote? Quote { get; private set; }

    private QuoteLine() { } // EF Core

    public static QuoteLine Create(
        Guid quoteId,
        Guid productId,
        string description,
        decimal quantity,
        Guid unitOfMeasureId,
        decimal unitPrice,
        int lineNumber,
        decimal discountPercent = 0,
        decimal taxPercent = 0,
        string? notes = null)
    {
        var line = new QuoteLine
        {
            QuoteId = quoteId,
            ProductId = productId,
            Description = description,
            Quantity = quantity,
            UnitOfMeasureId = unitOfMeasureId,
            UnitPrice = unitPrice,
            LineNumber = lineNumber,
            DiscountPercent = discountPercent,
            TaxPercent = taxPercent,
            Notes = notes
        };

        line.CalculateLineTotal();
        return line;
    }

    public void Update(decimal quantity, decimal unitPrice, decimal discountPercent, decimal taxPercent)
    {
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountPercent = discountPercent;
        TaxPercent = taxPercent;
        CalculateLineTotal();
    }

    private void CalculateLineTotal()
    {
        var grossTotal = Quantity * UnitPrice;
        DiscountAmount = grossTotal * (DiscountPercent / 100);
        LineTotal = grossTotal - DiscountAmount;
    }
}
