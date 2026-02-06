using MediatR;

namespace ModulerERP.SharedKernel.Interfaces;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
