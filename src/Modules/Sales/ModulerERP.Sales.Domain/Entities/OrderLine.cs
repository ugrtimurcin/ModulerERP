namespace ModulerERP.Sales.Domain.Entities;

/// <summary>
/// Order line items with fulfillment tracking.
/// </summary>
public class OrderLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    
    public int LineNumber { get; private set; }
    public string Description { get; private set; } = string.Empty;
    
    public decimal Quantity { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public decimal UnitPrice { get; private set; }
    
    /// <summary>Quantity already shipped</summary>
    public decimal ShippedQuantity { get; private set; }
    
    /// <summary>Quantity already invoiced</summary>
    public decimal InvoicedQuantity { get; private set; }
    
    public decimal DiscountPercent { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxPercent { get; private set; }
    public decimal LineTotal { get; private set; }
    
    public string? Notes { get; private set; }

    // Navigation
    public Order? Order { get; private set; }

    /// <summary>Remaining quantity to ship</summary>
    public decimal RemainingToShip => Quantity - ShippedQuantity;
    
    /// <summary>Remaining quantity to invoice</summary>
    public decimal RemainingToInvoice => Quantity - InvoicedQuantity;

    private OrderLine() { } // EF Core

    public static OrderLine Create(
        Guid orderId,
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
        var line = new OrderLine
        {
            OrderId = orderId,
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

    public void RecordShipment(decimal shippedQuantity)
    {
        ShippedQuantity += shippedQuantity;
    }

    public void RecordInvoice(decimal invoicedQuantity)
    {
        InvoicedQuantity += invoicedQuantity;
    }

    private void CalculateLineTotal()
    {
        var grossTotal = Quantity * UnitPrice;
        DiscountAmount = grossTotal * (DiscountPercent / 100);
        LineTotal = grossTotal - DiscountAmount;
    }
}
