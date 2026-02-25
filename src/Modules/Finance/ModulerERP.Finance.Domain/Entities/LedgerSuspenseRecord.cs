using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Entities;
using System;

namespace ModulerERP.Finance.Domain.Entities;

/// <summary>
/// A "Pending Ledger Queue" or "Suspense Account" record.
/// Stores business events that failed to post to the generic LedgerEngine due to missing Configuration or Balancing errors.
/// This ensures the originating module transaction succeeds, allowing Finance Admins to correct the configuration and replay the entry later.
/// </summary>
public class LedgerSuspenseRecord : BaseEntity
{
    public Guid TenantId { get; private set; }
    
    public TransactionType TransactionType { get; private set; }
    public string? Category { get; private set; }
    
    public DateTime EventDate { get; private set; }
    public string SourceType { get; private set; }
    public Guid? SourceId { get; private set; }
    public string SourceNumber { get; private set; }
    public string Description { get; private set; }
    
    /// <summary>The raw LedgerPostRequest object, serialized to JSON</summary>
    public string RawRequestPayload { get; private set; }
    
    /// <summary>The human-readable exception indicating why this failed.</summary>
    public string ErrorMessage { get; private set; }
    
    public bool IsResolved { get; private set; }
    public DateTime? ResolvedDate { get; private set; }
    public Guid? ResolvedJournalEntryId { get; private set; }

    private LedgerSuspenseRecord() 
    {
        SourceType = string.Empty;
        SourceNumber = string.Empty;
        Description = string.Empty;
        RawRequestPayload = string.Empty;
        ErrorMessage = string.Empty;
    }

    public static LedgerSuspenseRecord Create(
        Guid tenantId,
        TransactionType transactionType,
        string? category,
        DateTime eventDate,
        string sourceType,
        Guid? sourceId,
        string sourceNumber,
        string description,
        string rawRequestPayload,
        string errorMessage)
    {
        return new LedgerSuspenseRecord
        {
            TenantId = tenantId,
            TransactionType = transactionType,
            Category = category,
            EventDate = eventDate,
            SourceType = sourceType,
            SourceId = sourceId,
            SourceNumber = sourceNumber,
            Description = description,
            RawRequestPayload = rawRequestPayload,
            ErrorMessage = errorMessage,
            IsResolved = false
        };
    }

    public void MarkAsResolved(Guid journalEntryId)
    {
        IsResolved = true;
        ResolvedDate = DateTime.UtcNow;
        ResolvedJournalEntryId = journalEntryId;
    }
}
