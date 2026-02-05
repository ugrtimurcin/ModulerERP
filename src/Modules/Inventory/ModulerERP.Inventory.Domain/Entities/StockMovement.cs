using ModulerERP.SharedKernel.Entities;
using ModulerERP.Inventory.Domain.Enums;

namespace ModulerERP.Inventory.Domain.Entities;

/// <summary>
/// Immutable ledger record for stock changes.
/// Single Source of Truth for inventory history.
/// </summary>
public class StockMovement : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid? LocationId { get; private set; }
    
    public MovementType Type { get; private set; }
    
    /// <summary>Positive for in, negative for out</summary>
    public decimal Quantity { get; private set; }
    
    /// <summary>Reference document type (Invoice, PO, Transfer)</summary>
    public string? ReferenceType { get; private set; }
    
    /// <summary>Reference document ID</summary>
    public Guid? ReferenceId { get; private set; }
    
    /// <summary>Reference document number (e.g., 'INV-2026-001')</summary>
    public string? ReferenceNumber { get; private set; }
    
    /// <summary>Total cost of movement (for FIFO/Avg costing)</summary>
    public decimal? TotalCost { get; private set; }
    
    /// <summary>Unit cost at time of movement</summary>
    public decimal? UnitCost { get; private set; }
    
    /// <summary>Notes/Reason</summary>
    public string? Notes { get; private set; }
    
    /// <summary>Movement date (can differ from CreatedAt)</summary>
    public DateTime MovementDate { get; private set; }

    // Navigation
    public Product? Product { get; private set; }
    public Warehouse? Warehouse { get; private set; }
    public WarehouseLocation? Location { get; private set; }

    private StockMovement() { } // EF Core

    public static StockMovement Create(
        Guid tenantId,
        Guid productId,
        Guid warehouseId,
        MovementType type,
        decimal quantity,
        Guid createdByUserId,
        Guid? locationId = null,
        string? referenceType = null,
        Guid? referenceId = null,
        string? referenceNumber = null,
        decimal? unitCost = null,
        string? notes = null,
        DateTime? movementDate = null)
    {
        var movement = new StockMovement
        {
            ProductId = productId,
            WarehouseId = warehouseId,
            LocationId = locationId,
            Type = type,
            Quantity = quantity,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            ReferenceNumber = referenceNumber,
            UnitCost = unitCost,
            TotalCost = unitCost.HasValue ? unitCost.Value * Math.Abs(quantity) : null,
            Notes = notes,
            MovementDate = movementDate ?? DateTime.UtcNow
        };

        movement.SetTenant(tenantId);
        movement.SetCreator(createdByUserId);
        return movement;
    }
}
