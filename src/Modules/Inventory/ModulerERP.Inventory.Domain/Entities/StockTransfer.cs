using ModulerERP.SharedKernel.Entities;
using ModulerERP.Inventory.Domain.Enums;

namespace ModulerERP.Inventory.Domain.Entities;

/// <summary>
/// Inter-warehouse stock transfer header.
/// </summary>
public class StockTransfer : BaseEntity
{
    /// <summary>Transfer number (e.g., 'TRF-2026-001')</summary>
    public string TransferNumber { get; private set; } = string.Empty;
    
    public Guid SourceWarehouseId { get; private set; }
    public Guid DestinationWarehouseId { get; private set; }
    
    public TransferStatus Status { get; private set; } = TransferStatus.Pending;
    
    public DateTime? ShippedDate { get; private set; }
    public DateTime? ReceivedDate { get; private set; }
    
    public string? Notes { get; private set; }

    // Navigation
    public Warehouse? SourceWarehouse { get; private set; }
    public Warehouse? Destination { get; private set; }
    public ICollection<StockTransferLine> Lines { get; private set; } = new List<StockTransferLine>();

    private StockTransfer() { } // EF Core

    public static StockTransfer Create(
        Guid tenantId,
        string transferNumber,
        Guid sourceWarehouseId,
        Guid destinationWarehouseId,
        Guid createdByUserId,
        string? notes = null)
    {
        if (sourceWarehouseId == destinationWarehouseId)
            throw new ArgumentException("Source and destination warehouses must be different");

        var transfer = new StockTransfer
        {
            TransferNumber = transferNumber,
            SourceWarehouseId = sourceWarehouseId,
            DestinationWarehouseId = destinationWarehouseId,
            Notes = notes
        };

        transfer.SetTenant(tenantId);
        transfer.SetCreator(createdByUserId);
        return transfer;
    }

    public void MarkAsShipped()
    {
        Status = TransferStatus.InTransit;
        ShippedDate = DateTime.UtcNow;
    }

    public void MarkAsReceived()
    {
        Status = TransferStatus.Completed;
        ReceivedDate = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == TransferStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed transfer");
        
        Status = TransferStatus.Cancelled;
    }
}
