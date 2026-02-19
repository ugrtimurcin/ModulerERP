using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Sales.Domain.Entities;

/// <summary>
/// Allocates a payment to a specific invoice.
/// Enables partial payment matching and aging reports.
/// </summary>
public class SalesPayment : BaseEntity
{
    public Guid InvoiceId { get; private set; }
    
    /// <summary>Reference to Finance Payment entity</summary>
    public Guid? PaymentId { get; private set; }
    
    public decimal AllocatedAmount { get; private set; }
    public DateTime AllocationDate { get; private set; }
    public string? PaymentMethod { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? Notes { get; private set; }

    // Navigation
    public Invoice? Invoice { get; private set; }

    private SalesPayment() { } // EF Core

    public static SalesPayment Create(
        Guid tenantId,
        Guid invoiceId,
        decimal allocatedAmount,
        DateTime allocationDate,
        Guid createdByUserId,
        Guid? paymentId = null,
        string? paymentMethod = null,
        string? referenceNumber = null,
        string? notes = null)
    {
        var sp = new SalesPayment
        {
            InvoiceId = invoiceId,
            AllocatedAmount = allocatedAmount,
            AllocationDate = allocationDate,
            PaymentId = paymentId,
            PaymentMethod = paymentMethod,
            ReferenceNumber = referenceNumber,
            Notes = notes
        };

        sp.SetTenant(tenantId);
        sp.SetCreator(createdByUserId);
        return sp;
    }
}
