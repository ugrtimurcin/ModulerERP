using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Inventory.Domain.Entities;

/// <summary>
/// Represents a specific value for an attribute (e.g., "Red" for Color, "XL" for Size).
/// </summary>
public class AttributeValue : BaseEntity
{
    public Guid AttributeDefinitionId { get; private set; }
    public string Value { get; private set; } = string.Empty;
    
    /// <summary>Display code (e.g., "RD" for Red)</summary>
    public string? Code { get; private set; }
    
    public int SortOrder { get; private set; }

    // Navigation
    public AttributeDefinition? AttributeDefinition { get; private set; }

    private AttributeValue() { }

    public static AttributeValue Create(
        Guid tenantId,
        Guid attributeDefinitionId,
        string value,
        Guid createdByUserId,
        string? code = null,
        int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value is required", nameof(value));

        var attrValue = new AttributeValue
        {
            AttributeDefinitionId = attributeDefinitionId,
            Value = value,
            Code = code?.ToUpperInvariant(),
            SortOrder = sortOrder
        };

        attrValue.SetTenant(tenantId);
        attrValue.SetCreator(createdByUserId);
        return attrValue;
    }

    public void Update(string value, string? code, int sortOrder)
    {
        Value = value;
        Code = code?.ToUpperInvariant();
        SortOrder = sortOrder;
    }
}
