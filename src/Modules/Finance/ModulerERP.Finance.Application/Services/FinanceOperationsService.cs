using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;

namespace ModulerERP.Finance.Application.Services;

public class FinanceOperationsService : IFinanceOperationsService
{
    private readonly IRepository<FiscalPeriod> _fiscalPeriodRepository; // Use Generic Repo
    private readonly IRepository<Account> _accountRepository; // Use Generic Repo for Account
    private readonly IJournalEntryRepository _journalEntryRepository; // Use Specific Repo Interface
    private readonly IUnitOfWork _unitOfWork; // Needed for SaveChanges if repos don't save immediately

    public FinanceOperationsService(
        IRepository<FiscalPeriod> fiscalPeriodRepository,
        IRepository<Account> accountRepository,
        IJournalEntryRepository journalEntryRepository,
        IUnitOfWork unitOfWork)
    {
        _fiscalPeriodRepository = fiscalPeriodRepository;
        _accountRepository = accountRepository;
        _journalEntryRepository = journalEntryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> CreateReceivableAsync(
        Guid tenantId,
        string invoiceNumber,
        Guid partnerId,
        decimal amount,
        string currencyCode,
        DateTime invoiceDate,
        DateTime dueDate,
        Guid? sourceDocumentId = null,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Get Fiscal Period
            // 1. Get Fiscal Period
            var period = await _fiscalPeriodRepository.FirstOrDefaultAsync(
                p => p.StartDate <= invoiceDate && p.EndDate >= invoiceDate, 
                cancellationToken);
            
            if (period == null)
            {
                return Result.Failure($"No Fiscal Period found for date {invoiceDate:yyyy-MM-dd}. Please generate periods in Finance settings.");
            }

            if (period.Status != PeriodStatus.Open)
            {
                return Result.Failure($"Fiscal Period '{period.Code}' is {period.Status}. Transactions can only be posted to Open periods.");
            }

            // 2. Generate Entry Number
            var entryNumber = await _journalEntryRepository.GetNextEntryNumberAsync(cancellationToken);

            // 3. Create Journal Entry Header
            var je = JournalEntry.Create(
                tenantId,
                entryNumber,
                period.Id,
                invoiceDate,
                Guid.Empty, // CreatedBy (System)
                "SalesInvoice",
                sourceDocumentId,
                invoiceNumber,
                description ?? $"Invoice {invoiceNumber}"
            );

            // 4. Create Lines (Simplest AR Logic)
            // Dr Accounts Receivable (Asset)
            // Cr Sales Revenue (Income)
            // Cr Sales Tax Payable (Liability)
            
            // Need Accounts: 
            // 1. AR Account (From Partner Group or System Default)
            // 2. Sales Account (From Product Group or System Default)
            // 3. Tax Account
            
            // For MVP: We will Look up accounts by code or type. 
            // If not found, create them.
            
            var arAccount = await GetOrCreateAccount(tenantId, "1200", "Accounts Receivable", AccountType.Asset, cancellationToken);
            var salesAccount = await GetOrCreateAccount(tenantId, "4000", "Sales Revenue", AccountType.Revenue, cancellationToken);
            
            // Add AR Line (Debit)
            je.Lines.Add(JournalEntryLine.CreateDebit(
                je.Id,
                arAccount.Id,
                amount,
                1, // Line Number
                description ?? $"Invoice {invoiceNumber}",
                partnerId, // PartnerId
                null, // CostCenter
                null, // Currency
                null, 
                null
            ));

            // Add Sales Line (Credit)
            je.Lines.Add(JournalEntryLine.CreateCredit(
                je.Id,
                salesAccount.Id,
                amount,
                2, // Line Number
                "Sales Revenue",
                null, // PartnerId
                null, // CostCenter
                null, // Currency
                null,
                null
            ));
            
            je.UpdateTotals(amount, amount);
            je.Post(Guid.Empty); // Auto-post for now

            await _journalEntryRepository.AddAsync(je, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Finance Error: {ex.Message}");
        }
    }

    private async Task<Account> GetOrCreateAccount(Guid tenantId, string code, string name, AccountType type, CancellationToken ct)
    {
        var acc = await _accountRepository.FirstOrDefaultAsync(a => a.Code == code, ct);
        if (acc == null)
        {
            acc = Account.Create(tenantId, code, name, type, Guid.Empty);
            await _accountRepository.AddAsync(acc, ct);
            // Save?
        }
        return acc;
    }
}
