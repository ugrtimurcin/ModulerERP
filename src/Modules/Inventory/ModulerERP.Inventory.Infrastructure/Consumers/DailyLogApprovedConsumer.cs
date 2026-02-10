using MediatR;
using ModulerERP.Inventory.Application.Interfaces;
using ModulerERP.SharedKernel.IntegrationEvents;
using ModulerERP.Inventory.Domain.Enums; // For MovementType

namespace ModulerERP.Inventory.Infrastructure.Consumers;

public class DailyLogApprovedConsumer : INotificationHandler<DailyLogApprovedEvent>
{
    private readonly IStockService _stockService;

    public DailyLogApprovedConsumer(IStockService stockService)
    {
        _stockService = stockService;
    }

    public async Task Handle(DailyLogApprovedEvent message, CancellationToken cancellationToken)
    {
        // WarehouseId is provided in the event
        if (message.WarehouseId == Guid.Empty)
        {
             // Log warning: No warehouse linked
             return; 
        }

        foreach (var item in message.MaterialUsages)
        {
            var movementDto = new ModulerERP.Inventory.Application.DTOs.CreateStockMovementDto
            {
                WarehouseId = message.WarehouseId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Type = MovementType.Consumption, // Correct Enum Usage
                Notes = $"Daily Log {message.Date:yyyy-MM-dd} Usage",
                ReferenceId = message.DailyLogId,
                ReferenceType = "DailyLog",
                MovementDate = DateTime.UtcNow
            };

            await _stockService.ProcessMovementAsync(
                movementDto,
                message.TenantId,
                Guid.Empty, // System User
                cancellationToken
            );
        }
    }
}
