using MediatR;
using ModulerERP.FixedAssets.Application.Interfaces;
using ModulerERP.SharedKernel.IntegrationEvents;

namespace ModulerERP.FixedAssets.Application.EventHandlers;

public class AssetTransferConsumer : INotificationHandler<ProjectResourceAssignedEvent>
{
    private readonly IFixedAssetService _assetService;

    public AssetTransferConsumer(IFixedAssetService assetService)
    {
        _assetService = assetService;
    }

    public async Task Handle(ProjectResourceAssignedEvent notification, CancellationToken cancellationToken)
    {
        // Automation: When asset is assigned to a project, update its location and status
        var newLocation = $"Project: {notification.ProjectName}";
        
        await _assetService.UpdateLocationAndStatusAsync(
            notification.TenantId, 
            notification.AssetId, 
            newLocation, 
            "Deployed", // Or "In Use"
            cancellationToken
        );
    }
}
