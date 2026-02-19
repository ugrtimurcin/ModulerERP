using ModulerERP.SharedKernel.Entities;
using ModulerERP.Sales.Domain.Enums;

namespace ModulerERP.Sales.Domain.Entities;

/// <summary>
/// Customer returns (RMA) with dual-currency for KKTC compliance.
/// </summary>
public class SalesReturn : BaseEntity
{
    public string ReturnNumber { get; private set; } = string.Empty;

    public Guid? InvoiceId { get; private set; }
    public Guid PartnerId { get; private set; }

    public ReturnStatus Status { get; private set; } = ReturnStatus.Pending;

    // ── Transaction Currency ──
    public Guid CurrencyId { get; private set; }
    public decimal ExchangeRate { get; private set; } = 1;

    // ── Local Currency (KKTC: TRY) ──
    public Guid? LocalCurrencyId { get; private set; }
    public decimal LocalExchangeRate { get; private set; } = 1;
    public decimal LocalTotalAmount { get; private set; }
    public decimal LocalRefundAmount { get; private set; }

    // ── Details ──
    public string Reason { get; private set; } = string.Empty;
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
        Guid? warehouseId = null,
        Guid? localCurrencyId = null,
        decimal localExchangeRate = 1)
    {
        var salesReturn = new SalesReturn
        {
            ReturnNumber = returnNumber,
            PartnerId = partnerId,
            CurrencyId = currencyId,
            ExchangeRate = exchangeRate,
            Reason = reason,
            InvoiceId = invoiceId,
            WarehouseId = warehouseId,
            LocalCurrencyId = localCurrencyId,
            LocalExchangeRate = localExchangeRate
        };

        salesReturn.SetTenant(tenantId);
        salesReturn.SetCreator(createdByUserId);
        return salesReturn;
    }

    // ── Status Transitions (with guards) ──

    public void Approve()
    {
        if (Status != ReturnStatus.Pending)
            throw new InvalidOperationException($"Cannot approve return in '{Status}' status. Must be Pending.");
        Status = ReturnStatus.Approved;
        ApprovedDate = DateTime.UtcNow;
    }

    public void Receive()
    {
        if (Status != ReturnStatus.Approved)
            throw new InvalidOperationException($"Cannot receive return in '{Status}' status. Must be Approved.");
        Status = ReturnStatus.Received;
        ReceivedDate = DateTime.UtcNow;
    }

    public void Refund(decimal amount)
    {
        if (Status != ReturnStatus.Received)
            throw new InvalidOperationException($"Cannot refund return in '{Status}' status. Must be Received.");
        Status = ReturnStatus.Refunded;
        RefundAmount = amount;
        LocalRefundAmount = amount * LocalExchangeRate;
        RefundedDate = DateTime.UtcNow;
    }

    public void Reject()
    {
        if (Status != ReturnStatus.Pending)
            throw new InvalidOperationException($"Cannot reject return in '{Status}' status. Must be Pending.");
        Status = ReturnStatus.Rejected;
    }

    public void UpdateTotalAmount(decimal totalAmount)
    {
        TotalAmount = totalAmount;
        LocalTotalAmount = totalAmount * LocalExchangeRate;
    }

    public void SetNotes(string? notes) => Notes = notes;
}
