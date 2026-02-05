using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Finance.Application.Interfaces;

public interface IJournalEntryRepository : IRepository<JournalEntry>
{
    Task<string> GetNextEntryNumberAsync(CancellationToken cancellationToken = default);
}
