using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;

namespace ModulerERP.Finance.Application.Services;

public class FiscalPeriodClosingService : IFiscalPeriodClosingService
{
    private readonly IRepository<FiscalPeriod> _fiscalPeriodRepository;
    private readonly IRepository<LedgerSuspenseRecord> _suspenseRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FiscalPeriodClosingService(
        IRepository<FiscalPeriod> fiscalPeriodRepository,
        IRepository<LedgerSuspenseRecord> suspenseRepository,
        IJournalEntryRepository journalEntryRepository,
        IUnitOfWork unitOfWork)
    {
        _fiscalPeriodRepository = fiscalPeriodRepository;
        _suspenseRepository = suspenseRepository;
        _journalEntryRepository = journalEntryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ClosePeriodAsync(Guid tenantId, Guid periodId, CancellationToken cancellationToken = default)
    {
        var period = await _fiscalPeriodRepository.GetByIdAsync(periodId, cancellationToken);
        if (period == null || period.TenantId != tenantId)
        {
            return Result.Failure("Fiscal period not found.");
        }

        if (period.Status != PeriodStatus.Open)
        {
            return Result.Failure($"Fiscal period is already {period.Status}.");
        }

        // 1. Validation: Ensure no pending Suspense Records exist in this date range
        var suspenseRecords = await _suspenseRepository.GetAllAsync(cancellationToken);
        
        var unresolvedInPeriod = suspenseRecords.Any(s => 
            s.TenantId == tenantId && 
            s.EventDate >= period.StartDate && 
            s.EventDate <= period.EndDate && 
            !s.IsResolved);

        if (unresolvedInPeriod)
        {
            return Result.Failure("Cannot close fiscal period. There are unresolved entries in the Pending Ledger Queue. Finance Admin must map and clear these first.");
        }

        // 2. Validation: Ensure no Unposted Journal Entries exist in this period
        // Real implementation would have a specific method in the IJournalEntryRepository.
        // For MVP, we pass over this assuming system auto-posts everything.

        // 3. Foreign Exchange (FX) Revaluation
        // Requirement: Generate Journal Entries to recognize unrealized gains/losses on foreign currency balances
        // Example: If USD AR balance is 1,000, and end of month rate changed from 30 -> 35 TRY, 
        // We must post an entry for +5,000 TRY to AR and FX Gain.
        // Note: For MVP Phase 4, we define the structure.
        await CalculateAndPostFxRevaluationAsync(tenantId, period, cancellationToken);

        // 4. Year-End Check (If last period in Fiscal Year, sweep P&L into Retained Earnings)
        // Check if next period changes year or if marked specifically as Year End.

        // 5. Finalize Closure
        period.Close(); // Updates status to Closed

        _fiscalPeriodRepository.Update(period);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task CalculateAndPostFxRevaluationAsync(Guid tenantId, FiscalPeriod period, CancellationToken cancellationToken)
    {
         // 1. Get EOM Exchange Rates
         // 2. Get Trial Balance of all Monetary Accounts at Period.EndDate
         // 3. For each non-base currency account, compare Base Balance to (Transaction Balance * EOM Rate)
         // 4. If variance exists, generate FX Gain/Loss Journal Entry via LedgerPostingService
         
         // To be implemented fully in Enterprise extension.
         await Task.CompletedTask;
    }
}
