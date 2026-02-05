using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System.Linq;

namespace ModulerERP.Finance.Application.Services;

public class JournalEntryService : IJournalEntryService
{
    private readonly IJournalEntryRepository _repository;
    private readonly IRepository<FiscalPeriod> _fiscalPeriodRepository;
    private readonly IUnitOfWork _unitOfWork;

    public JournalEntryService(
        IJournalEntryRepository repository, 
        IRepository<FiscalPeriod> fiscalPeriodRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _fiscalPeriodRepository = fiscalPeriodRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<JournalEntryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        
        var dtos = entities.Select(e => new JournalEntryDto
        {
            Id = e.Id,
            EntryNumber = e.EntryNumber,
            Description = e.Description ?? string.Empty,
            EntryDate = e.EntryDate,
            Status = e.Status.ToString(),
            TotalDebit = e.TotalDebit,
            TotalCredit = e.TotalCredit,
            SourceType = e.SourceType,
            SourceNumber = e.SourceNumber
        }).ToList();

        return Result<List<JournalEntryDto>>.Success(dtos);
    }

    public async Task<Result<JournalEntryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return Result<JournalEntryDto>.Failure("Journal Entry not found");

        var dto = new JournalEntryDto
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
                Credit = l.Credit
            }).OrderBy(l => l.Id).ToList()
        };

        return Result<JournalEntryDto>.Success(dto);
    }

    public async Task<Result<JournalEntryDto>> CreateManualEntryAsync(CreateJournalEntryDto dto, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
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
            Guid.Empty, // TenantId (Needs context)
            entryNumber,
            period.Id,
            dto.EntryDate,
            createdByUserId,
            "Manual",
            null,
            dto.ReferenceNumber,
            dto.Description
        );

        int lineNum = 1;
        foreach (var l in dto.Lines)
        {
            if (l.Debit > 0)
                je.Lines.Add(JournalEntryLine.CreateDebit(je.Id, l.AccountId, l.Debit, lineNum++, l.Description, l.PartnerId));
            else
                je.Lines.Add(JournalEntryLine.CreateCredit(je.Id, l.AccountId, l.Credit, lineNum++, l.Description, l.PartnerId));
        }

        je.UpdateTotals(totalDebit, totalCredit);
        je.Post(createdByUserId);

        await _repository.AddAsync(je, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(je.Id, cancellationToken);
    }
}
