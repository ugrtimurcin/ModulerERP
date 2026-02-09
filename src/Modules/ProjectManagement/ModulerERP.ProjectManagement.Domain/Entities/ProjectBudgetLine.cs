using ModulerERP.ProjectManagement.Domain.Enums;
using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.ProjectManagement.Domain.Entities;

public class ProjectBudgetLine : BaseEntity
{
    public Guid ProjectId { get; set; }
    
    // Cost Details
    public string CostCode { get; set; } = string.Empty; // e.g., "15.220.1002"
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public decimal UnitPrice { get; set; }
    
    // Computed Total (Persisted or calculated in app logic - usually persisted for queries)
    public decimal TotalAmount { get; private set; }

    public BudgetCategory Category { get; set; }

    public void CalculateTotal()
    {
        TotalAmount = Quantity * UnitPrice;
    }
}
