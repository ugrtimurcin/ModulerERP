using MediatR;
using ModulerERP.SharedKernel.IntegrationEvents;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Domain.Enums;

namespace ModulerERP.ProjectManagement.Infrastructure.Consumers;

public class StockConsumedConsumer : INotificationHandler<StockConsumedEvent>
{
    private readonly IProjectTransactionService _transactionService;

    public StockConsumedConsumer(IProjectTransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public async Task Handle(StockConsumedEvent notification, CancellationToken cancellationToken)
    {
        // 1. Validation: Only process movements linked to a Project
        if (notification.ProjectId.HasValue)
        {
            // 2. Calculate Cost (Quantity * UnitCost) happens in Inventory, passed here via Event
            
            await _transactionService.AddTransactionAsync(notification.TenantId, new CreateProjectTransactionDto(
                notification.ProjectId.Value,
                null, // ProjectTaskId
                "Inventory", // SourceModule
                notification.MovementId, // SourceRecordId
                $"Stock Usage: {notification.ProductName} ({notification.Quantity} {notification.UnitOfMeasure})", // Description
                notification.TotalCost,
                notification.CurrencyId, // Usually Base Currency (e.g., TRY)
                0, // Exchange Rate
                ProjectTransactionType.Material, // It's a material cost
                notification.Date
            ));
        }
    }
}
