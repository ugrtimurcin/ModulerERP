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

    public void AddLine(Account account, decimal debit, decimal credit, string? description = null, Guid? partnerId = null, Guid? currencyId = null, decimal? exchangeRate = null, decimal? originalAmount = null, Guid? costCenterId = null)
    {
        if (Status != JournalStatus.Draft)
            throw new InvalidOperationException("Can only add lines to draft entries");

        if (account.IsHeader)
            throw new InvalidOperationException($"Account {account.Code} is a header account and cannot be posted to.");

        if (debit == 0 && credit == 0)
            throw new InvalidOperationException("Line must have a non-zero debit or credit amount.");

        if (debit > 0 && credit > 0)
            throw new InvalidOperationException("Line cannot have both debit and credit amounts.");

        var lineNumber = Lines.Count + 1;
        JournalEntryLine line;

        if (debit > 0)
            line = JournalEntryLine.CreateDebit(Id, account.Id, debit, lineNumber, description, partnerId, costCenterId, currencyId, originalAmount, exchangeRate);
        else
            line = JournalEntryLine.CreateCredit(Id, account.Id, credit, lineNumber, description, partnerId, costCenterId, currencyId, originalAmount, exchangeRate);

        Lines.Add(line);
        TotalDebit += debit;
        TotalCredit += credit;
    }

    public void Post(Guid postedByUserId)
    {
        if (Lines.Count == 0)
            throw new InvalidOperationException("Cannot post an empty journal entry.");

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
}
