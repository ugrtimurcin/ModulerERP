using ModulerERP.SharedKernel.Entities;
using ModulerERP.Sales.Domain.Enums;

namespace ModulerERP.Sales.Domain.Entities;

/// <summary>
/// Customer returns (RMA) processing.
/// </summary>
public class SalesReturn : BaseEntity
{
    /// <summary>Return number (e.g., 'RET-2026-001')</summary>
    public string ReturnNumber { get; private set; } = string.Empty;
    
    public Guid? InvoiceId { get; private set; }
    public Guid PartnerId { get; private set; }
    
    public ReturnStatus Status { get; private set; } = ReturnStatus.Pending;
    
    public Guid CurrencyId { get; private set; }
    public decimal ExchangeRate { get; private set; } = 1;
    
    /// <summary>Reason for return</summary>
    public string Reason { get; private set; } = string.Empty;
    
    /// <summary>Warehouse to receive returned goods</summary>
    public Guid? WarehouseId { get; private set; }
    
    public DateTime? ApprovedDate { get; private set; }
    public DateTime? ReceivedDate { get; private set; }
    public DateTime? RefundedDate { get; private set; }
    
    public string? Notes { get; private set; }
    
    public decimal TotalAmount { get; private set; }
    public decimal RefundAmount { get; private set; }

    // Navigation
    public Invoice? Invoice { get; private set; }
    public ICollection<SalesReturnLine> Lines { get; private set; } = new List<SalesReturnLine>();

    private SalesReturn() { } // EF Core

    public static SalesReturn Create(
        Guid tenantId,
        string returnNumber,
        Guid partnerId,
        Guid currencyId,
        decimal exchangeRate,
        string reason,
        Guid createdByUserId,
        Guid? invoiceId = null,
        Guid? warehouseId = null)
    {
        var salesReturn = new SalesReturn
        {
            ReturnNumber = returnNumber,
            PartnerId = partnerId,
            CurrencyId = currencyId,
            ExchangeRate = exchangeRate,
            Reason = reason,
            InvoiceId = invoiceId,
            WarehouseId = warehouseId
        };

        salesReturn.SetTenant(tenantId);
        salesReturn.SetCreator(createdByUserId);
        return salesReturn;
    }

    public void Approve()
    {
        Status = ReturnStatus.Approved;
        ApprovedDate = DateTime.UtcNow;
    }

    public void Receive()
    {
        Status = ReturnStatus.Received;
        ReceivedDate = DateTime.UtcNow;
    }

    public void Refund(decimal amount)
    {
        Status = ReturnStatus.Refunded;
        RefundAmount = amount;
        RefundedDate = DateTime.UtcNow;
    }

    public void Reject() => Status = ReturnStatus.Rejected;

    public void UpdateTotalAmount(decimal totalAmount) => TotalAmount = totalAmount;
}
