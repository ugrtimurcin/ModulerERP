using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.JournalEntries.Commands;

public record ReverseJournalEntryCommand(Guid JournalEntryId, Guid ReversedByUserId) : IRequest<Result<JournalEntryDto>>;

public class ReverseJournalEntryCommandHandler : IRequestHandler<ReverseJournalEntryCommand, Result<JournalEntryDto>>
{
    private readonly IJournalEntryRepository _repository;
    private readonly IRepository<FiscalPeriod> _fiscalPeriodRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ReverseJournalEntryCommandHandler(
        IJournalEntryRepository repository,
        IRepository<FiscalPeriod> fiscalPeriodRepository,
        IRepository<Account> accountRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _fiscalPeriodRepository = fiscalPeriodRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<JournalEntryDto>> Handle(ReverseJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var originalEntry = await _repository.GetByIdAsync(request.JournalEntryId, cancellationToken);
        if (originalEntry == null)
            return Result<JournalEntryDto>.Failure("Original Journal Entry not found.");

        if (originalEntry.Status == JournalStatus.Voided)
            return Result<JournalEntryDto>.Failure("Journal Entry is already voided.");

        var newEntryDate = DateTime.UtcNow;

        var period = await _fiscalPeriodRepository.FirstOrDefaultAsync(
            p => p.StartDate <= newEntryDate && p.EndDate >= newEntryDate,
            cancellationToken);

        if (period == null || period.Status != PeriodStatus.Open)
            return Result<JournalEntryDto>.Failure($"No open Fiscal Period found for date {newEntryDate:yyyy-MM-dd}. Cannot post reversal.");

        var entryNumber = await _repository.GetNextEntryNumberAsync(cancellationToken);

        var reversalEntry = JournalEntry.Create(
            _currentUserService.TenantId,
            entryNumber,
            period.Id,
            newEntryDate,
            request.ReversedByUserId,
            "Reversal",
            originalEntry.Id,
            originalEntry.EntryNumber,
            $"Reversal of {originalEntry.EntryNumber}"
        );

        reversalEntry.SetReversal(originalEntry.Id);

        foreach (var l in originalEntry.Lines)
        {
            var account = await _accountRepository.GetByIdAsync(l.AccountId, cancellationToken);
            if (account == null) continue; // Safety check

            // Swap Base and Tx Debits/Credits
            reversalEntry.AddLine(
                account, 
                baseDebit: l.BaseCredit, 
                baseCredit: l.BaseDebit, 
                txDebit: l.TransactionCredit, 
                txCredit: l.TransactionDebit, 
                baseCurrencyId: l.BaseCurrencyId, 
                txCurrencyId: l.TransactionCurrencyId, 
                exchangeRate: l.ExchangeRate, 
                description: $"Reversal of {l.Description ?? "Line"}", 
                partnerId: l.PartnerId, 
                costCenterId: l.CostCenterId);
        }

        reversalEntry.Post(request.ReversedByUserId);
        
        // Mark original as Voided
        originalEntry.Void();
        _repository.Update(originalEntry);

        await _repository.AddAsync(reversalEntry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var entity = await _repository.GetByIdAsync(reversalEntry.Id, cancellationToken);

        if (entity == null) return Result<JournalEntryDto>.Failure("Failed to retrieve created reversal entry.");

        var resultDto = new JournalEntryDto
        {
            Id = entity.Id,
            EntryNumber = entity.EntryNumber,
            Description = entity.Description ?? string.Empty,
            EntryDate = entity.EntryDate,
            Status = entity.Status.ToString(),
            TotalBaseDebit = entity.TotalBaseDebit,
            TotalBaseCredit = entity.TotalBaseCredit,
            TotalTransactionDebit = entity.TotalTransactionDebit,
            TotalTransactionCredit = entity.TotalTransactionCredit,
            SourceType = entity.SourceType,
            SourceNumber = entity.SourceNumber,
            Lines = entity.Lines.Select(l => new JournalEntryLineDto
            {
                Id = l.Id,
                AccountCode = l.Account?.Code ?? "N/A",
                AccountName = l.Account?.Name ?? "Unknown",
                Description = l.Description ?? string.Empty,
                BaseDebit = l.BaseDebit,
                BaseCredit = l.BaseCredit,
                TransactionDebit = l.TransactionDebit,
                TransactionCredit = l.TransactionCredit,
                BaseCurrencyId = l.BaseCurrencyId,
                TransactionCurrencyId = l.TransactionCurrencyId,
                ExchangeRate = l.ExchangeRate
            }).OrderBy(l => l.Id).ToList()
        };

        return Result<JournalEntryDto>.Success(resultDto);
    }
}
