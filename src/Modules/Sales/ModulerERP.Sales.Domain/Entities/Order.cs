using ModulerERP.SharedKernel.Entities;
using ModulerERP.Sales.Domain.Enums;

namespace ModulerERP.Sales.Domain.Entities;

/// <summary>
/// Committed sales order with KKTC dual-currency support.
/// Created from accepted Quote or directly.
/// </summary>
public class Order : BaseEntity
{
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid? QuoteId { get; private set; }
    public Guid PartnerId { get; private set; }

    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    // ── Transaction Currency ──
    public Guid CurrencyId { get; private set; }
    public decimal ExchangeRate { get; private set; } = 1;

    // ── Local Currency (KKTC: TRY) ──
    public Guid? LocalCurrencyId { get; private set; }
    public decimal LocalExchangeRate { get; private set; } = 1;
    public decimal LocalSubTotal { get; private set; }
    public decimal LocalTaxAmount { get; private set; }
    public decimal LocalTotalAmount { get; private set; }

    // ── Addresses & Terms ──
    public string? ShippingAddress { get; private set; }
    public string? BillingAddress { get; private set; }
    public Guid? WarehouseId { get; private set; }
    public DateTime? RequestedDeliveryDate { get; private set; }
    public string? PaymentTerms { get; private set; }
    public string? Notes { get; private set; }

    // ── Totals ──
    public decimal SubTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }

    // ── Document-level Discount & Withholding Tax (Stopaj) ──
    public decimal DocumentDiscountRate { get; private set; }
    public decimal DocumentDiscountAmount { get; private set; }
    public decimal WithholdingTaxRate { get; private set; }
    public decimal WithholdingTaxAmount { get; private set; }

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
        string? paymentTerms = null,
        Guid? localCurrencyId = null,
        decimal localExchangeRate = 1)
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
            PaymentTerms = paymentTerms,
            LocalCurrencyId = localCurrencyId,
            LocalExchangeRate = localExchangeRate
        };

        order.SetTenant(tenantId);
        order.SetCreator(createdByUserId);
        return order;
    }

    // ── Status Transitions (with guards) ──

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm order in '{Status}' status. Must be Pending.");
        Status = OrderStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.Confirmed)
            throw new InvalidOperationException($"Cannot cancel order in '{Status}' status. Must be Pending or Confirmed.");
        Status = OrderStatus.Cancelled;
    }

    public void MarkPartiallyShipped()
    {
        if (Status != OrderStatus.Confirmed && Status != OrderStatus.PartiallyShipped)
            throw new InvalidOperationException($"Cannot mark order as partially shipped in '{Status}' status.");
        Status = OrderStatus.PartiallyShipped;
    }

    public void MarkShipped()
    {
        if (Status != OrderStatus.Confirmed && Status != OrderStatus.PartiallyShipped)
            throw new InvalidOperationException($"Cannot mark order as shipped in '{Status}' status.");
        Status = OrderStatus.Shipped;
    }

    public void MarkInvoiced()
    {
        if (Status != OrderStatus.Shipped && Status != OrderStatus.Confirmed && Status != OrderStatus.PartiallyShipped)
            throw new InvalidOperationException($"Cannot mark order as invoiced in '{Status}' status.");
        Status = OrderStatus.Invoiced;
    }

    // ── Totals ──

    public void UpdateTotals(decimal subTotal, decimal discountAmount, decimal taxAmount,
        decimal docDiscountRate = 0, decimal withholdingTaxRate = 0)
    {
        SubTotal = subTotal;
        DiscountAmount = discountAmount;
        TaxAmount = taxAmount;

        DocumentDiscountRate = docDiscountRate;
        DocumentDiscountAmount = (subTotal - discountAmount) * (docDiscountRate / 100);

        var netAfterDocDiscount = subTotal - discountAmount - DocumentDiscountAmount;
        WithholdingTaxRate = withholdingTaxRate;
        WithholdingTaxAmount = netAfterDocDiscount * (withholdingTaxRate / 100);

        TotalAmount = netAfterDocDiscount + taxAmount - WithholdingTaxAmount;

        // Local currency equivalents
        LocalSubTotal = SubTotal * LocalExchangeRate;
        LocalTaxAmount = TaxAmount * LocalExchangeRate;
        LocalTotalAmount = TotalAmount * LocalExchangeRate;
    }

    public void SetAddresses(string? shippingAddress, string? billingAddress)
    {
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
    }

    public void SetNotes(string? notes) => Notes = notes;
}
