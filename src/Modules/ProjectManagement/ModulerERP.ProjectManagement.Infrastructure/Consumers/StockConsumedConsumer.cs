using MediatR;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.ProjectManagement.Domain.Enums;
using ModulerERP.SharedKernel.IntegrationEvents;

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
        if (notification.ProjectId.HasValue)
        {
            var amount = notification.Quantity * notification.CostPrice;

            var dto = new CreateProjectTransactionDto(
                notification.ProjectId.Value,
                null, // TaskId
                "Stock", // SourceModule
                notification.MovementId, // SourceRecordId
                $"Stock Consumed: {notification.Quantity} units", // Description
                amount,
                notification.CurrencyId,
                1.0m, // TODO: Implement Exchange Rate Service
                ProjectTransactionType.Material
            );

            // Using Guid.Empty for UserId as this is a system action
            await _transactionService.CreateAsync(notification.TenantId, Guid.Empty, dto);
        }
    }
}
