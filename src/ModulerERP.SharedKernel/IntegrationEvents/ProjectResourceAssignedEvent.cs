using MediatR;

namespace ModulerERP.SharedKernel.IntegrationEvents;

public record ProjectResourceAssignedEvent(
    Guid ProjectId,
    string ProjectName, // Useful for setting Location as "Project: {Name}"
    Guid AssetId,
    Guid TenantId,
    DateTime AssignmentDate
) : INotification;
