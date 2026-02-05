namespace ModulerERP.Finance.Domain.Entities;

/// <summary>
/// Payment allocation to invoices/bills.
/// Handles partial payments and overpayments.
/// </summary>
public class PaymentAllocation
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PaymentId { get; private set; }
    
    /// <summary>Document type: SalesInvoice, SupplierBill</summary>
    public string DocumentType { get; private set; } = string.Empty;
    
    /// <summary>Document ID</summary>
    public Guid DocumentId { get; private set; }
    
    /// <summary>Amount allocated to this document</summary>
    public decimal AllocatedAmount { get; private set; }

    // Navigation
    public Payment? Payment { get; private set; }

    private PaymentAllocation() { } // EF Core

    public static PaymentAllocation Create(
        Guid paymentId,
        string documentType,
        Guid documentId,
        decimal allocatedAmount)
    {
        if (allocatedAmount <= 0)
            throw new ArgumentException("Allocated amount must be positive");

        return new PaymentAllocation
        {
            PaymentId = paymentId,
            DocumentType = documentType,
            DocumentId = documentId,
            AllocatedAmount = allocatedAmount
        };
    }
}
