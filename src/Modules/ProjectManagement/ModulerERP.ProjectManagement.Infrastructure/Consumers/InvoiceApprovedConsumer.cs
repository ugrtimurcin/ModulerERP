using MediatR;
using ModulerERP.SharedKernel.IntegrationEvents; 
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Domain.Enums;

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
        // 1. Validation: Only process if a ProjectId was selected on the Invoice
        if (notification.ProjectId.HasValue)
        {
            // 2. Create the Project Expense
            await _transactionService.AddTransactionAsync(notification.TenantId, new CreateProjectTransactionDto(
                notification.ProjectId.Value,
                null, // ProjectTaskId
                "Procurement", // SourceModule
                notification.InvoiceId, // SourceRecordId
                $"Invoice #{notification.InvoiceNumber} - Supplier: {notification.SupplierName}", // Description
                notification.Amount,
                notification.CurrencyId,
                0, // Exchange Rate 0 to let service fetch it
                ProjectTransactionType.Expense,
                notification.Date
            ));
        }
    }
}
