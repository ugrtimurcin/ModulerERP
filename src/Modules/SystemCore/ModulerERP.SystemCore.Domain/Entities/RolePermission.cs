using ModulerERP.SharedKernel.Enums;

namespace ModulerERP.SystemCore.Domain.Entities;

/// <summary>
/// Maps roles to permissions with data scope level.
/// </summary>
public class RolePermission
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
    
    /// <summary>Data scope: Own, Department, Branch, or Global</summary>
    public DataScope Scope { get; private set; } = DataScope.Own;

    // Navigation
    public Role? Role { get; private set; }
    public Permission? Permission { get; private set; }

    private RolePermission() { } // EF Core

    public static RolePermission Create(Guid roleId, Guid permissionId, DataScope scope = DataScope.Own)
    {
        return new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            Scope = scope
        };
    }

    public void UpdateScope(DataScope scope) => Scope = scope;
}
