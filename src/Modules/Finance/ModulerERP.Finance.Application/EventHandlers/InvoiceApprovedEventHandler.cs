using MediatR;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.IntegrationEvents;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.Finance.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace ModulerERP.Finance.Application.EventHandlers;

public class InvoiceApprovedEventHandler : INotificationHandler<InvoiceApprovedEvent>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<FiscalPeriod> _fiscalPeriodRepository;
    private readonly IUnitOfWork _unitOfWork;

    private const string ExpenseAccountPrefix = "770";
    private const string ApAccountPrefix = "320";

    public InvoiceApprovedEventHandler(
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

    public async Task Handle(InvoiceApprovedEvent notification, CancellationToken cancellationToken)
    {
        // Find Open Period
        var period = await _fiscalPeriodRepository.FirstOrDefaultAsync(
            p => p.StartDate <= notification.Date && p.EndDate >= notification.Date, 
            cancellationToken);

        if (period == null || period.Status != PeriodStatus.Open)
        {
            // Note: In an event-driven system, failing to handle an event might requeue it or put it in a dead-letter queue.
            // For MVP, we throw an exception to fail the transaction if it's in the same process, or log it if out of process.
            throw new InvalidOperationException("Cannot create Journal Entry: no open Fiscal Period for the invoice date.");
        }

        // Find Expense and AP Accounts (Hardcoded or fetched by some config for now)
        // Assume 770 is Expense and 320 is AP
        var allAccounts = await _accountRepository.GetAllAsync(cancellationToken);
        var expenseAccount = allAccounts.FirstOrDefault(a => a.Code.StartsWith(ExpenseAccountPrefix) && !a.IsHeader);
        var apAccount = allAccounts.FirstOrDefault(a => a.Code.StartsWith(ApAccountPrefix) && !a.IsHeader);

        if (expenseAccount == null || apAccount == null)
        {
             // Missing configuration
             return; // Or throw depending on strictness
        }

        var entryNumber = await _journalEntryRepository.GetNextEntryNumberAsync(cancellationToken);
        
        var je = JournalEntry.Create(
            notification.TenantId,
            entryNumber,
            period.Id,
            notification.Date,
            Guid.Empty, // System User
            "Invoice",
            notification.InvoiceId,
            notification.InvoiceNumber,
            $"Supplier Invoice: {notification.SupplierName}"
        );

        // Debit Expense, Credit AP
        je.AddLine(expenseAccount, notification.Amount, 0, $"Expense for Invoice {notification.InvoiceNumber}");
        je.AddLine(apAccount, 0, notification.Amount, $"AP for {notification.SupplierName}");

        // Auto-post since it's an approved invoice
        je.Post(Guid.Empty);

        await _journalEntryRepository.AddAsync(je, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
