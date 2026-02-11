using ModulerERP.ProjectManagement.Domain.Enums;
using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.ProjectManagement.Domain.Entities;

public class ProjectTask : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Hierarchy
    public Guid? ParentTaskId { get; set; }

    // Scheduling
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }

    // Progress
    public decimal CompletionPercentage { get; set; } // 0 to 100
    public ProjectTaskStatus Status { get; set; }

    // Assignment - Many-to-Many via ProjectTaskResource
    public ICollection<ProjectTaskResource> Resources { get; set; } = new List<ProjectTaskResource>();
}
