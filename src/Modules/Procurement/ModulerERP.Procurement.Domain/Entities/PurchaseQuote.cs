using ModulerERP.SharedKernel.Entities;
using ModulerERP.Procurement.Domain.Enums;

namespace ModulerERP.Procurement.Domain.Entities;

public class PurchaseQuote : BaseEntity
{
    public Guid RfqId { get; private set; }
    public Guid SupplierId { get; private set; }
    public string QuoteReference { get; private set; } = string.Empty;
    public DateTime ValidUntil { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool IsSelected { get; private set; }
    public PurchaseQuoteStatus Status { get; private set; }

    // Navigation
    public RequestForQuotation RequestForQuotation { get; private set; } = null!;
    public ICollection<PurchaseQuoteItem> Items { get; private set; } = new List<PurchaseQuoteItem>();

    private PurchaseQuote() { }

    public static PurchaseQuote Create(
        Guid tenantId, 
        Guid rfqId, 
        Guid supplierId, 
        string quoteReference, 
        DateTime validUntil, 
        decimal totalAmount, 
        Guid createdByUserId)
    {
        var quote = new PurchaseQuote
        {
            RfqId = rfqId,
            SupplierId = supplierId,
            QuoteReference = quoteReference,
            ValidUntil = validUntil,
            TotalAmount = totalAmount,
            IsSelected = false,
            Status = PurchaseQuoteStatus.Pending
        };
        quote.SetTenant(tenantId);
        quote.SetCreator(createdByUserId);
        return quote;
    }

    public void Accept()
    {
        Status = PurchaseQuoteStatus.Accepted;
        IsSelected = true;
    }

    public void Reject()
    {
        Status = PurchaseQuoteStatus.Rejected;
        IsSelected = false;
    }
}
