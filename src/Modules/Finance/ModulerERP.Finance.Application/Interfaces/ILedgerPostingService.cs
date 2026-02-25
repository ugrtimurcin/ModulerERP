using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;

namespace ModulerERP.Finance.Application.Interfaces;

/// <summary>
/// The Unified Ledger Engine. Translates generic business events into strict Double-Entry Journal Entries.
/// </summary>
public interface ILedgerPostingService
{
    /// <summary>
    /// Processes a generic request by evaluating the active Posting Profiles and generating an immutable, balanced Journal Entry.
    /// </summary>
    /// <param name="request">The generic business event data</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>The generated JournalEntry ID, or throws a configuration exception if the mapping is incomplete.</returns>
    Task<Guid> PostAsync(LedgerPostRequest request, CancellationToken cancellationToken);
}
