using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Procurement.Domain.Entities;

public class PurchaseReturnItem : BaseEntity
{
    public Guid ReturnId { get; private set; }
    public Guid ReceiptItemId { get; private set; }
    public decimal Quantity { get; private set; }
    public Guid? ReasonId { get; private set; }

    // Navigation
    public PurchaseReturn PurchaseReturn { get; private set; } = null!;

    private PurchaseReturnItem() { }

    public static PurchaseReturnItem Create(
        Guid tenantId,
        Guid returnId,
        Guid receiptItemId,
        decimal quantity,
        Guid createdByUserId,
        Guid? reasonId = null)
    {
        var item = new PurchaseReturnItem
        {
            ReturnId = returnId,
            ReceiptItemId = receiptItemId,
            Quantity = quantity,
            ReasonId = reasonId
        };
        item.SetTenant(tenantId);
        item.SetCreator(createdByUserId);
        return item;
    }
}
