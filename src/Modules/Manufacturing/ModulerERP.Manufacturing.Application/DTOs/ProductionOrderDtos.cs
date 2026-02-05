namespace ModulerERP.Manufacturing.Application.DTOs;

// Production Order DTOs
public record ProductionOrderListDto(
    Guid Id,
    string OrderNumber,
    Guid ProductId,
    string? ProductName,
    int Status,
    string StatusName,
    decimal PlannedQuantity,
    decimal ProducedQuantity,
    DateTime? PlannedStartDate,
    DateTime? PlannedEndDate,
    int Priority,
    DateTime CreatedAt);

public record ProductionOrderDetailDto(
    Guid Id,
    string OrderNumber,
    Guid BomId,
    string? BomName,
    Guid ProductId,
    string? ProductName,
    Guid WarehouseId,
    string? WarehouseName,
    int Status,
    string StatusName,
    decimal PlannedQuantity,
    decimal ProducedQuantity,
    DateTime? PlannedStartDate,
    DateTime? PlannedEndDate,
    DateTime? ActualStartDate,
    DateTime? ActualEndDate,
    int Priority,
    string? Notes,
    IEnumerable<ProductionOrderLineDto> Lines,
    DateTime CreatedAt);

public record ProductionOrderLineDto(
    Guid Id,
    Guid ProductId,
    string? ProductName,
    decimal RequiredQuantity,
    decimal IssuedQuantity,
    int Sequence);

public record CreateProductionOrderDto(
    string OrderNumber,
    Guid BomId,
    Guid ProductId,
    decimal PlannedQuantity,
    Guid WarehouseId,
    DateTime? PlannedStartDate = null,
    DateTime? PlannedEndDate = null,
    int Priority = 5,
    string? Notes = null);

public record UpdateProductionOrderDto(
    decimal PlannedQuantity,
    Guid WarehouseId,
    DateTime? PlannedStartDate,
    DateTime? PlannedEndDate,
    int Priority,
    string? Notes);

public record RecordProductionDto(decimal Quantity);
