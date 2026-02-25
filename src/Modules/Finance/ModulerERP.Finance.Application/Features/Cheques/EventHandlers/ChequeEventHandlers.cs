using MediatR;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.Finance.Domain.Events;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Application.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.Cheques.EventHandlers;

public class ChequeCreatedEventHandler : INotificationHandler<ChequeCreatedEvent>
{
    private readonly ILedgerPostingService _ledgerPostingService;

    public ChequeCreatedEventHandler(ILedgerPostingService ledgerPostingService)
    {
        _ledgerPostingService = ledgerPostingService;
    }

    public async Task Handle(ChequeCreatedEvent notification, CancellationToken cancellationToken)
    {
        var request = new LedgerPostRequest
        {
            TenantId = notification.TenantId,
            UserId = Guid.Empty, // System Action
            TransactionType = TransactionType.ChequeDeposit,
            Category = "Initial Receipt", // Specific configuration mapping
            EventDate = notification.OccurredOn,
            SourceType = "Cheque",
            SourceId = notification.ChequeId,
            SourceNumber = notification.ChequeNumber,
            Description = $"Initial receipt of Cheque {notification.ChequeNumber}",
            
            BaseCurrencyId = Guid.Empty,
            TransactionCurrencyId = Guid.Empty,
            ExchangeRate = 1m,

            Amounts = new System.Collections.Generic.List<LedgerPostAmount>
            {
                new LedgerPostAmount
                {
                    Role = PostingAccountRole.ChequePortfolio,
                    IsDebit = true,
                    BaseAmount = notification.Amount,
                    TransactionAmount = notification.Amount,
                    LineDescription = $"Cheque {notification.ChequeNumber} received"
                },
                new LedgerPostAmount
                {
                    Role = PostingAccountRole.AccountsReceivable,
                    IsDebit = false,
                    BaseAmount = notification.Amount,
                    TransactionAmount = notification.Amount,
                    LineDescription = $"Cheque {notification.ChequeNumber} from partner"
                }
            }
        };

        await _ledgerPostingService.PostAsync(request, cancellationToken);
    }
}

public class ChequeStatusUpdatedEventHandler : INotificationHandler<ChequeStatusUpdatedEvent>
{
    private readonly ILedgerPostingService _ledgerPostingService;

    public ChequeStatusUpdatedEventHandler(ILedgerPostingService ledgerPostingService)
    {
        _ledgerPostingService = ledgerPostingService;
    }

    public async Task Handle(ChequeStatusUpdatedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.OldStatus == ChequeStatus.Portfolio && notification.NewStatus == ChequeStatus.BankCollection)
        {
            var request = new LedgerPostRequest
            {
                TenantId = notification.TenantId,
                UserId = Guid.Empty,
                TransactionType = TransactionType.ChequeDeposit,
                Category = "To Collection",
                EventDate = notification.OccurredOn,
                SourceType = "Cheque",
                SourceId = notification.ChequeId,
                SourceNumber = notification.ChequeNumber,
                Description = $"Cheque {notification.ChequeNumber} moved to Bank Collection",
                
                BaseCurrencyId = Guid.Empty,
                TransactionCurrencyId = Guid.Empty,
                ExchangeRate = 1m,

                Amounts = new System.Collections.Generic.List<LedgerPostAmount>
                {
                    new LedgerPostAmount
                    {
                        Role = PostingAccountRole.ChequeInCollection,
                        IsDebit = true,
                        BaseAmount = notification.Amount,
                        TransactionAmount = notification.Amount,
                        LineDescription = "To Bank Collection"
                    },
                    new LedgerPostAmount
                    {
                        Role = PostingAccountRole.ChequePortfolio,
                        IsDebit = false,
                        BaseAmount = notification.Amount,
                        TransactionAmount = notification.Amount,
                        LineDescription = "From Portfolio"
                    }
                }
            };
            await _ledgerPostingService.PostAsync(request, cancellationToken);
        }
        else if (notification.OldStatus == ChequeStatus.BankCollection && notification.NewStatus == ChequeStatus.Paid)
        {
            var request = new LedgerPostRequest
            {
                TenantId = notification.TenantId,
                UserId = Guid.Empty,
                TransactionType = TransactionType.ChequeClearance,
                EventDate = notification.OccurredOn,
                SourceType = "Cheque",
                SourceId = notification.ChequeId,
                SourceNumber = notification.ChequeNumber,
                Description = $"Cheque {notification.ChequeNumber} Paid/Cleared",
                
                BaseCurrencyId = Guid.Empty,
                TransactionCurrencyId = Guid.Empty,
                ExchangeRate = 1m,

                Amounts = new System.Collections.Generic.List<LedgerPostAmount>
                {
                    new LedgerPostAmount
                    {
                        Role = PostingAccountRole.BankCash,
                        IsDebit = true,
                        BaseAmount = notification.Amount,
                        TransactionAmount = notification.Amount,
                        LineDescription = "Cheque Paid"
                    },
                    new LedgerPostAmount
                    {
                        Role = PostingAccountRole.ChequeInCollection,
                        IsDebit = false,
                        BaseAmount = notification.Amount,
                        TransactionAmount = notification.Amount,
                        LineDescription = "From Bank Collection"
                    }
                }
            };
            await _ledgerPostingService.PostAsync(request, cancellationToken);
        }
    }
}
