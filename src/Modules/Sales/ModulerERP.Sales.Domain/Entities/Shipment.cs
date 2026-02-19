using ModulerERP.SharedKernel.Entities;
using ModulerERP.Sales.Domain.Enums;

namespace ModulerERP.Sales.Domain.Entities;

/// <summary>
/// Physical delivery with İrsaliye (waybill) support for KKTC compliance.
/// </summary>
public class Shipment : BaseEntity
{
    public string ShipmentNumber { get; private set; } = string.Empty;

    public Guid OrderId { get; private set; }
    public Guid WarehouseId { get; private set; }

    public ShipmentStatus Status { get; private set; } = ShipmentStatus.Pending;

    // ── Carrier & Tracking ──
    public string? Carrier { get; private set; }
    public string? TrackingNumber { get; private set; }

    // ── Dates ──
    public DateTime? EstimatedDeliveryDate { get; private set; }
    public DateTime? ShippedDate { get; private set; }
    public DateTime? DeliveredDate { get; private set; }

    // ── İrsaliye (Waybill) — KKTC Legal ──
    public string? WaybillNumber { get; private set; }
    public string? DriverName { get; private set; }
    public string? VehiclePlate { get; private set; }
    public DateTime? DispatchDateTime { get; private set; }

    // ── Address ──
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

    // ── İrsaliye Assignment ──

    public void SetWaybillInfo(string waybillNumber, string? driverName, string? vehiclePlate, DateTime? dispatchDateTime)
    {
        WaybillNumber = waybillNumber;
        DriverName = driverName;
        VehiclePlate = vehiclePlate;
        DispatchDateTime = dispatchDateTime;
    }

    // ── Status Transitions (with guards) ──

    public void Ship(string? trackingNumber = null)
    {
        if (Status != ShipmentStatus.Pending)
            throw new InvalidOperationException($"Cannot ship in '{Status}' status. Must be Pending.");
        Status = ShipmentStatus.Shipped;
        ShippedDate = DateTime.UtcNow;
        TrackingNumber = trackingNumber;
    }

    public void MarkInTransit()
    {
        if (Status != ShipmentStatus.Shipped)
            throw new InvalidOperationException($"Cannot mark in-transit in '{Status}' status. Must be Shipped.");
        Status = ShipmentStatus.InTransit;
    }

    public void MarkDelivered()
    {
        if (Status != ShipmentStatus.Shipped && Status != ShipmentStatus.InTransit)
            throw new InvalidOperationException($"Cannot mark delivered in '{Status}' status.");
        Status = ShipmentStatus.Delivered;
        DeliveredDate = DateTime.UtcNow;
    }

    public void MarkFailed() => Status = ShipmentStatus.Failed;

    public void SetNotes(string? notes) => Notes = notes;
}
