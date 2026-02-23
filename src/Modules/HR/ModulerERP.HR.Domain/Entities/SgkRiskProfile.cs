using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.HR.Domain.Entities;

public class SgkRiskProfile : BaseEntity
{
    public string Name { get; private set; } = string.Empty; // e.g. "Tehlike Sınıfı 1", "Ofis Çalışanı"
    public decimal EmployerSgkMultiplier { get; private set; } // e.g. 1.0, 1.15
    public string Description { get; private set; } = string.Empty;

    private SgkRiskProfile() { }

    public static SgkRiskProfile Create(Guid tenantId, Guid createdBy, string name, decimal employerSgkMultiplier, string description)
    {
        var profile = new SgkRiskProfile
        {
            Name = name,
            EmployerSgkMultiplier = employerSgkMultiplier,
            Description = description
        };
        profile.SetTenant(tenantId);
        profile.SetCreator(createdBy);
        return profile;
    }

    public void Update(string name, decimal multiplier, string description)
    {
        Name = name;
        EmployerSgkMultiplier = multiplier;
        Description = description;
    }
}
