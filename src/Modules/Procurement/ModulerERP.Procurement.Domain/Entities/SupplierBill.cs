using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Procurement.Domain.Entities;

/// <summary>
/// Supplier invoice for payables.
/// Links to Finance module.
/// </summary>
public class SupplierBill : BaseEntity
{
    /// <summary>Bill number from supplier</summary>
    public string BillNumber { get; private set; } = string.Empty;
    
    /// <summary>Our internal reference</summary>
    public string? InternalReference { get; private set; }
    
    public Guid? PurchaseOrderId { get; private set; }
    public Guid SupplierId { get; private set; }
    
    public Guid CurrencyId { get; private set; }
    public decimal ExchangeRate { get; private set; } = 1;
    
    public DateTime BillDate { get; private set; }
    public DateTime DueDate { get; private set; }
    
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    
    /// <summary>Amount paid so far</summary>
    public decimal PaidAmount { get; private set; }
    
    public bool IsPaid => PaidAmount >= TotalAmount;
    public decimal BalanceDue => TotalAmount - PaidAmount;
    
    public string? Notes { get; private set; }

    // Navigation
    public PurchaseOrder? PurchaseOrder { get; private set; }
    public ICollection<SupplierBillLine> Lines { get; private set; } = new List<SupplierBillLine>();

    private SupplierBill() { } // EF Core

    public static SupplierBill Create(
        Guid tenantId,
        string billNumber,
        Guid supplierId,
        Guid currencyId,
        decimal exchangeRate,
        DateTime billDate,
        DateTime dueDate,
        Guid createdByUserId,
        Guid? purchaseOrderId = null,
        string? internalReference = null)
    {
        var bill = new SupplierBill
        {
            BillNumber = billNumber,
            SupplierId = supplierId,
            CurrencyId = currencyId,
            ExchangeRate = exchangeRate,
            BillDate = billDate,
            DueDate = dueDate,
            PurchaseOrderId = purchaseOrderId,
            InternalReference = internalReference
        };

        bill.SetTenant(tenantId);
        bill.SetCreator(createdByUserId);
        return bill;
    }

    public void RecordPayment(decimal amount)
    {
        PaidAmount += amount;
    }

    public void UpdateTotals(decimal subTotal, decimal taxAmount)
    {
        SubTotal = subTotal;
        TaxAmount = taxAmount;
        TotalAmount = subTotal + taxAmount;
    }
}
