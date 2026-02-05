namespace ModulerERP.Procurement.Domain.Entities;

/// <summary>
/// Goods receipt line items with quality inspection support.
/// </summary>
public class GoodsReceiptLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid GoodsReceiptId { get; private set; }
    public Guid PurchaseOrderLineId { get; private set; }
    public Guid ProductId { get; private set; }
    
    /// <summary>Quantity received</summary>
    public decimal ReceivedQuantity { get; private set; }
    
    /// <summary>Quantity accepted after QC</summary>
    public decimal AcceptedQuantity { get; private set; }
    
    /// <summary>Quantity rejected after QC</summary>
    public decimal RejectedQuantity { get; private set; }
    
    /// <summary>Lot/Batch number assigned</summary>
    public string? LotNumber { get; private set; }
    
    /// <summary>Expiry date for perishables</summary>
    public DateTime? ExpiryDate { get; private set; }
    
    /// <summary>Warehouse location placed</summary>
    public Guid? LocationId { get; private set; }
    
    public string? Notes { get; private set; }

    // Navigation
    public GoodsReceipt? GoodsReceipt { get; private set; }

    private GoodsReceiptLine() { } // EF Core

    public static GoodsReceiptLine Create(
        Guid goodsReceiptId,
        Guid purchaseOrderLineId,
        Guid productId,
        decimal receivedQuantity,
        string? lotNumber = null,
        DateTime? expiryDate = null,
        Guid? locationId = null)
    {
        return new GoodsReceiptLine
        {
            GoodsReceiptId = goodsReceiptId,
            PurchaseOrderLineId = purchaseOrderLineId,
            ProductId = productId,
            ReceivedQuantity = receivedQuantity,
            AcceptedQuantity = receivedQuantity, // Default to all accepted
            LotNumber = lotNumber,
            ExpiryDate = expiryDate,
            LocationId = locationId
        };
    }

    public void RecordQualityCheck(decimal accepted, decimal rejected, string? notes = null)
    {
        AcceptedQuantity = accepted;
        RejectedQuantity = rejected;
        Notes = notes;
    }
}
