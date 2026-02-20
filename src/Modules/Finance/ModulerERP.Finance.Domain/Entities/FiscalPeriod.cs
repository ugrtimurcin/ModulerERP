using ModulerERP.SharedKernel.Entities;
using ModulerERP.Finance.Domain.Enums;

namespace ModulerERP.Finance.Domain.Entities;

/// <summary>
/// Fiscal year and period management.
/// </summary>
public class FiscalPeriod : BaseEntity
{
    /// <summary>Period code (e.g., '2026-01')</summary>
    public string Code { get; private set; } = string.Empty;
    
    public string Name { get; private set; } = string.Empty;
    
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    
    /// <summary>Fiscal year this period belongs to</summary>
    public int FiscalYear { get; private set; }
    
    /// <summary>Period number within the year (1-12 or 1-13)</summary>
    public int PeriodNumber { get; private set; }
    
    public PeriodStatus Status { get; private set; } = PeriodStatus.Open;
    
    /// <summary>Is this an adjustment period?</summary>
    public bool IsAdjustment { get; private set; }

    // Navigation
    public ICollection<JournalEntry> JournalEntries { get; private set; } = new List<JournalEntry>();

    private FiscalPeriod() { } // EF Core

    public static FiscalPeriod Create(
        Guid tenantId,
        string code,
        string name,
        DateTime startDate,
        DateTime endDate,
        int fiscalYear,
        int periodNumber,
        Guid createdByUserId,
        bool isAdjustment = false)
    {
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date");

        var period = new FiscalPeriod
        {
            Code = code,
            Name = name,
            StartDate = startDate,
            EndDate = endDate,
            FiscalYear = fiscalYear,
            PeriodNumber = periodNumber,
            IsAdjustment = isAdjustment
        };

        period.SetTenant(tenantId);
        period.SetCreator(createdByUserId);
        return period;
    }

    public void Close() => Status = PeriodStatus.Closed;
    public void Lock() => Status = PeriodStatus.Locked;
    
    public void Reopen(string reason, bool isAuthorizedAdmin)
    {
        if (!isAuthorizedAdmin)
            throw new UnauthorizedAccessException("Only an authorized Finance Administrator can reopen a fiscal period.");
            
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A valid reason must be provided to reopen a fiscal period.", nameof(reason));

        Status = PeriodStatus.Open;
    }

    public bool ContainsDate(DateTime date) =>
        date >= StartDate && date <= EndDate;
}
