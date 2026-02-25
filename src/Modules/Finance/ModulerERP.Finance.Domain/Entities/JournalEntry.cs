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
    
    /// <summary>Total debit amount in Base Currency</summary>
    public decimal TotalBaseDebit { get; private set; }
    
    /// <summary>Total credit amount in Base Currency</summary>
    public decimal TotalBaseCredit { get; private set; }

    /// <summary>Total debit amount in Transaction Currency</summary>
    public decimal TotalTransactionDebit { get; private set; }
    
    /// <summary>Total credit amount in Transaction Currency</summary>
    public decimal TotalTransactionCredit { get; private set; }
    
    /// <summary>If this entry reverses another, its ID is stored here.</summary>
    public Guid? ReversesJournalEntryId { get; private set; }
    
    public DateTime? PostedDate { get; private set; }
    public Guid? PostedByUserId { get; private set; }

    // Navigation
    public FiscalPeriod? FiscalPeriod { get; private set; }
    public ICollection<JournalEntryLine> Lines { get; private set; } = new List<JournalEntryLine>();

    public bool IsBalanced => TotalBaseDebit == TotalBaseCredit;

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

    public void SetReversal(Guid originalEntryId)
    {
        if (Lines.Count > 0)
            throw new InvalidOperationException("Can only set reversal ID before lines are added.");
        ReversesJournalEntryId = originalEntryId;
    }

    public void AddLine(Account account, decimal baseDebit, decimal baseCredit, decimal txDebit, decimal txCredit, Guid baseCurrencyId, Guid txCurrencyId, decimal exchangeRate, string? description = null, Guid? partnerId = null, Guid? costCenterId = null)
    {
        if (Status != JournalStatus.Draft)
            throw new InvalidOperationException("Can only add lines to draft entries");

        if (account.IsHeader)
            throw new InvalidOperationException($"Account {account.Code} is a header account and cannot be posted to.");

        if (baseDebit == 0 && baseCredit == 0 && txDebit == 0 && txCredit == 0)
            throw new InvalidOperationException("Line must have a non-zero base or tx debit/credit amount.");

        if (baseDebit > 0 && baseCredit > 0)
            throw new InvalidOperationException("Line cannot have both debit and credit amounts in base currency.");

        var lineNumber = Lines.Count + 1;
        JournalEntryLine line;

        if (baseDebit > 0 || txDebit > 0)
            line = JournalEntryLine.CreateDebit(Id, account.Id, baseDebit, txDebit, baseCurrencyId, txCurrencyId, exchangeRate, lineNumber, description, partnerId, costCenterId);
        else
            line = JournalEntryLine.CreateCredit(Id, account.Id, baseCredit, txCredit, baseCurrencyId, txCurrencyId, exchangeRate, lineNumber, description, partnerId, costCenterId);

        Lines.Add(line);
        TotalBaseDebit += baseDebit;
        TotalBaseCredit += baseCredit;
        TotalTransactionDebit += txDebit;
        TotalTransactionCredit += txCredit;
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
