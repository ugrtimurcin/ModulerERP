namespace ModulerERP.SharedKernel.IntegrationEvents;

public record ProjectCreatedEvent(
    Guid TenantId,
    Guid ProjectId,
    string ProjectCode,
    string ProjectName
) : MediatR.INotification;
