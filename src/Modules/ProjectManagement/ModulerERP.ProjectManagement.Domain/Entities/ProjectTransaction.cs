using ModulerERP.ProjectManagement.Domain.Enums;
using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.ProjectManagement.Domain.Entities;

public class ProjectTransaction : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid? ProjectTaskId { get; set; }

    // Source
    public string SourceModule { get; set; } = string.Empty; // "Procurement", "Inventory", "HR", "Finance"
    public Guid SourceRecordId { get; set; } // InvoiceId, StockMovementId, etc.
    public string Description { get; set; } = string.Empty;

    // Financials
    public decimal Amount { get; set; } // Transaction Amount
    public Guid CurrencyId { get; set; } // Original Currency
    public decimal ExchangeRate { get; set; }
    
    public decimal ProjectCurrencyAmount { get; set; } // [NEW] Amount in Project Budget Currency
    public decimal LocalCurrencyAmount { get; set; } // [NEW] Amount in Tenant Local Currency
    public decimal AmountReporting { get; set; } // Deprecated? Or Same as LocalCurrencyAmount
    public DateTime Date { get; set; }

    public ProjectTransactionType Type { get; set; } // Expense/Income category
    public ProjectTransactionSourceType SourceType { get; set; } // [NEW] Source System (Stock, HR, etc.)
}
