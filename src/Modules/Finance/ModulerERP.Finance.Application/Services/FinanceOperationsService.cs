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
    private readonly ICurrentUserService _currentUserService;

    public FinanceOperationsService(
        IRepository<FiscalPeriod> fiscalPeriodRepository,
        IRepository<Account> accountRepository,
        IJournalEntryRepository journalEntryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _fiscalPeriodRepository = fiscalPeriodRepository;
        _accountRepository = accountRepository;
        _journalEntryRepository = journalEntryRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
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
                _currentUserService.UserId, // CreatedBy (System or User)
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
            
            var arAccount = await GetAccount(tenantId, "1200", ct: cancellationToken);
            if (arAccount == null) return Result.Failure("AR Account '1200' not found. Please ensure Chart of Accounts is seeded.");

            var salesAccount = await GetAccount(tenantId, "4000", ct: cancellationToken);
            if (salesAccount == null) return Result.Failure("Sales Revenue Account '4000' not found. Please ensure Chart of Accounts is seeded.");
            
            // Add AR Line (Debit)
            je.AddLine(arAccount, amount, 0, description ?? $"Invoice {invoiceNumber}", partnerId);

            // Add Sales Line (Credit)
            je.AddLine(salesAccount, 0, amount, "Sales Revenue");
            
            je.Post(_currentUserService.UserId); // Auto-post for now

            await _journalEntryRepository.AddAsync(je, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Finance Error: {ex.Message}");
        }
    }

    private async Task<Account?> GetAccount(Guid tenantId, string code, CancellationToken ct)
    {
        return await _accountRepository.FirstOrDefaultAsync(a => a.Code == code && a.TenantId == tenantId, ct);
    }
}
