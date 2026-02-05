using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Inventory.Domain.Entities;

/// <summary>
/// Represents a product brand or manufacturer.
/// </summary>
public class Brand : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Website { get; private set; }
    public string? LogoUrl { get; private set; }

    // Navigation
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private Brand() { }

    public static Brand Create(
        Guid tenantId,
        string code,
        string name,
        Guid createdByUserId,
        string? description = null,
        string? website = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        var brand = new Brand
        {
            Code = code.ToUpperInvariant(),
            Name = name,
            Description = description,
            Website = website
        };

        brand.SetTenant(tenantId);
        brand.SetCreator(createdByUserId);
        return brand;
    }

    public void Update(string name, string? description, string? website, string? logoUrl)
    {
        Name = name;
        Description = description;
        Website = website;
        LogoUrl = logoUrl;
    }
}
