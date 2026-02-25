using Microsoft.EntityFrameworkCore;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using System.Text.Json;

namespace ModulerERP.Finance.Application.Services;

public class LedgerPostingService : ILedgerPostingService
{
    private readonly IRepository<PostingProfile> _profileRepo;
    private readonly IRepository<FiscalPeriod> _fiscalPeriodRepo;
    private readonly IJournalEntryRepository _journalEntryRepo;
    private readonly IRepository<Account> _accountRepo;
    private readonly IRepository<LedgerSuspenseRecord> _suspenseRepo;
    private readonly IUnitOfWork _unitOfWork;

    public LedgerPostingService(
        IRepository<PostingProfile> profileRepo,
        IRepository<FiscalPeriod> fiscalPeriodRepo,
        IJournalEntryRepository journalEntryRepo,
        IRepository<Account> accountRepo,
        IRepository<LedgerSuspenseRecord> suspenseRepo,
        IUnitOfWork unitOfWork)
    {
        _profileRepo = profileRepo;
        _fiscalPeriodRepo = fiscalPeriodRepo;
        _journalEntryRepo = journalEntryRepo;
        _accountRepo = accountRepo;
        _suspenseRepo = suspenseRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> PostAsync(LedgerPostRequest request, CancellationToken cancellationToken)
    {
        try 
        {
            // 1. Verify Fiscal Period constraints
            var period = await _fiscalPeriodRepo.FirstOrDefaultAsync(
                p => p.StartDate <= request.EventDate && p.EndDate >= request.EventDate && p.TenantId == request.TenantId, 
                cancellationToken);

            if (period == null || period.Status != PeriodStatus.Open)
            {
                throw new InvalidOperationException($"No open Fiscal Period exists for Tenant {request.TenantId} on {request.EventDate:yyyy-MM-dd}.");
            }

            // 2. Resolve the abstract Posting Profile from configuration
            var profiles = await _profileRepo.GetAllAsync(cancellationToken);
            
            var profile = profiles.FirstOrDefault(p => 
                p.TenantId == request.TenantId && 
                p.TransactionType == request.TransactionType && 
                p.Category == request.Category);

            profile ??= profiles.FirstOrDefault(p => 
                p.TenantId == request.TenantId && 
                p.TransactionType == request.TransactionType && 
                p.IsDefault);

            if (profile == null)
            {
                throw new InvalidOperationException($"No Posting Profile configured for {request.TransactionType} (Category: {request.Category ?? "Default"}).");
            }

            // 3. Prepare the Journal Entry Header
            var entryNumber = await _journalEntryRepo.GetNextEntryNumberAsync(cancellationToken);
            
            var je = JournalEntry.Create(
                request.TenantId,
                entryNumber,
                period.Id,
                request.EventDate,
                request.UserId,
                request.SourceType,
                request.SourceId,
                request.SourceNumber,
                request.Description
            );

            // 4. Map the Raw Amounts to Specific Accounts based on Role configuration
            var accounts = await _accountRepo.GetAllAsync(cancellationToken);

            foreach (var amount in request.Amounts)
            {
                var lineConfig = profile.Lines.FirstOrDefault(l => l.Role == amount.Role);
                if (lineConfig == null)
                {
                    throw new InvalidOperationException($"Posting Profile '{profile.Name}' is missing an Account mapping for Role '{amount.Role}'.");
                }

                var account = accounts.FirstOrDefault(a => a.Id == lineConfig.AccountId);
                if (account == null)
                {
                    throw new InvalidOperationException($"Account mapping for Role '{amount.Role}' points to an invalid Account ID.");
                }

                if (account.RequiresCostCenter && !request.CostCenterId.HasValue)
                    throw new InvalidOperationException($"Account '{account.Code}' strictly requires a Cost Center for '{amount.Role}'.");

                if (account.RequiresPartner && !request.PartnerId.HasValue)
                    throw new InvalidOperationException($"Account '{account.Code}' strictly requires a Partner for '{amount.Role}'.");

                if (amount.IsDebit)
                {
                    je.AddLine(account, amount.BaseAmount, 0, amount.TransactionAmount, 0, request.BaseCurrencyId, request.TransactionCurrencyId, request.ExchangeRate, amount.LineDescription, request.PartnerId, request.CostCenterId);
                }
                else
                {
                    je.AddLine(account, 0, amount.BaseAmount, 0, amount.TransactionAmount, request.BaseCurrencyId, request.TransactionCurrencyId, request.ExchangeRate, amount.LineDescription, request.PartnerId, request.CostCenterId);
                }
            }

            // 5. Verify Balance Constraint
            je.Post(request.UserId);

            // 6. Save Ledger
            await _journalEntryRepo.AddAsync(je, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return je.Id;
        }
        catch (Exception ex)
        {
            var rawRequestPayload = JsonSerializer.Serialize(request);
            var suspenseRecord = LedgerSuspenseRecord.Create(
                request.TenantId,
                request.TransactionType,
                request.Category,
                request.EventDate,
                request.SourceType,
                request.SourceId,
                request.SourceNumber,
                request.Description,
                rawRequestPayload,
                ex.Message
            );

            await _suspenseRepo.AddAsync(suspenseRecord, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return Guid.Empty; // Indicate transaction succeeded functionally but financially fell to suspense
        }
    }
}
