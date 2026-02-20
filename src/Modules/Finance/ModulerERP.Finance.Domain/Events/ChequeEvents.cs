using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Finance.Domain.Events;

public class ChequeCreatedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public Guid TenantId { get; }
    public Guid ChequeId { get; }
    public decimal Amount { get; }
    public string ChequeNumber { get; }
    public ChequeType Type { get; }

    public ChequeCreatedEvent(Guid tenantId, Guid chequeId, decimal amount, string chequeNumber, ChequeType type)
    {
        TenantId = tenantId;
        ChequeId = chequeId;
        Amount = amount;
        ChequeNumber = chequeNumber;
        Type = type;
    }
}

public class ChequeStatusUpdatedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public Guid TenantId { get; }
    public Guid ChequeId { get; }
    public ChequeStatus OldStatus { get; }
    public ChequeStatus NewStatus { get; }
    public decimal Amount { get; }
    public string ChequeNumber { get; }

    public ChequeStatusUpdatedEvent(Guid tenantId, Guid chequeId, ChequeStatus oldStatus, ChequeStatus newStatus, decimal amount, string chequeNumber)
    {
        TenantId = tenantId;
        ChequeId = chequeId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        Amount = amount;
        ChequeNumber = chequeNumber;
    }
}
