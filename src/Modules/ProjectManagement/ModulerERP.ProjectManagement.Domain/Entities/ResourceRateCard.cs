using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.ProjectManagement.Domain.Entities;

public class ResourceRateCard : BaseEntity
{
    /// <summary>
    /// Optional: If set, this rate only applies to this specific project.
    /// If null, this is the standard rate for the resource.
    /// </summary>
    public Guid? ProjectId { get; set; }

    // Resource Reference (Polymorphic-ish)
    public Guid? EmployeeId { get; set; }
    public Guid? AssetId { get; set; }

    // Costing
    public decimal HourlyRate { get; set; }
    public Guid CurrencyId { get; set; }

    // Validity Period
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
