using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Finance.Domain.Events;

public class PaymentCreatedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public Guid TenantId { get; }
    public Guid PaymentId { get; }
    public decimal Amount { get; }
    public Guid AccountId { get; }
    public string PaymentNumber { get; }

    public PaymentCreatedEvent(Guid tenantId, Guid paymentId, decimal amount, Guid accountId, string paymentNumber)
    {
        TenantId = tenantId;
        PaymentId = paymentId;
        Amount = amount;
        AccountId = accountId;
        PaymentNumber = paymentNumber;
    }
}
