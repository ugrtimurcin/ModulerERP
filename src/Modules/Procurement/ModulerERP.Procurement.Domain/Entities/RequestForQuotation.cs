using ModulerERP.SharedKernel.Entities;
using ModulerERP.Procurement.Domain.Enums;

namespace ModulerERP.Procurement.Domain.Entities;

public class RequestForQuotation : BaseEntity
{
    public string RfqNumber { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public DateTime DeadLine { get; private set; }
    public RfqStatus Status { get; private set; }

    // Navigation
    public ICollection<RequestForQuotationItem> Items { get; private set; } = new List<RequestForQuotationItem>();
    public ICollection<PurchaseQuote> Quotes { get; private set; } = new List<PurchaseQuote>();

    private RequestForQuotation() { }

    public static RequestForQuotation Create(Guid tenantId, string rfqNumber, string title, DateTime deadline, Guid createdByUserId)
    {
        var rfq = new RequestForQuotation
        {
            RfqNumber = rfqNumber,
            Title = title,
            DeadLine = deadline,
            Status = RfqStatus.Open
        };
        rfq.SetTenant(tenantId);
        rfq.SetCreator(createdByUserId);
        return rfq;
    }

    public void Close()
    {
        Status = RfqStatus.Closed;
    }

    public void Award()
    {
        Status = RfqStatus.Awarded;
    }
}
