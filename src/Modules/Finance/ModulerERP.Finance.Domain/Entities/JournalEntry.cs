using ModulerERP.SharedKernel.Entities;
using ModulerERP.Finance.Domain.Enums;

namespace ModulerERP.Finance.Domain.Entities;

/// <summary>
/// Journal entry header - the immutable ledger.
/// </summary>
public class JournalEntry : BaseEntity
{
    /// <summary>Journal entry number (e.g., 'JE-2026-0001')</summary>
    public string EntryNumber { get; private set; } = string.Empty;
    
    public Guid FiscalPeriodId { get; private set; }
    
    public DateTime EntryDate { get; private set; }
    
    public JournalStatus Status { get; private set; } = JournalStatus.Draft;
    
    /// <summary>Source document type (Invoice, Payment, Manual)</summary>
    public string? SourceType { get; private set; }
    
    /// <summary>Source document ID</summary>
    public Guid? SourceId { get; private set; }
    
    /// <summary>Source document number</summary>
    public string? SourceNumber { get; private set; }
    
    public string? Description { get; private set; }
    
    /// <summary>Total debit amount</summary>
    public decimal TotalDebit { get; private set; }
    
    /// <summary>Total credit amount</summary>
    public decimal TotalCredit { get; private set; }
    
    public DateTime? PostedDate { get; private set; }
    public Guid? PostedByUserId { get; private set; }

    // Navigation
    public FiscalPeriod? FiscalPeriod { get; private set; }
    public ICollection<JournalEntryLine> Lines { get; private set; } = new List<JournalEntryLine>();

    public bool IsBalanced => TotalDebit == TotalCredit;

    private JournalEntry() { } // EF Core

    public static JournalEntry Create(
        Guid tenantId,
        string entryNumber,
        Guid fiscalPeriodId,
        DateTime entryDate,
        Guid createdByUserId,
        string? sourceType = null,
        Guid? sourceId = null,
        string? sourceNumber = null,
        string? description = null)
    {
        var entry = new JournalEntry
        {
            EntryNumber = entryNumber,
            FiscalPeriodId = fiscalPeriodId,
            EntryDate = entryDate,
            SourceType = sourceType,
            SourceId = sourceId,
            SourceNumber = sourceNumber,
            Description = description
        };

        entry.SetTenant(tenantId);
        entry.SetCreator(createdByUserId);
        return entry;
    }

    public void Post(Guid postedByUserId)
    {
        if (!IsBalanced)
            throw new InvalidOperationException("Journal entry is not balanced");

        Status = JournalStatus.Posted;
        PostedDate = DateTime.UtcNow;
        PostedByUserId = postedByUserId;
    }

    public void Void()
    {
        if (Status != JournalStatus.Posted)
            throw new InvalidOperationException("Only posted entries can be voided");

        Status = JournalStatus.Voided;
    }

    public void UpdateTotals(decimal totalDebit, decimal totalCredit)
    {
        TotalDebit = totalDebit;
        TotalCredit = totalCredit;
    }
}
