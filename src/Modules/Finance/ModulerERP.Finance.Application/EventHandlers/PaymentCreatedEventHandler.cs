using MediatR;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.Finance.Domain.Events;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.Finance.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.EventHandlers;

public class PaymentCreatedEventHandler : INotificationHandler<PaymentCreatedEvent>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<FiscalPeriod> _fiscalPeriodRepository;
    private readonly IUnitOfWork _unitOfWork;

    private const string ArAccountPrefix = "120";
    private const string ApAccountPrefix = "320";

    public PaymentCreatedEventHandler(
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

    public async Task Handle(PaymentCreatedEvent notification, CancellationToken cancellationToken)
    {
        var period = await _fiscalPeriodRepository.FirstOrDefaultAsync(
            p => p.StartDate <= notification.OccurredOn && p.EndDate >= notification.OccurredOn, 
            cancellationToken);

        if (period == null || period.Status != PeriodStatus.Open)
        {
            throw new InvalidOperationException("Cannot create Journal Entry for payment: no open Fiscal Period.");
        }

        // For MVP, we need the Payment Bank/Cash account and an opposing account (e.g. AP/AR).
        // Let's fetch the Bank/Cash account.
        var bankAccount = await _accountRepository.GetByIdAsync(notification.AccountId, cancellationToken);
        if (bankAccount == null) return;
        
        // Find AR/AP based on whether it was a receipt or payment. For simplicity, just find a placeholder clearing account.
        var allAccounts = await _accountRepository.GetAllAsync(cancellationToken);
        var clearingAccount = allAccounts.FirstOrDefault(a => a.Code.StartsWith(ArAccountPrefix) && !a.IsHeader) // AR
                              ?? allAccounts.FirstOrDefault(a => a.Code.StartsWith(ApAccountPrefix) && !a.IsHeader); // AP fallback

        if (clearingAccount == null) return;

        var entryNumber = await _journalEntryRepository.GetNextEntryNumberAsync(cancellationToken);
        
        var je = JournalEntry.Create(
            notification.TenantId,
            entryNumber,
            period.Id,
            notification.OccurredOn,
            Guid.Empty,
            "Payment",
            notification.PaymentId,
            notification.PaymentNumber,
            $"Auto-generated Journal Entry for Payment {notification.PaymentNumber}"
        );

        // Simple bookkeeping: Debit clearing, Credit Bank
        // Note: Real world depends on Payment Direction (In/Out)
        je.AddLine(clearingAccount, notification.Amount, 0, $"Clearing for {notification.PaymentNumber}");
        je.AddLine(bankAccount, 0, notification.Amount, $"Bank payout {notification.PaymentNumber}");

        je.Post(Guid.Empty);

        await _journalEntryRepository.AddAsync(je, cancellationToken);
        // Important: Handlers will be running in the same transaction space. 
        // We shouldn't necessarily call SaveChangesAsync here unless MediatR is fire-and-forget or after-commit.
        // Actually, if we're in the same DBContext and using regular MediatR Publish before SaveChanges inside CommandHandler,
        // we might not need to SaveChanges here. 
        // Let's call SaveChangesAsync to be safe if it's considered an atomic step, or we can let the CommandHandler do it.
        // Since the requirement might be "single transaction", letting CommandHandler do it is better if published before SaveChanges.
    }
}
