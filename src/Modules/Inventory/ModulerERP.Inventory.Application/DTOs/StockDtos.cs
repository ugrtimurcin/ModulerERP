using ModulerERP.Inventory.Domain.Enums;

namespace ModulerERP.Inventory.Application.DTOs;

public class CreateStockMovementDto
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? LocationId { get; set; }
    public MovementType Type { get; set; }
    public decimal Quantity { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceNumber { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Notes { get; set; }
    public DateTime? MovementDate { get; set; }
}

public class CreateStockTransferDto
{
    public Guid SourceWarehouseId { get; set; }
    public Guid DestinationWarehouseId { get; set; }
    public List<StockTransferItemDto> Items { get; set; } = new();
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime? TransferDate { get; set; }
}

public class StockTransferItemDto
{
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
}

public class StockMovementDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public MovementType Type { get; set; }
    public decimal Quantity { get; set; }
    public string? ReferenceType { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime MovementDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class StockLevelDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityOnOrder { get; set; }
    public decimal QuantityAvailable { get; set; }
    public DateTime LastUpdated { get; set; }
}
