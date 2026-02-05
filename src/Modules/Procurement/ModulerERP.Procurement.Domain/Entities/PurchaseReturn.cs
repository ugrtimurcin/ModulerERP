using ModulerERP.SharedKernel.Entities;
using ModulerERP.Procurement.Domain.Enums;

namespace ModulerERP.Procurement.Domain.Entities;

public class PurchaseReturn : BaseEntity
{
    public string ReturnNumber { get; private set; } = string.Empty;
    public Guid SupplierId { get; private set; }
    public Guid GoodsReceiptId { get; private set; }
    public PurchaseReturnStatus Status { get; private set; }

    // Navigation
    public ICollection<PurchaseReturnItem> Items { get; private set; } = new List<PurchaseReturnItem>();

    private PurchaseReturn() { }

    public static PurchaseReturn Create(
        Guid tenantId,
        string returnNumber,
        Guid supplierId,
        Guid goodsReceiptId,
        Guid createdByUserId)
    {
        var ret = new PurchaseReturn
        {
            ReturnNumber = returnNumber,
            SupplierId = supplierId,
            GoodsReceiptId = goodsReceiptId,
            Status = PurchaseReturnStatus.Draft
        };
        ret.SetTenant(tenantId);
        ret.SetCreator(createdByUserId);
        return ret;
    }

    public void Ship()
    {
        Status = PurchaseReturnStatus.Shipped;
    }

    public void Complete()
    {
        Status = PurchaseReturnStatus.Completed;
    }
}
