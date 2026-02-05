using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Sales.Domain.Entities;

public class PriceList : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    
    public Guid CurrencyId { get; set; }
    
    public bool IsInternal { get; set; }

    // Navigation property for items can be added if needed, 
    // but usually queried separately for performance.
}
