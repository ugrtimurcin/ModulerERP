namespace ModulerERP.CRM.Domain.Entities;

/// <summary>
/// Polymorphic tagging system - maps tags to any entity type.
/// Note: This is a simple junction table without BaseEntity inheritance.
/// </summary>
public class EntityTag
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    
    /// <summary>Multi-tenancy key</summary>
    public Guid TenantId { get; private set; }
    
    /// <summary>FK to Tags</summary>
    public Guid TagId { get; private set; }
    
    /// <summary>ID of Partner, Lead, Ticket, etc.</summary>
    public Guid EntityId { get; private set; }
    
    /// <summary>'Partner', 'Lead', 'Ticket'</summary>
    public string EntityType { get; private set; } = string.Empty;
    
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public Tag? Tag { get; private set; }

    private EntityTag() { } // EF Core

    public static EntityTag Create(
        Guid tenantId,
        Guid tagId,
        Guid entityId,
        string entityType)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("EntityType is required", nameof(entityType));

        return new EntityTag
        {
            TenantId = tenantId,
            TagId = tagId,
            EntityId = entityId,
            EntityType = entityType
        };
    }
}
