using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.SharedKernel.IntegrationEvents;

public class InvoiceApprovedEvent : IDomainEvent
{
    public Guid TenantId { get; }
    public Guid InvoiceId { get; }
    public Guid? ProjectId { get; }
    public decimal Amount { get; }
    public Guid CurrencyId { get; }
    public DateTime Date { get; }

    public string InvoiceNumber { get; }
    public string SupplierName { get; }

    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public InvoiceApprovedEvent(Guid tenantId, Guid invoiceId, Guid? projectId, decimal amount, Guid currencyId, DateTime date, string invoiceNumber, string supplierName)
    {
        TenantId = tenantId;
        InvoiceId = invoiceId;
        ProjectId = projectId;
        Amount = amount;
        CurrencyId = currencyId;
        Date = date;
        InvoiceNumber = invoiceNumber;
        SupplierName = supplierName;
    }
}
