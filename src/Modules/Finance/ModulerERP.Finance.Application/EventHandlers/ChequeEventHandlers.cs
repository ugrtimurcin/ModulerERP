using MediatR;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.Finance.Domain.Events;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.Finance.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.EventHandlers;

public class ChequeCreatedEventHandler : INotificationHandler<ChequeCreatedEvent>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<FiscalPeriod> _fiscalPeriodRepository;
    private readonly IUnitOfWork _unitOfWork;

    private const string PortfolioAccountPrefix = "101";
    private const string PartnerAccountPrefix = "120";

    public ChequeCreatedEventHandler(
        IJournalEntryRepository journalEntryRepository,
        IRepository<Account> accountRepository,
        IRepository<FiscalPeriod> fiscalPeriodRepository,
        IUnitOfWork unitOfWork)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
        _fiscalPeriodRepository = fiscalPeriodRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ChequeCreatedEvent notification, CancellationToken cancellationToken)
    {
        var period = await _fiscalPeriodRepository.FirstOrDefaultAsync(
            p => p.StartDate <= notification.OccurredOn && p.EndDate >= notification.OccurredOn, 
            cancellationToken);

        if (period == null || period.Status != PeriodStatus.Open)
        {
            throw new InvalidOperationException("Cannot create Journal Entry for cheque: no open Fiscal Period.");
        }

        // 101 Checks in Portfolio vs 120 AR or 320 AP
        var allAccounts = await _accountRepository.GetAllAsync(cancellationToken);
        var portfolioAccount = allAccounts.FirstOrDefault(a => a.Code.StartsWith(PortfolioAccountPrefix) && !a.IsHeader);
        var partnerAccount = allAccounts.FirstOrDefault(a => a.Code.StartsWith(PartnerAccountPrefix) && !a.IsHeader);

        if (portfolioAccount == null || partnerAccount == null) return;

        var entryNumber = await _journalEntryRepository.GetNextEntryNumberAsync(cancellationToken);
        
        var je = JournalEntry.Create(
            notification.TenantId,
            entryNumber,
            period.Id,
            notification.OccurredOn,
            Guid.Empty,
            "Cheque",
            notification.ChequeId,
            notification.ChequeNumber,
            $"Initial receipt of Cheque {notification.ChequeNumber}"
        );

        // Debit Portfolio, Credit Partner (AR)
        je.AddLine(portfolioAccount, notification.Amount, 0, $"Cheque {notification.ChequeNumber} received");
        je.AddLine(partnerAccount, 0, notification.Amount, $"Cheque {notification.ChequeNumber} from partner");

        je.Post(Guid.Empty);

        await _journalEntryRepository.AddAsync(je, cancellationToken);
    }
}

public class ChequeStatusUpdatedEventHandler : INotificationHandler<ChequeStatusUpdatedEvent>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<FiscalPeriod> _fiscalPeriodRepository;
    private readonly IUnitOfWork _unitOfWork;

    private const string PortfolioAccountPrefix = "101";
    private const string BankCollectionAccountPrefix = "101.02";
    private const string BankAccountPrefix = "102";

    public ChequeStatusUpdatedEventHandler(
        IJournalEntryRepository journalEntryRepository,
        IRepository<Account> accountRepository,
        IRepository<FiscalPeriod> fiscalPeriodRepository,
        IUnitOfWork unitOfWork)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
        _fiscalPeriodRepository = fiscalPeriodRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ChequeStatusUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // Simple logic for state machine Journal entries
        var period = await _fiscalPeriodRepository.FirstOrDefaultAsync(
            p => p.StartDate <= notification.OccurredOn && p.EndDate >= notification.OccurredOn, 
            cancellationToken);

        if (period == null || period.Status != PeriodStatus.Open) return;

        var allAccounts = await _accountRepository.GetAllAsync(cancellationToken);
        var portfolio = allAccounts.FirstOrDefault(a => a.Code.StartsWith(PortfolioAccountPrefix) && !a.IsHeader);
        var bankCollection = allAccounts.FirstOrDefault(a => a.Code.StartsWith(BankCollectionAccountPrefix) && !a.IsHeader) // Example collection account
                             ?? portfolio; // Fallback
        var bankAccount = allAccounts.FirstOrDefault(a => a.Code.StartsWith(BankAccountPrefix) && !a.IsHeader); // Bank

        if (portfolio == null || bankCollection == null || bankAccount == null) return;

        var entryNumber = await _journalEntryRepository.GetNextEntryNumberAsync(cancellationToken);
        var je = JournalEntry.Create(
            notification.TenantId,
            entryNumber,
            period.Id,
            notification.OccurredOn,
            Guid.Empty,
            "Cheque",
            notification.ChequeId,
            notification.ChequeNumber,
            $"Cheque status changed to {notification.NewStatus}"
        );

        if (notification.OldStatus == ChequeStatus.Portfolio && notification.NewStatus == ChequeStatus.BankCollection)
        {
            // Debit BankCollection, Credit Portfolio
            je.AddLine(bankCollection, notification.Amount, 0, "To Bank Collection");
            je.AddLine(portfolio, 0, notification.Amount, "From Portfolio");
        }
        else if (notification.OldStatus == ChequeStatus.BankCollection && notification.NewStatus == ChequeStatus.Paid)
        {
            // Debit Bank, Credit BankCollection
            je.AddLine(bankAccount, notification.Amount, 0, "Cheque Paid");
            je.AddLine(bankCollection, 0, notification.Amount, "From Bank Collection");
        }
        else
        {
            // Ignore other transitions for MVP
            return;
        }

        je.Post(Guid.Empty);
        await _journalEntryRepository.AddAsync(je, cancellationToken);
    }
}
