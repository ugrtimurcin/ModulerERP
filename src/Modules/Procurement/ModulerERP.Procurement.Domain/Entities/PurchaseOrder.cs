using ModulerERP.SharedKernel.Entities;
using ModulerERP.Procurement.Domain.Enums;

namespace ModulerERP.Procurement.Domain.Entities;

/// <summary>
/// Purchase order to suppliers.
/// Multi-currency with TRNC exchange rate freezing.
/// </summary>
public class PurchaseOrder : BaseEntity
{
    /// <summary>PO number (e.g., 'PO-2026-001')</summary>
    public string OrderNumber { get; private set; } = string.Empty;
    
    public Guid SupplierId { get; private set; }
    
    public PurchaseOrderStatus Status { get; private set; } = PurchaseOrderStatus.Draft;
    
    public Guid CurrencyId { get; private set; }
    public decimal ExchangeRate { get; private set; } = 1;
    
    /// <summary>Destination warehouse for receiving</summary>
    public Guid WarehouseId { get; private set; }
    
    /// <summary>Expected delivery date</summary>
    public DateTime? ExpectedDeliveryDate { get; private set; }
    
    public string? PaymentTerms { get; private set; }
    public string? ShippingTerms { get; private set; }
    public string? Notes { get; private set; }
    
    public DateTime? SentDate { get; private set; }
    public DateTime? ConfirmedDate { get; private set; }
    
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }

    // Navigation
    public ICollection<PurchaseOrderLine> Lines { get; private set; } = new List<PurchaseOrderLine>();
    public ICollection<GoodsReceipt> Receipts { get; private set; } = new List<GoodsReceipt>();
    public ICollection<SupplierBill> Bills { get; private set; } = new List<SupplierBill>();

    private PurchaseOrder() { } // EF Core

    public static PurchaseOrder Create(
        Guid tenantId,
        string orderNumber,
        Guid supplierId,
        Guid currencyId,
        decimal exchangeRate,
        Guid warehouseId,
        Guid createdByUserId,
        DateTime? expectedDeliveryDate = null,
        string? paymentTerms = null,
        string? shippingTerms = null)
    {
        var order = new PurchaseOrder
        {
            OrderNumber = orderNumber,
            SupplierId = supplierId,
            CurrencyId = currencyId,
            ExchangeRate = exchangeRate,
            WarehouseId = warehouseId,
            ExpectedDeliveryDate = expectedDeliveryDate,
            PaymentTerms = paymentTerms,
            ShippingTerms = shippingTerms
        };

        order.SetTenant(tenantId);
        order.SetCreator(createdByUserId);
        return order;
    }

    public void Send()
    {
        Status = PurchaseOrderStatus.Sent;
        SentDate = DateTime.UtcNow;
    }

    public void Confirm()
    {
        Status = PurchaseOrderStatus.Confirmed;
        ConfirmedDate = DateTime.UtcNow;
    }

    public void MarkPartiallyReceived() => Status = PurchaseOrderStatus.PartiallyReceived;
    public void MarkFullyReceived() => Status = PurchaseOrderStatus.FullyReceived;
    public void Cancel() => Status = PurchaseOrderStatus.Cancelled;

    public void UpdateTotals(decimal subTotal, decimal taxAmount)
    {
        SubTotal = subTotal;
        TaxAmount = taxAmount;
        TotalAmount = subTotal + taxAmount;
    }
}
