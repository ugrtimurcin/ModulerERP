using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.JournalEntries.Commands;

public record CreateJournalEntryCommand(CreateJournalEntryDto Dto, Guid CreatedByUserId) : IRequest<Result<JournalEntryDto>>;

public class CreateJournalEntryCommandHandler : IRequestHandler<CreateJournalEntryCommand, Result<JournalEntryDto>>
{
    private readonly IJournalEntryRepository _repository;
    private readonly IRepository<FiscalPeriod> _fiscalPeriodRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateJournalEntryCommandHandler(
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

    public async Task<Result<JournalEntryDto>> Handle(CreateJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var createdByUserId = request.CreatedByUserId;

        // 1. Validation: Balanced?
        var totalDebit = dto.Lines.Sum(l => l.Debit);
        var totalCredit = dto.Lines.Sum(l => l.Credit);

        if (totalDebit != totalCredit)
            return Result<JournalEntryDto>.Failure($"Entry is unbalanced. Debit: {totalDebit}, Credit: {totalCredit}");

        if (totalDebit == 0)
            return Result<JournalEntryDto>.Failure("Entry must have non-zero amount.");

        // 2. Validation: Open Period?
        var period = await _fiscalPeriodRepository.FirstOrDefaultAsync(
                p => p.StartDate <= dto.EntryDate && p.EndDate >= dto.EntryDate, 
                cancellationToken);
            
        if (period == null)
             return Result<JournalEntryDto>.Failure($"No Fiscal Period found for date {dto.EntryDate:yyyy-MM-dd}.");

        if (period.Status != PeriodStatus.Open)
             return Result<JournalEntryDto>.Failure($"Fiscal Period '{period.Code}' is {period.Status}. Cannot post.");

        // 3. Create Entry
        var entryNumber = await _repository.GetNextEntryNumberAsync(cancellationToken);
        
        var je = JournalEntry.Create(
            _currentUserService.TenantId, // TenantId
            entryNumber,
            period.Id,
            dto.EntryDate,
            createdByUserId,
            "Manual",
            null,
            dto.ReferenceNumber,
            dto.Description
        );

        foreach (var l in dto.Lines)
        {
            var account = await _accountRepository.GetByIdAsync(l.AccountId, cancellationToken);
            if (account == null) return Result<JournalEntryDto>.Failure($"Account {l.AccountId} not found.");

            if (l.Debit > 0)
                je.AddLine(account, l.Debit, 0, l.Description, l.PartnerId, l.CurrencyId, l.ExchangeRate, l.OriginalAmount);
            else
                je.AddLine(account, 0, l.Credit, l.Description, l.PartnerId, l.CurrencyId, l.ExchangeRate, l.OriginalAmount);
        }

        je.Post(createdByUserId);

        await _repository.AddAsync(je, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch the created entity with includes to build DTO
        var entity = await _repository.GetByIdAsync(je.Id, cancellationToken);
        
        if (entity == null) return Result<JournalEntryDto>.Failure("Failed to retrieve created Journal Entry.");

        var resultDto = new JournalEntryDto
        {
            Id = entity.Id,
            EntryNumber = entity.EntryNumber,
            Description = entity.Description ?? string.Empty,
            EntryDate = entity.EntryDate,
            Status = entity.Status.ToString(),
            TotalDebit = entity.TotalDebit,
            TotalCredit = entity.TotalCredit,
            SourceType = entity.SourceType,
            SourceNumber = entity.SourceNumber,
            Lines = entity.Lines.Select(l => new JournalEntryLineDto
            {
                Id = l.Id,
                AccountCode = l.Account?.Code ?? "N/A",
                AccountName = l.Account?.Name ?? "Unknown",
                Description = l.Description ?? string.Empty,
                Debit = l.Debit,
                Credit = l.Credit,
                CurrencyId = l.CurrencyId,
                ExchangeRate = l.ExchangeRate,
                OriginalAmount = l.OriginalAmount
            }).OrderBy(l => l.Id).ToList()
        };

        return Result<JournalEntryDto>.Success(resultDto);
    }
}
