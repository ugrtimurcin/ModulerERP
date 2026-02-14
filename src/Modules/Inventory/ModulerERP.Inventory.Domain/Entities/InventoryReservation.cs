using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Inventory.Domain.Entities;

public class InventoryReservation : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public decimal ReservedQuantity { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public Guid? RelatedTaskId { get; private set; }
    
    private InventoryReservation() { }

    public static InventoryReservation Create(
        Guid tenantId,
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        DateTime expiryDate,
        Guid createdByUserId,
        Guid? relatedTaskId = null)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        if (expiryDate <= DateTime.UtcNow) throw new ArgumentException("Expiry date must be in the future", nameof(expiryDate));

        var reservation = new InventoryReservation
        {
            ProductId = productId,
            WarehouseId = warehouseId,
            ReservedQuantity = quantity,
            ExpiryDate = expiryDate,
            RelatedTaskId = relatedTaskId
        };
        
        reservation.SetTenant(tenantId);
        reservation.SetCreator(createdByUserId);
        return reservation;
    }
    

}
