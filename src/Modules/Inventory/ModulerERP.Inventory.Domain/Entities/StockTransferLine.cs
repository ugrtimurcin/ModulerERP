namespace ModulerERP.Inventory.Domain.Entities;

/// <summary>
/// Stock transfer line items.
/// </summary>
public class StockTransferLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid StockTransferId { get; private set; }
    public Guid ProductId { get; private set; }
    
    /// <summary>Quantity to transfer</summary>
    public decimal Quantity { get; private set; }
    
    /// <summary>Quantity actually received (may differ)</summary>
    public decimal? ReceivedQuantity { get; private set; }
    
    public string? Notes { get; private set; }

    // Navigation
    public StockTransfer? StockTransfer { get; private set; }
    public Product? Product { get; private set; }

    private StockTransferLine() { } // EF Core

    public static StockTransferLine Create(Guid stockTransferId, Guid productId, decimal quantity, string? notes = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        return new StockTransferLine
        {
            StockTransferId = stockTransferId,
            ProductId = productId,
            Quantity = quantity,
            Notes = notes
        };
    }

    public void SetReceivedQuantity(decimal receivedQuantity)
    {
        if (receivedQuantity < 0)
            throw new ArgumentException("Received quantity cannot be negative");
        
        ReceivedQuantity = receivedQuantity;
    }
}
