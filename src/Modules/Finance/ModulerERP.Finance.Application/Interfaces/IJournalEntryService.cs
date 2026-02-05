using ModulerERP.Finance.Application.DTOs;
using ModulerERP.SharedKernel.Results;

namespace ModulerERP.Finance.Application.Interfaces;

public interface IJournalEntryService
{
    Task<Result<List<JournalEntryDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<JournalEntryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<JournalEntryDto>> CreateManualEntryAsync(CreateJournalEntryDto dto, Guid createdByUserId, CancellationToken cancellationToken = default); // Future use
}
