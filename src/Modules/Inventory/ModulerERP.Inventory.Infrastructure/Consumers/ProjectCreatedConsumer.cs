using MediatR;
using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.Inventory.Application.Interfaces;
using ModulerERP.SharedKernel.IntegrationEvents;

namespace ModulerERP.Inventory.Infrastructure.Consumers;

public class ProjectCreatedConsumer : INotificationHandler<ProjectCreatedEvent>
{
    private readonly IWarehouseService _warehouseService;
    private readonly IPublisher _publisher;

    public ProjectCreatedConsumer(IWarehouseService warehouseService, IPublisher publisher)
    {
        _warehouseService = warehouseService;
        _publisher = publisher;
    }

    public async Task Handle(ProjectCreatedEvent message, CancellationToken cancellationToken)
    {
        // Check if warehouse already exists (idempotency check by name/project code if possible, or just create)
        // For now, we assume it's a new project.
        
        var createDto = new CreateWarehouseDto
        {
            Code = $"SITE-{message.ProjectCode}",
            Name = $"Site - {message.ProjectName}",
            Description = $"Virtual Warehouse for Project {message.ProjectCode}",
            Address = "Project Site",
            IsDefault = false,
            BranchId = null
        };

        try
        {
            // Use Guid.Empty for system user
            var warehouse = await _warehouseService.CreateAsync(createDto, message.TenantId, Guid.Empty, cancellationToken);

            // Publish event back to Project Module to link the warehouse
            await _publisher.Publish(new ProjectWarehouseCreatedEvent(
                message.TenantId,
                message.ProjectId,
                warehouse.Id
            ), cancellationToken);
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Failed to create warehouse for project {message.ProjectCode}: {ex.Message}");
            throw; // Retry?
        }
    }
}
