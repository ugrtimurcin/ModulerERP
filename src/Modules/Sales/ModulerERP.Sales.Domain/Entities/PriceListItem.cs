using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Sales.Domain.Entities;

public class PriceListItem : BaseEntity
{
    public Guid PriceListId { get; set; }
    public PriceList PriceList { get; set; } = null!;
    
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public Guid UnitId { get; set; }
    
    public decimal Price { get; set; }
}
