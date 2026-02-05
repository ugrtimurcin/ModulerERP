using ModulerERP.SharedKernel.Entities;
using ModulerERP.Sales.Domain.Enums;

namespace ModulerERP.Sales.Domain.Entities;

/// <summary>
/// Physical delivery to customer with tracking.
/// </summary>
public class Shipment : BaseEntity
{
    /// <summary>Shipment number (e.g., 'SHP-2026-001')</summary>
    public string ShipmentNumber { get; private set; } = string.Empty;
    
    public Guid OrderId { get; private set; }
    public Guid WarehouseId { get; private set; }
    
    public ShipmentStatus Status { get; private set; } = ShipmentStatus.Pending;
    
    /// <summary>Carrier name (e.g., 'DHL', 'FedEx')</summary>
    public string? Carrier { get; private set; }
    
    /// <summary>Carrier tracking number</summary>
    public string? TrackingNumber { get; private set; }
    
    /// <summary>Estimated delivery date</summary>
    public DateTime? EstimatedDeliveryDate { get; private set; }
    
    /// <summary>Actual ship date</summary>
    public DateTime? ShippedDate { get; private set; }
    
    /// <summary>Actual delivery date</summary>
    public DateTime? DeliveredDate { get; private set; }
    
    /// <summary>Shipping address as JSON</summary>
    public string? ShippingAddress { get; private set; }
    
    public string? Notes { get; private set; }

    // Navigation
    public Order? Order { get; private set; }
    public ICollection<ShipmentLine> Lines { get; private set; } = new List<ShipmentLine>();

    private Shipment() { } // EF Core

    public static Shipment Create(
        Guid tenantId,
        string shipmentNumber,
        Guid orderId,
        Guid warehouseId,
        Guid createdByUserId,
        string? carrier = null,
        string? shippingAddress = null,
        DateTime? estimatedDeliveryDate = null)
    {
        var shipment = new Shipment
        {
            ShipmentNumber = shipmentNumber,
            OrderId = orderId,
            WarehouseId = warehouseId,
            Carrier = carrier,
            ShippingAddress = shippingAddress,
            EstimatedDeliveryDate = estimatedDeliveryDate
        };

        shipment.SetTenant(tenantId);
        shipment.SetCreator(createdByUserId);
        return shipment;
    }

    public void Ship(string? trackingNumber = null)
    {
        Status = ShipmentStatus.Shipped;
        ShippedDate = DateTime.UtcNow;
        TrackingNumber = trackingNumber;
    }

    public void MarkInTransit() => Status = ShipmentStatus.InTransit;

    public void MarkDelivered()
    {
        Status = ShipmentStatus.Delivered;
        DeliveredDate = DateTime.UtcNow;
    }

    public void MarkFailed() => Status = ShipmentStatus.Failed;
}
