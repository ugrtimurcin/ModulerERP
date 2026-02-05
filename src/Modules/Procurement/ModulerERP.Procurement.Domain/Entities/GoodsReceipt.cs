using ModulerERP.SharedKernel.Entities;
using ModulerERP.Procurement.Domain.Enums;

namespace ModulerERP.Procurement.Domain.Entities;

/// <summary>
/// Document goods received against a PO.
/// Triggers stock movements.
/// </summary>
public class GoodsReceipt : BaseEntity
{
    /// <summary>Receipt number (e.g., 'GRN-2026-001')</summary>
    public string ReceiptNumber { get; private set; } = string.Empty;
    
    public Guid PurchaseOrderId { get; private set; }
    public Guid WarehouseId { get; private set; }
    
    public ReceiptStatus Status { get; private set; } = ReceiptStatus.Pending;
    
    public DateTime ReceiptDate { get; private set; }
    
    /// <summary>Supplier's delivery note number</summary>
    public string? SupplierDeliveryNote { get; private set; }
    
    public string? Notes { get; private set; }

    // Navigation
    public PurchaseOrder? PurchaseOrder { get; private set; }
    public ICollection<GoodsReceiptLine> Lines { get; private set; } = new List<GoodsReceiptLine>();

    private GoodsReceipt() { } // EF Core

    public static GoodsReceipt Create(
        Guid tenantId,
        string receiptNumber,
        Guid purchaseOrderId,
        Guid warehouseId,
        DateTime receiptDate,
        Guid createdByUserId,
        string? supplierDeliveryNote = null)
    {
        var receipt = new GoodsReceipt
        {
            ReceiptNumber = receiptNumber,
            PurchaseOrderId = purchaseOrderId,
            WarehouseId = warehouseId,
            ReceiptDate = receiptDate,
            SupplierDeliveryNote = supplierDeliveryNote
        };

        receipt.SetTenant(tenantId);
        receipt.SetCreator(createdByUserId);
        return receipt;
    }

    public void MarkReceived() => Status = ReceiptStatus.Received;
    public void MarkQualityChecked() => Status = ReceiptStatus.QualityChecked;
}
