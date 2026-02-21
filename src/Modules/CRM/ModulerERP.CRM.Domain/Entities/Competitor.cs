using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.CRM.Domain.Entities;

/// <summary>
/// Represents market competitors to track who opportunities are lost to.
/// </summary>
public class Competitor : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Website { get; private set; }
    public string? Strengths { get; private set; }
    public string? Weaknesses { get; private set; }

    private Competitor() { } // EF Core

    public static Competitor Create(Guid tenantId, string name, Guid createdByUserId, string? website = null, string? strengths = null, string? weaknesses = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));

        var competitor = new Competitor
        {
            Name = name,
            Website = website,
            Strengths = strengths,
            Weaknesses = weaknesses
        };
        competitor.SetTenant(tenantId);
        competitor.SetCreator(createdByUserId);
        return competitor;
    }

    public void Update(string name, string? website, string? strengths, string? weaknesses)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));

        Name = name;
        Website = website;
        Strengths = strengths;
        Weaknesses = weaknesses;
    }
}
