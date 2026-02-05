namespace ModulerERP.Sales.Domain.Entities;

/// <summary>
/// Shipment line items.
/// </summary>
public class ShipmentLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ShipmentId { get; private set; }
    public Guid OrderLineId { get; private set; }
    public Guid ProductId { get; private set; }
    
    /// <summary>Quantity shipped</summary>
    public decimal Quantity { get; private set; }
    
    /// <summary>Lot/Batch number if applicable</summary>
    public string? LotNumber { get; private set; }
    
    /// <summary>Serial numbers as JSON array</summary>
    public string? SerialNumbers { get; private set; }

    // Navigation
    public Shipment? Shipment { get; private set; }

    private ShipmentLine() { } // EF Core

    public static ShipmentLine Create(
        Guid shipmentId,
        Guid orderLineId,
        Guid productId,
        decimal quantity,
        string? lotNumber = null,
        string? serialNumbers = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        return new ShipmentLine
        {
            ShipmentId = shipmentId,
            OrderLineId = orderLineId,
            ProductId = productId,
            Quantity = quantity,
            LotNumber = lotNumber,
            SerialNumbers = serialNumbers
        };
    }
}
