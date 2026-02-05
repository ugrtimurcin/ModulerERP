using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Inventory.Domain.Entities;

/// <summary>
/// Hierarchical product categorization.
/// </summary>
public class ProductCategory : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    
    /// <summary>For nested categories</summary>
    public Guid? ParentCategoryId { get; private set; }
    
    /// <summary>Display order</summary>
    public int SortOrder { get; private set; }

    // Navigation
    public ProductCategory? ParentCategory { get; private set; }
    public ICollection<ProductCategory> ChildCategories { get; private set; } = new List<ProductCategory>();
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private ProductCategory() { } // EF Core

    public static ProductCategory Create(
        Guid tenantId,
        string name,
        Guid createdByUserId,
        string? description = null,
        Guid? parentCategoryId = null,
        int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        var category = new ProductCategory
        {
            Name = name,
            Description = description,
            ParentCategoryId = parentCategoryId,
            SortOrder = sortOrder
        };

        category.SetTenant(tenantId);
        category.SetCreator(createdByUserId);
        return category;
    }

    public void Update(string name, string? description, int sortOrder)
    {
        Name = name;
        Description = description;
        SortOrder = sortOrder;
    }

    public void SetParentCategory(Guid? parentCategoryId) => ParentCategoryId = parentCategoryId;
}
