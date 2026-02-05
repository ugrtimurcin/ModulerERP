using ModulerERP.Sales.Domain.Enums;

namespace ModulerERP.Sales.Application.DTOs;

public class ShipmentDto
{
    public Guid Id { get; set; }
    public string ShipmentNumber { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty; // Requires mapping
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty; // Requires mapping
    public ShipmentStatus Status { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public string? ShippingAddress { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ShipmentLineDto> Lines { get; set; } = new();
}

public class ShipmentLineDto
{
    public Guid Id { get; set; }
    public Guid OrderLineId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty; // Requires mapping
    public string ProductSku { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? LotNumber { get; set; }
    public string? SerialNumbers { get; set; }
}

public class CreateShipmentDto
{
    public Guid OrderId { get; set; }
    public Guid WarehouseId { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? ShippingAddress { get; set; }
    public string? Notes { get; set; }
    public List<CreateShipmentLineDto> Lines { get; set; } = new();
}

public class CreateShipmentLineDto
{
    public Guid OrderLineId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public string? LotNumber { get; set; }
    public string? SerialNumbers { get; set; }
}

public class UpdateShipmentDto
{
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? ShippingAddress { get; set; }
    public string? Notes { get; set; }
}
