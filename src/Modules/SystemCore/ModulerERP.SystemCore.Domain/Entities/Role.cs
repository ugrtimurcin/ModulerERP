using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.SystemCore.Domain.Entities;

/// <summary>
/// Technical access profile (e.g., 'Finance_Viewer').
/// NOT HR Job Titles - strictly for authorization.
/// </summary>
public class Role : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    
    /// <summary>Protected from deletion (e.g., 'Admin')</summary>
    public bool IsSystemRole { get; private set; }
    
    /// <summary>For role inheritance - child role inherits parent permissions</summary>
    public Guid? ParentRoleId { get; private set; }

    /// <summary>
    /// Stored as JSON: ["stock.view", "invoice.create"]
    /// </summary>
    public List<string> Permissions { get; set; } = new();

    // Navigation
    public Role? ParentRole { get; private set; }
    public ICollection<Role> ChildRoles { get; private set; } = new List<Role>();

    private Role() { } // EF Core

    public static Role Create(Guid tenantId, string name, string? description = null, bool isSystemRole = false, Guid? parentRoleId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required", nameof(name));

        var role = new Role
        {
            Name = name,
            Description = description,
            IsSystemRole = isSystemRole,
            ParentRoleId = parentRoleId
        };
        
        role.SetTenant(tenantId);
        return role;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public void SetParentRole(Guid? parentRoleId)
    {
        ParentRoleId = parentRoleId;
    }
}
