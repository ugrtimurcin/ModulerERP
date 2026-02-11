using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.ProjectManagement.Domain.Entities;

public class ProjectTaskResource : BaseEntity
{
    public Guid ProjectTaskId { get; set; }
    public ProjectTask ProjectTask { get; set; } = null!;

    public Guid ProjectResourceId { get; set; }
    public ProjectResource ProjectResource { get; set; } = null!;

    public decimal AllocationPercent { get; set; } = 100;
}
