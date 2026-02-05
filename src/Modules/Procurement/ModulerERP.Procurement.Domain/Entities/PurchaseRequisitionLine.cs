namespace ModulerERP.Procurement.Domain.Entities;

/// <summary>
/// Purchase requisition line items.
/// </summary>
public class PurchaseRequisitionLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid RequisitionId { get; private set; }
    public Guid ProductId { get; private set; }
    
    public decimal Quantity { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    
    /// <summary>Estimated unit price</summary>
    public decimal EstimatedPrice { get; private set; }
    
    /// <summary>Preferred supplier if any</summary>
    public Guid? PreferredSupplierId { get; private set; }
    
    public string? Notes { get; private set; }

    // Navigation
    public PurchaseRequisition? Requisition { get; private set; }

    private PurchaseRequisitionLine() { } // EF Core

    public static PurchaseRequisitionLine Create(
        Guid requisitionId,
        Guid productId,
        decimal quantity,
        Guid unitOfMeasureId,
        decimal estimatedPrice = 0,
        Guid? preferredSupplierId = null,
        string? notes = null)
    {
        return new PurchaseRequisitionLine
        {
            RequisitionId = requisitionId,
            ProductId = productId,
            Quantity = quantity,
            UnitOfMeasureId = unitOfMeasureId,
            EstimatedPrice = estimatedPrice,
            PreferredSupplierId = preferredSupplierId,
            Notes = notes
        };
    }
}
