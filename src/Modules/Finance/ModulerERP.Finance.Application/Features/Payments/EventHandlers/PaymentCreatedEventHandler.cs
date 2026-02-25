using MediatR;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.Finance.Domain.Events;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Application.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.Payments.EventHandlers;

public class PaymentCreatedEventHandler : INotificationHandler<PaymentCreatedEvent>
{
    private readonly ILedgerPostingService _ledgerPostingService;

    public PaymentCreatedEventHandler(ILedgerPostingService ledgerPostingService)
    {
        _ledgerPostingService = ledgerPostingService;
    }

    public async Task Handle(PaymentCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Note: For MVP we assume this is an Incoming Payment. 
        // In reality, the event should provide PaymentDirection (In/Out) to determine TransactionType.
        var request = new LedgerPostRequest
        {
            TenantId = notification.TenantId,
            UserId = System.Guid.Empty, // System Action
            TransactionType = TransactionType.IncomingPayment, 
            EventDate = notification.OccurredOn,
            SourceType = "Payment",
            SourceId = notification.PaymentId,
            SourceNumber = notification.PaymentNumber,
            Description = $"Auto-generated Journal Entry for Payment {notification.PaymentNumber}",
            
            BaseCurrencyId = System.Guid.Empty,
            TransactionCurrencyId = System.Guid.Empty,
            ExchangeRate = 1m, // Note: Phase 3 will inject daily Kur Farkı calculations here

            Amounts = new System.Collections.Generic.List<LedgerPostAmount>
            {
                new LedgerPostAmount
                {
                    Role = PostingAccountRole.BankCash,
                    IsDebit = true,
                    BaseAmount = notification.Amount,
                    TransactionAmount = notification.Amount,
                    LineDescription = $"Bank receipt {notification.PaymentNumber}"
                },
                new LedgerPostAmount
                {
                    Role = PostingAccountRole.AccountsReceivable,
                    IsDebit = false,
                    BaseAmount = notification.Amount,
                    TransactionAmount = notification.Amount,
                    LineDescription = $"Clearing for {notification.PaymentNumber}"
                }
            }
        };

        await _ledgerPostingService.PostAsync(request, cancellationToken);
    }
}
