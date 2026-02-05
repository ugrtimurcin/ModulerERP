using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Procurement.Domain.Entities;

public class PurchaseQuoteItem : BaseEntity
{
    public Guid QuoteId { get; private set; }
    public Guid RfqItemId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Price { get; private set; }
    public int LeadTimeDays { get; private set; }

    // Navigation
    public PurchaseQuote PurchaseQuote { get; private set; } = null!;

    private PurchaseQuoteItem() { }

    public static PurchaseQuoteItem Create(
        Guid tenantId, 
        Guid quoteId, 
        Guid rfqItemId, 
        Guid productId, 
        decimal price, 
        int leadTimeDays, 
        Guid createdByUserId)
    {
        var item = new PurchaseQuoteItem
        {
            QuoteId = quoteId,
            RfqItemId = rfqItemId,
            ProductId = productId,
            Price = price,
            LeadTimeDays = leadTimeDays
        };
        item.SetTenant(tenantId);
        item.SetCreator(createdByUserId);
        return item;
    }
}
