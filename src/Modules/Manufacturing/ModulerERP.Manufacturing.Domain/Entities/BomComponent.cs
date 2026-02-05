namespace ModulerERP.Manufacturing.Domain.Entities;

/// <summary>
/// BOM component/ingredient.
/// </summary>
public class BomComponent
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BomId { get; private set; }
    
    /// <summary>Component product</summary>
    public Guid ProductId { get; private set; }
    
    public int LineNumber { get; private set; }
    
    /// <summary>Quantity required per parent quantity</summary>
    public decimal Quantity { get; private set; }
    
    public Guid UnitOfMeasureId { get; private set; }
    
    /// <summary>Expected waste percentage</summary>
    public decimal ScrapPercent { get; private set; }
    
    /// <summary>Is this component optional?</summary>
    public bool IsOptional { get; private set; }
    
    public string? Notes { get; private set; }

    // Navigation
    public BillOfMaterials? Bom { get; private set; }

    private BomComponent() { } // EF Core

    public static BomComponent Create(
        Guid bomId,
        Guid productId,
        decimal quantity,
        Guid unitOfMeasureId,
        int lineNumber,
        decimal scrapPercent = 0,
        bool isOptional = false,
        string? notes = null)
    {
        return new BomComponent
        {
            BomId = bomId,
            ProductId = productId,
            Quantity = quantity,
            UnitOfMeasureId = unitOfMeasureId,
            LineNumber = lineNumber,
            ScrapPercent = scrapPercent,
            IsOptional = isOptional,
            Notes = notes
        };
    }

    /// <summary>Get total quantity including scrap</summary>
    public decimal GetTotalQuantity(decimal parentQuantity)
    {
        var baseQty = Quantity * parentQuantity;
        return baseQty * (1 + ScrapPercent / 100);
    }
}
