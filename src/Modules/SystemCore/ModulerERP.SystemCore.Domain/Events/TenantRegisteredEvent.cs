using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.SystemCore.Domain.Events;

public class TenantRegisteredEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public Guid TenantId { get; }
    public string TenantName { get; }
    public string AdminEmail { get; }

    public TenantRegisteredEvent(Guid tenantId, string tenantName, string adminEmail)
    {
        TenantId = tenantId;
        TenantName = tenantName;
        AdminEmail = adminEmail;
    }
}
