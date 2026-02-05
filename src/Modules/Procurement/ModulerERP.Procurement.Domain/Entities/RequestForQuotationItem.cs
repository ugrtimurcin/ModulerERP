using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Procurement.Domain.Entities;

public class RequestForQuotationItem : BaseEntity
{
    public Guid RfqId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal TargetQuantity { get; private set; }

    // Navigation
    public RequestForQuotation RequestForQuotation { get; private set; } = null!;

    private RequestForQuotationItem() { }

    public static RequestForQuotationItem Create(Guid tenantId, Guid rfqId, Guid productId, decimal targetQuantity, Guid createdByUserId)
    {
        var item = new RequestForQuotationItem
        {
            RfqId = rfqId,
            ProductId = productId,
            TargetQuantity = targetQuantity
        };
        item.SetTenant(tenantId);
        item.SetCreator(createdByUserId);
        return item;
    }
}
