using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.CRM.Domain.Entities;

/// <summary>
/// Represents geographical territories (e.g., districts in TRNC: Lefkoşa, Girne, Mağusa).
/// Used for sales reporting and access control.
/// </summary>
public class Territory : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    
    /// <summary>Manager responsible for this territory</summary>
    public Guid? RegionalManagerId { get; private set; }

    private Territory() { } // EF Core

    public static Territory Create(Guid tenantId, string name, string code, Guid createdByUserId, Guid? regionalManagerId = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required", nameof(code));

        var territory = new Territory
        {
            Name = name,
            Code = code.ToUpperInvariant(),
            RegionalManagerId = regionalManagerId
        };
        territory.SetTenant(tenantId);
        territory.SetCreator(createdByUserId);
        return territory;
    }

    public void Update(string name, string code, Guid? regionalManagerId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required", nameof(code));

        Name = name;
        Code = code.ToUpperInvariant();
        RegionalManagerId = regionalManagerId;
    }
}
