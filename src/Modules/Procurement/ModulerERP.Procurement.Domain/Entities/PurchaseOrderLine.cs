namespace ModulerERP.Procurement.Domain.Entities;

/// <summary>
/// Purchase order line items.
/// </summary>
public class PurchaseOrderLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PurchaseOrderId { get; private set; }
    public Guid? RequisitionLineId { get; private set; }
    public Guid ProductId { get; private set; }
    
    public int LineNumber { get; private set; }
    public string Description { get; private set; } = string.Empty;
    
    public decimal Quantity { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public decimal UnitPrice { get; private set; }
    
    /// <summary>Quantity already received</summary>
    public decimal ReceivedQuantity { get; private set; }
    
    /// <summary>Quantity already billed</summary>
    public decimal BilledQuantity { get; private set; }
    
    public decimal TaxPercent { get; private set; }
    public decimal LineTotal { get; private set; }
    
    public string? Notes { get; private set; }

    // Navigation
    public PurchaseOrder? PurchaseOrder { get; private set; }

    public decimal RemainingToReceive => Quantity - ReceivedQuantity;
    public decimal RemainingToBill => Quantity - BilledQuantity;

    private PurchaseOrderLine() { } // EF Core

    public static PurchaseOrderLine Create(
        Guid purchaseOrderId,
        Guid productId,
        string description,
        decimal quantity,
        Guid unitOfMeasureId,
        decimal unitPrice,
        int lineNumber,
        decimal taxPercent = 0,
        Guid? requisitionLineId = null,
        string? notes = null)
    {
        var line = new PurchaseOrderLine
        {
            PurchaseOrderId = purchaseOrderId,
            ProductId = productId,
            Description = description,
            Quantity = quantity,
            UnitOfMeasureId = unitOfMeasureId,
            UnitPrice = unitPrice,
            LineNumber = lineNumber,
            TaxPercent = taxPercent,
            RequisitionLineId = requisitionLineId,
            Notes = notes,
            LineTotal = quantity * unitPrice
        };

        return line;
    }

    public void RecordReceipt(decimal receivedQuantity) => ReceivedQuantity += receivedQuantity;
    public void RecordBill(decimal billedQuantity) => BilledQuantity += billedQuantity;
}
