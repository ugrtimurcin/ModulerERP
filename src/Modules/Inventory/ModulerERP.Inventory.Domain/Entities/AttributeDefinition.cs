using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Inventory.Domain.Entities;

/// <summary>
/// Defines an attribute type for product variants (e.g., Size, Color, Material).
/// </summary>
public class AttributeDefinition : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public AttributeType Type { get; private set; } = AttributeType.Select;
    public int SortOrder { get; private set; }

    // Navigation
    public ICollection<AttributeValue> Values { get; private set; } = new List<AttributeValue>();

    private AttributeDefinition() { }

    public static AttributeDefinition Create(
        Guid tenantId,
        string code,
        string name,
        AttributeType type,
        Guid createdByUserId,
        int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        var attr = new AttributeDefinition
        {
            Code = code.ToUpperInvariant(),
            Name = name,
            Type = type,
            SortOrder = sortOrder
        };

        attr.SetTenant(tenantId);
        attr.SetCreator(createdByUserId);
        return attr;
    }

    public void Update(string name, int sortOrder)
    {
        Name = name;
        SortOrder = sortOrder;
    }
}

public enum AttributeType
{
    Text = 0,
    Select = 1,
    Number = 2,
    Boolean = 3
}
