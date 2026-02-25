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
    
    /// <summary>Base Currency Debit amount (e.g., TRY)</summary>
    public decimal BaseDebit { get; private set; }
    
    /// <summary>Base Currency Credit amount (e.g., TRY)</summary>
    public decimal BaseCredit { get; private set; }
    
    /// <summary>Transaction Currency Debit amount (e.g., GBP)</summary>
    public decimal TransactionDebit { get; private set; }
    
    /// <summary>Transaction Currency Credit amount (e.g., GBP)</summary>
    public decimal TransactionCredit { get; private set; }

    /// <summary>Base Currency of the Tenant</summary>
    public Guid BaseCurrencyId { get; private set; }
    
    /// <summary>Original currency used in transaction</summary>
    public Guid TransactionCurrencyId { get; private set; }
    
    /// <summary>Exchange rate used at posting time</summary>
    public decimal ExchangeRate { get; private set; }
    
    /// <summary>Optional partner reference</summary>
    public Guid? PartnerId { get; private set; }
    
    /// <summary>Optional cost center</summary>
    public Guid? CostCenterId { get; private set; }
    
    /// <summary>Line description</summary>
    public string? Description { get; private set; }

    // Navigation
    public JournalEntry? JournalEntry { get; private set; }
    public Account? Account { get; private set; }

    private JournalEntryLine() { } // EF Core

    internal static JournalEntryLine CreateDebit(
        Guid journalEntryId,
        Guid accountId,
        decimal baseAmount,
        decimal txAmount,
        Guid baseCurrencyId,
        Guid txCurrencyId,
        decimal exchangeRate,
        int lineNumber,
        string? description = null,
        Guid? partnerId = null,
        Guid? costCenterId = null)
    {
        return new JournalEntryLine
        {
            JournalEntryId = journalEntryId,
            AccountId = accountId,
            LineNumber = lineNumber,
            BaseDebit = baseAmount,
            BaseCredit = 0,
            TransactionDebit = txAmount,
            TransactionCredit = 0,
            BaseCurrencyId = baseCurrencyId,
            TransactionCurrencyId = txCurrencyId,
            ExchangeRate = exchangeRate,
            Description = description,
            PartnerId = partnerId,
            CostCenterId = costCenterId
        };
    }

    internal static JournalEntryLine CreateCredit(
        Guid journalEntryId,
        Guid accountId,
        decimal baseAmount,
        decimal txAmount,
        Guid baseCurrencyId,
        Guid txCurrencyId,
        decimal exchangeRate,
        int lineNumber,
        string? description = null,
        Guid? partnerId = null,
        Guid? costCenterId = null)
    {
        return new JournalEntryLine
        {
            JournalEntryId = journalEntryId,
            AccountId = accountId,
            LineNumber = lineNumber,
            BaseDebit = 0,
            BaseCredit = baseAmount,
            TransactionDebit = 0,
            TransactionCredit = txAmount,
            BaseCurrencyId = baseCurrencyId,
            TransactionCurrencyId = txCurrencyId,
            ExchangeRate = exchangeRate,
            Description = description,
            PartnerId = partnerId,
            CostCenterId = costCenterId
        };
    }
}
