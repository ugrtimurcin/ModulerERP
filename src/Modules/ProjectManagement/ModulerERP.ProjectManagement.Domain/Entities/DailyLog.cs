using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.ProjectManagement.Domain.Entities;

public class DailyLog : BaseEntity
{
    public Guid ProjectId { get; set; }
    public DateTime Date { get; set; }
    
    public string WeatherCondition { get; set; } = string.Empty; // e.g., "Sunny, 25C"
    public string SiteManagerNote { get; set; } = string.Empty;
    
    public bool IsApproved { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public Guid? ApprovedByUserId { get; set; }

    public ICollection<DailyLogResourceUsage> ResourceUsages { get; set; } = new List<DailyLogResourceUsage>();
    public ICollection<DailyLogMaterialUsage> MaterialUsages { get; set; } = new List<DailyLogMaterialUsage>();
}

public class DailyLogResourceUsage : BaseEntity
{
    public Guid DailyLogId { get; set; }
    public Guid ProjectResourceId { get; set; }
    public Guid? ProjectTaskId { get; set; } // Optional: Link to Gantt Task
    
    public decimal HoursWorked { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class DailyLogMaterialUsage : BaseEntity
{
    public Guid DailyLogId { get; set; }
    public Guid ProductId { get; set; } // Inventory Product
    public decimal Quantity { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    
    public string Location { get; set; } = string.Empty; // e.g., "Block A"
}
