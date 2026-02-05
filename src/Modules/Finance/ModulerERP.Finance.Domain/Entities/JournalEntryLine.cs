namespace ModulerERP.Finance.Domain.Entities;

/// <summary>
/// Journal entry line - individual debit/credit.
/// </summary>
public class JournalEntryLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid JournalEntryId { get; private set; }
    public Guid AccountId { get; private set; }
    
    public int LineNumber { get; private set; }
    
    /// <summary>Debit amount (0 if credit)</summary>
    public decimal Debit { get; private set; }
    
    /// <summary>Credit amount (0 if debit)</summary>
    public decimal Credit { get; private set; }
    
    /// <summary>Optional partner reference</summary>
    public Guid? PartnerId { get; private set; }
    
    /// <summary>Optional cost center</summary>
    public Guid? CostCenterId { get; private set; }
    
    /// <summary>Line description</summary>
    public string? Description { get; private set; }
    
    /// <summary>Original currency (for multi-currency transactions)</summary>
    public Guid? CurrencyId { get; private set; }
    
    /// <summary>Amount in original currency</summary>
    public decimal? OriginalAmount { get; private set; }
    
    /// <summary>Exchange rate used</summary>
    public decimal? ExchangeRate { get; private set; }

    // Navigation
    public JournalEntry? JournalEntry { get; private set; }
    public Account? Account { get; private set; }

    private JournalEntryLine() { } // EF Core

    public static JournalEntryLine CreateDebit(
        Guid journalEntryId,
        Guid accountId,
        decimal amount,
        int lineNumber,
        string? description = null,
        Guid? partnerId = null,
        Guid? costCenterId = null,
        Guid? currencyId = null,
        decimal? originalAmount = null,
        decimal? exchangeRate = null)
    {
        return new JournalEntryLine
        {
            JournalEntryId = journalEntryId,
            AccountId = accountId,
            LineNumber = lineNumber,
            Debit = amount,
            Credit = 0,
            Description = description,
            PartnerId = partnerId,
            CostCenterId = costCenterId,
            CurrencyId = currencyId,
            OriginalAmount = originalAmount,
            ExchangeRate = exchangeRate
        };
    }

    public static JournalEntryLine CreateCredit(
        Guid journalEntryId,
        Guid accountId,
        decimal amount,
        int lineNumber,
        string? description = null,
        Guid? partnerId = null,
        Guid? costCenterId = null,
        Guid? currencyId = null,
        decimal? originalAmount = null,
        decimal? exchangeRate = null)
    {
        return new JournalEntryLine
        {
            JournalEntryId = journalEntryId,
            AccountId = accountId,
            LineNumber = lineNumber,
            Debit = 0,
            Credit = amount,
            Description = description,
            PartnerId = partnerId,
            CostCenterId = costCenterId,
            CurrencyId = currencyId,
            OriginalAmount = originalAmount,
            ExchangeRate = exchangeRate
        };
    }
}
