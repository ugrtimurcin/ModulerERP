using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Inventory.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    
    // JSONB storage for attributes (e.g., {"Color": "Red", "Size": "XL"})
    public string Attributes { get; private set; } = "{}";
    
    public Guid? ImageId { get; private set; }

    private ProductVariant() { }

    public static ProductVariant Create(
        Guid tenantId,
        Guid productId,
        string code,
        string name,
        string attributes,
        Guid createdByUserId)
    {
        var variant = new ProductVariant
        {
            ProductId = productId,
            Code = code,
            Name = name,
            Attributes = attributes
        };
        
        variant.SetTenant(tenantId);
        variant.SetCreator(createdByUserId);
        
        return variant;
    }

    public void Update(string name, string attributes)
    {
        Name = name;
        Attributes = attributes;
    }
}
