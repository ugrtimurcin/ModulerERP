using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.CRM.Domain.Entities;

/// <summary>
/// Standardized list of reasons for rejecting a lead or losing an opportunity.
/// </summary>
public class RejectionReason : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private RejectionReason() { } // EF Core

    public static RejectionReason Create(Guid tenantId, string code, string description, Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required", nameof(code));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required", nameof(description));

        var reason = new RejectionReason
        {
            Code = code.ToUpperInvariant(),
            Description = description
        };
        reason.SetTenant(tenantId);
        reason.SetCreator(createdByUserId);
        return reason;
    }

    public void Update(string code, string description)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required", nameof(code));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required", nameof(description));

        Code = code.ToUpperInvariant();
        Description = description;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
