namespace ModulerERP.SystemCore.Domain.Entities;

/// <summary>
/// User-to-role assignment (N-N relationship).
/// </summary>
public class UserRole
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public Guid TenantId { get; private set; }

    // Navigation
    public User? User { get; private set; }
    public Role? Role { get; private set; }

    private UserRole() { } // EF Core

    public static UserRole Create(Guid tenantId, Guid userId, Guid roleId)
    {
        return new UserRole
        {
            TenantId = tenantId,
            UserId = userId,
            RoleId = roleId
        };
    }
}
