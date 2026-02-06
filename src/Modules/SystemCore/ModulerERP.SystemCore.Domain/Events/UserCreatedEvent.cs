using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.SystemCore.Domain.Events;

public class UserCreatedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public Guid UserId { get; }
    public Guid TenantId { get; }
    public string Email { get; }
    public string FirstName { get; }
    public string LastName { get; }

    public UserCreatedEvent(Guid userId, Guid tenantId, string email, string firstName, string lastName)
    {
        UserId = userId;
        TenantId = tenantId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }
}
