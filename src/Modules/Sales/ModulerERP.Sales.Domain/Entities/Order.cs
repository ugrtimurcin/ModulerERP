using ModulerERP.SharedKernel.Entities;
using ModulerERP.Sales.Domain.Enums;

namespace ModulerERP.Sales.Domain.Entities;

/// <summary>
/// Committed sales order.
/// Created from accepted Quote or directly.
/// </summary>
public class Order : BaseEntity
{
    /// <summary>Order number (e.g., 'ORD-2026-500')</summary>
    public string OrderNumber { get; private set; } = string.Empty;
    
    /// <summary>Source quote if converted</summary>
    public Guid? QuoteId { get; private set; }
    
    public Guid PartnerId { get; private set; }
    
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    
    public Guid CurrencyId { get; private set; }
    public decimal ExchangeRate { get; private set; } = 1;
    
    /// <summary>Snapshot shipping address</summary>
    public string? ShippingAddress { get; private set; }
    
    /// <summary>Snapshot billing address</summary>
    public string? BillingAddress { get; private set; }
    
    /// <summary>Default source warehouse for shipments</summary>
    public Guid? WarehouseId { get; private set; }
    
    /// <summary>Requested delivery date</summary>
    public DateTime? RequestedDeliveryDate { get; private set; }
    
    public string? PaymentTerms { get; private set; }
    public string? Notes { get; private set; }
    
    public decimal SubTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }

    // Navigation
    public Quote? Quote { get; private set; }
    public ICollection<OrderLine> Lines { get; private set; } = new List<OrderLine>();
    public ICollection<Shipment> Shipments { get; private set; } = new List<Shipment>();
    public ICollection<Invoice> Invoices { get; private set; } = new List<Invoice>();

    private Order() { } // EF Core

    public static Order Create(
        Guid tenantId,
        string orderNumber,
        Guid partnerId,
        Guid currencyId,
        decimal exchangeRate,
        Guid createdByUserId,
        Guid? quoteId = null,
        Guid? warehouseId = null,
        DateTime? requestedDeliveryDate = null,
        string? paymentTerms = null)
    {
        var order = new Order
        {
            OrderNumber = orderNumber,
            PartnerId = partnerId,
            CurrencyId = currencyId,
            ExchangeRate = exchangeRate,
            QuoteId = quoteId,
            WarehouseId = warehouseId,
            RequestedDeliveryDate = requestedDeliveryDate,
            PaymentTerms = paymentTerms
        };

        order.SetTenant(tenantId);
        order.SetCreator(createdByUserId);
        return order;
    }

    public void Confirm() => Status = OrderStatus.Confirmed;
    public void Cancel() => Status = OrderStatus.Cancelled;
    public void MarkPartiallyShipped() => Status = OrderStatus.PartiallyShipped;
    public void MarkShipped() => Status = OrderStatus.Shipped;
    public void MarkInvoiced() => Status = OrderStatus.Invoiced;

    public void UpdateTotals(decimal subTotal, decimal discountAmount, decimal taxAmount)
    {
        SubTotal = subTotal;
        DiscountAmount = discountAmount;
        TaxAmount = taxAmount;
        TotalAmount = subTotal - discountAmount + taxAmount;
    }

    public void SetAddresses(string? shippingAddress, string? billingAddress)
    {
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
    }
}
