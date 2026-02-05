using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.CRM.Domain.Entities;

/// <summary>
/// Centralized tag definitions for organizing CRM entities.
/// </summary>
public class Tag : BaseEntity
{
    /// <summary>Tag Name</summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>Hex color code for UI (e.g., '#FF0000')</summary>
    public string ColorCode { get; private set; } = "#3B82F6";
    
    /// <summary>Optional. Restrict tag to specific entity type?</summary>
    public string? EntityType { get; private set; }

    // Navigation
    public ICollection<EntityTag> EntityTags { get; private set; } = new List<EntityTag>();

    private Tag() { } // EF Core

    public static Tag Create(
        Guid tenantId,
        string name,
        Guid createdByUserId,
        string colorCode = "#3B82F6",
        string? entityType = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        var tag = new Tag
        {
            Name = name,
            ColorCode = colorCode,
            EntityType = entityType
        };

        tag.SetTenant(tenantId);
        tag.SetCreator(createdByUserId);
        return tag;
    }

    public void Update(string name, string colorCode, string? entityType = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        
        Name = name;
        ColorCode = colorCode;
        EntityType = entityType;
    }
}
