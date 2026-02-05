namespace ModulerERP.Sales.Domain.Entities;

/// <summary>
/// Sales return line items.
/// </summary>
public class SalesReturnLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid SalesReturnId { get; private set; }
    public Guid? InvoiceLineId { get; private set; }
    public Guid ProductId { get; private set; }
    
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }
    
    /// <summary>Reason for returning this item</summary>
    public string? Reason { get; private set; }

    // Navigation
    public SalesReturn? SalesReturn { get; private set; }

    private SalesReturnLine() { } // EF Core

    public static SalesReturnLine Create(
        Guid salesReturnId,
        Guid productId,
        string description,
        decimal quantity,
        decimal unitPrice,
        Guid? invoiceLineId = null,
        string? reason = null)
    {
        return new SalesReturnLine
        {
            SalesReturnId = salesReturnId,
            ProductId = productId,
            Description = description,
            Quantity = quantity,
            UnitPrice = unitPrice,
            LineTotal = quantity * unitPrice,
            InvoiceLineId = invoiceLineId,
            Reason = reason
        };
    }
}
