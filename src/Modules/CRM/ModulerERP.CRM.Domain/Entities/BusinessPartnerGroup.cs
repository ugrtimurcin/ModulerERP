using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.CRM.Domain.Entities;

/// <summary>
/// Categorization for partners (e.g., 'Hotels', 'Construction Firms')
/// </summary>
public class BusinessPartnerGroup : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    // Navigation
    public ICollection<BusinessPartner> Partners { get; private set; } = new List<BusinessPartner>();

    private BusinessPartnerGroup() { } // EF Core

    public static BusinessPartnerGroup Create(Guid tenantId, string name, Guid createdByUserId, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name is required", nameof(name));

        var group = new BusinessPartnerGroup
        {
            Name = name,
            Description = description
        };

        group.SetTenant(tenantId);
        group.SetCreator(createdByUserId);
        return group;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }
}
