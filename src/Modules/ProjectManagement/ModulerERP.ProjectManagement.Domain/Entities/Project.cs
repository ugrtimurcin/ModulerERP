using ModulerERP.ProjectManagement.Domain.Enums;
using ModulerERP.SharedKernel.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.ValueObjects;

namespace ModulerERP.ProjectManagement.Domain.Entities;

public class Project : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Relationships
    public Guid? CustomerId { get; set; }
    public Guid? ProjectManagerId { get; set; }
    public Guid? VirtualWarehouseId { get; set; } // Linked to Inventory

    // Commercial
    public Guid ContractCurrencyId { get; set; }
    public Guid BudgetCurrencyId { get; set; } // [NEW] Added for multi-currency budgeting
    public Guid LocalCurrencyId { get; set; } // [NEW] Added for multi-currency reporting
    public decimal ContractAmount { get; set; }
    
    // TRNC Financial Settings
    public decimal DefaultRetentionRate { get; set; } = 0.10m; // 10%
    public decimal DefaultWithholdingTaxRate { get; set; } = 0.04m; // 4%

    // Location
    public Address? SiteAddress { get; set; }

    // Dates
    public DateTime StartDate { get; set; }
    public DateTime? TargetDate { get; set; }
    public DateTime? ActualFinishDate { get; set; }

    // State
    public ProjectStatus Status { get; set; }
    public decimal CompletionPercentage { get; set; }
    
    // Budgeting (V2.0 - BoQ)
    public ICollection<BillOfQuantitiesItem> BoQItems { get; set; } = new List<BillOfQuantitiesItem>();

    public decimal GetTotalContractAmount()
    {
        return BoQItems.Sum(x => x.TotalContractAmount);
    }

    public decimal GetTotalEstimatedCost()
    {
        return BoQItems.Sum(x => x.TotalEstimatedCost);
    }
    
    // Change Management (Zeyilname)
    public ICollection<ProjectChangeOrder> ChangeOrders { get; set; } = new List<ProjectChangeOrder>();
    
    public void ApplyChangeOrder(ProjectChangeOrder order)
    {
        if (order.Status != ChangeOrderStatus.Approved)
            throw new InvalidOperationException("Cannot apply unapproved change order.");
            
        ContractAmount += order.AmountChange;
        
        if (TargetDate.HasValue && order.TimeExtensionDays != 0)
        {
            TargetDate = TargetDate.Value.AddDays(order.TimeExtensionDays);
        }
    }
}
