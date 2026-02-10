using ModulerERP.SharedKernel.Entities;
using ModulerERP.SharedKernel.Enums; // Assuming Employee/Asset enum exists or string Role

namespace ModulerERP.ProjectManagement.Domain.Entities;

public class ProjectResource : BaseEntity
{
    public Guid ProjectId { get; set; }
    
    // Resource Reference
    public Guid? EmployeeId { get; set; } // Link to HR
    public Guid? AssetId { get; set; } // Link to Fixed Assets
    
    public string Role { get; set; } = string.Empty; // e.g., "Foreman", "Excavator"
    
    public decimal HourlyCost { get; set; } // Cost to Project
    public Guid CurrencyId { get; set; }
}
