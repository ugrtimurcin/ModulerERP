namespace ModulerERP.SharedKernel.IntegrationEvents;

public record ProjectWarehouseCreatedEvent(
    Guid TenantId,
    Guid ProjectId,
    Guid WarehouseId
) : MediatR.INotification;
