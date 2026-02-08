using MediatR;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.ProjectManagement.Domain.Enums;
using ModulerERP.SharedKernel.IntegrationEvents;

namespace ModulerERP.ProjectManagement.Infrastructure.Consumers;

public class InvoiceApprovedConsumer : INotificationHandler<InvoiceApprovedEvent>
{
    private readonly IProjectTransactionService _transactionService;

    public InvoiceApprovedConsumer(IProjectTransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public async Task Handle(InvoiceApprovedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.ProjectId.HasValue)
        {
            var dto = new CreateProjectTransactionDto(
                notification.ProjectId.Value,
                null, // TaskId
                "Invoice", // SourceModule
                notification.InvoiceId, // SourceRecordId
                $"Invoice Approved", // Description
                notification.Amount,
                notification.CurrencyId,
                1.0m, // TODO: Implement Exchange Rate Service
                ProjectTransactionType.GeneralExpense
            );

            // Using Guid.Empty for UserId as this is a system action
            await _transactionService.CreateAsync(notification.TenantId, Guid.Empty, dto);
        }
    }
}
