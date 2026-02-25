using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Finance.Domain.Entities;

/// <summary>
/// TRNC Compliant Tax Configuration supporting multi-stage taxes (KDV + Stopaj simultaneously).
/// </summary>
public class TaxProfile : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public ICollection<TaxProfileLine> Lines { get; private set; } = new List<TaxProfileLine>();

    private TaxProfile() { } // EF Core

    public static TaxProfile Create(Guid tenantId, string code, string name, Guid createdByUserId, string? description = null)
    {
        var profile = new TaxProfile
        {
            Code = code,
            Name = name,
            Description = description
        };
        profile.SetTenant(tenantId);
        profile.SetCreator(createdByUserId);
        return profile;
    }

    public void AddLine(Guid taxRateId, bool isInclusive, int calculationOrder)
    {
        Lines.Add(TaxProfileLine.Create(Id, taxRateId, isInclusive, calculationOrder));
    }
}
