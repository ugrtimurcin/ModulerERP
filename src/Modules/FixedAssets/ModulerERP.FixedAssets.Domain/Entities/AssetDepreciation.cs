namespace ModulerERP.FixedAssets.Domain.Entities;

/// <summary>
/// Monthly depreciation record.
/// </summary>
public class AssetDepreciation
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid AssetId { get; private set; }
    
    /// <summary>Period (e.g., '2026-01')</summary>
    public string Period { get; private set; } = string.Empty;
    
    public int Year { get; private set; }
    public int Month { get; private set; }
    
    /// <summary>Depreciation amount</summary>
    public decimal Amount { get; private set; }
    
    /// <summary>Book value after depreciation</summary>
    public decimal BookValueAfter { get; private set; }
    
    /// <summary>Linked journal entry</summary>
    public Guid? JournalEntryId { get; private set; }
    
    public DateTime CalculatedDate { get; private set; } = DateTime.UtcNow;

    // Navigation
    public Asset? Asset { get; private set; }

    private AssetDepreciation() { } // EF Core

    public static AssetDepreciation Create(
        Guid assetId,
        string period,
        int year,
        int month,
        decimal amount,
        decimal bookValueAfter)
    {
        return new AssetDepreciation
        {
            AssetId = assetId,
            Period = period,
            Year = year,
            Month = month,
            Amount = amount,
            BookValueAfter = bookValueAfter
        };
    }

    public void LinkJournalEntry(Guid journalEntryId) => JournalEntryId = journalEntryId;
}
