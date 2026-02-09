using ModulerERP.ProjectManagement.Domain.Enums;
using ModulerERP.SharedKernel.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.ProjectManagement.Domain.Entities;

public class Project : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Relationships
    public Guid? CustomerId { get; set; }
    public Guid? ProjectManagerId { get; set; }

    // Commercial
    public Guid ContractCurrencyId { get; set; }
    public decimal ContractAmount { get; set; }

    // Dates
    public DateTime StartDate { get; set; }
    public DateTime? TargetDate { get; set; }
    public DateTime? ActualFinishDate { get; set; }

    // State
    // State
    public ProjectStatus Status { get; set; }
    public decimal CompletionPercentage { get; set; }
    
    // Budgeting (V2.0)
    public ICollection<ProjectBudgetLine> BudgetLines { get; set; } = new List<ProjectBudgetLine>();

    public decimal GetTotalBudget()
    {
        return BudgetLines.Sum(x => x.TotalAmount);
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
