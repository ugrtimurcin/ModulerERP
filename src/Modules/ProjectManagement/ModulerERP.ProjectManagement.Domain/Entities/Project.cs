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
    public ProjectStatus Status { get; set; }
    public decimal CompletionPercentage { get; set; }
    
    // Owned Entity
    public ProjectBudget Budget { get; set; } = new();
}
