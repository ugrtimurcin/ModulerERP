using ModulerERP.ProjectManagement.Domain.Enums;
using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.ProjectManagement.Domain.Entities;

public class BillOfQuantitiesItem : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid? ParentId { get; set; } // Hierarchy
    
    // Identification
    public string ItemCode { get; set; } = string.Empty; // e.g., "15.120.1001"
    public string Description { get; set; } = string.Empty;
    
    // Metrics
    public decimal Quantity { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    
    // Financials
    public decimal ContractUnitPrice { get; set; } // Income (Price sold to client)
    public decimal EstimatedUnitCost { get; set; } // Expense (Internal budget)
    
    public BudgetCategory Category { get; set; }

    // Computed Totals
    public decimal TotalContractAmount => Quantity * ContractUnitPrice;
    public decimal TotalEstimatedCost => Quantity * EstimatedUnitCost;
}
