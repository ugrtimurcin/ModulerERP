using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.SharedKernel.Results;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.JournalEntries.Queries;

// ── Get All Journal Entries ──
public record GetJournalEntriesQuery : IRequest<Result<List<JournalEntryDto>>>;

public class GetJournalEntriesQueryHandler : IRequestHandler<GetJournalEntriesQuery, Result<List<JournalEntryDto>>>
{
    private readonly IJournalEntryRepository _repository;

    public GetJournalEntriesQueryHandler(IJournalEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<JournalEntryDto>>> Handle(GetJournalEntriesQuery request, CancellationToken cancellationToken)
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
}

// ── Get Journal Entry By Id ──
public record GetJournalEntryByIdQuery(Guid Id) : IRequest<Result<JournalEntryDto>>;

public class GetJournalEntryByIdQueryHandler : IRequestHandler<GetJournalEntryByIdQuery, Result<JournalEntryDto>>
{
    private readonly IJournalEntryRepository _repository;

    public GetJournalEntryByIdQueryHandler(IJournalEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<JournalEntryDto>> Handle(GetJournalEntryByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
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
}
