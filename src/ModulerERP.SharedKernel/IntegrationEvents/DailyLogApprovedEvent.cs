using MediatR;

public record DailyLogApprovedEvent(
    Guid TenantId,
    Guid ProjectId,
    Guid WarehouseId,
    Guid DailyLogId,
    DateTime Date,
    List<DailyLogMaterialUsageItem> MaterialUsages
) : INotification;

public record DailyLogMaterialUsageItem(
    Guid ProductId,
    decimal Quantity,
    Guid UnitOfMeasureId
);
