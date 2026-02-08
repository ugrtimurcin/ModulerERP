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
    public decimal Amount { get; set; }
    public Guid CurrencyId { get; set; } // Original Currency
    public decimal ExchangeRate { get; set; }
    public decimal AmountReporting { get; set; } // Converted to Tenant's Functional Currency (TRY)
    public DateTime Date { get; set; }

    public ProjectTransactionType Type { get; set; }
}
