using MediatR;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.IntegrationEvents;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Application.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.JournalEntries.EventHandlers;

public class InvoiceApprovedEventHandler : INotificationHandler<InvoiceApprovedEvent>
{
    private readonly ILedgerPostingService _ledgerPostingService;

    public InvoiceApprovedEventHandler(ILedgerPostingService ledgerPostingService)
    {
        _ledgerPostingService = ledgerPostingService;
    }

    public async Task Handle(InvoiceApprovedEvent notification, CancellationToken cancellationToken)
    {
        var request = new LedgerPostRequest
        {
            TenantId = notification.TenantId,
            UserId = Guid.Empty, // System event
            TransactionType = TransactionType.PurchaseInvoice,
            EventDate = notification.Date,
            SourceType = "Invoice",
            SourceId = notification.InvoiceId,
            SourceNumber = notification.InvoiceNumber,
            Description = $"Supplier Invoice: {notification.SupplierName}",
            
            // Note: Standard MVP assumes Base Currency == Transaction Currency for this handler.
            // A more mature pipeline would derive this from the event.
            BaseCurrencyId = Guid.Empty, 
            TransactionCurrencyId = Guid.Empty,
            ExchangeRate = 1m,
            // PartnerId = notification.SupplierId, // Assuming the event had a SupplierId

            Amounts = new System.Collections.Generic.List<LedgerPostAmount>
            {
                new LedgerPostAmount
                {
                    Role = PostingAccountRole.CostOfGoodsSold, // Or "Expense". This binds via the PostingProfile to 770/153.
                    IsDebit = true,
                    BaseAmount = notification.Amount,
                    TransactionAmount = notification.Amount,
                    LineDescription = $"Expense for {notification.InvoiceNumber}"
                },
                new LedgerPostAmount
                {
                    Role = PostingAccountRole.AccountsPayable, // This binds via the PostingProfile to 320.
                    IsDebit = false,
                    BaseAmount = notification.Amount,
                    TransactionAmount = notification.Amount,
                    LineDescription = $"AP for {notification.SupplierName}"
                }
            }
        };

        await _ledgerPostingService.PostAsync(request, cancellationToken);
    }
}
