using Microsoft.EntityFrameworkCore;
using ModulerERP.SystemCore.Application.DTOs;
using ModulerERP.SystemCore.Application.Interfaces;
using ModulerERP.SystemCore.Domain.Entities;
using ModulerERP.SystemCore.Infrastructure.Persistence;

namespace ModulerERP.SystemCore.Infrastructure.Services;

/// <summary>
/// Role management service implementation.
/// </summary>
public class RoleService : IRoleService
{
    private readonly SystemCoreDbContext _context;

    public RoleService(SystemCoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RoleListDto>> GetRolesAsync(Guid tenantId)
    {
        return await _context.Roles.IgnoreQueryFilters()
            .Where(r => r.TenantId == tenantId && !r.IsDeleted)
            .Select(r => new RoleListDto(
                r.Id, r.Name, r.Description, r.IsSystemRole,
                r.RolePermissions.Count()
            ))
            .ToListAsync();
    }

    public async Task<RoleDto?> GetRoleByIdAsync(Guid tenantId, Guid roleId)
    {
        return await _context.Roles.IgnoreQueryFilters()
            .Where(r => r.TenantId == tenantId && r.Id == roleId && !r.IsDeleted)
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Select(r => new RoleDto(
                r.Id, r.Name, r.Description, r.IsSystemRole,
                r.RolePermissions.Where(rp => rp.Permission != null).Select(rp => new PermissionDto(
                    rp.Permission!.Id, rp.Permission.Code, rp.Permission.Description, rp.Permission.ModuleName
                ))
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<RoleDto> CreateRoleAsync(Guid tenantId, CreateRoleDto dto)
    {
        var role = Role.Create(tenantId, dto.Name, dto.Description);
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return new RoleDto(
            role.Id, role.Name, role.Description, role.IsSystemRole,
            Enumerable.Empty<PermissionDto>()
        );
    }

    public async Task AssignPermissionAsync(Guid tenantId, Guid roleId, Guid permissionId)
    {
        var exists = await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (!exists)
        {
            var rolePermission = RolePermission.Create(roleId, permissionId);
            _context.RolePermissions.Add(rolePermission);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemovePermissionAsync(Guid tenantId, Guid roleId, Guid permissionId)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (rolePermission != null)
        {
            _context.RolePermissions.Remove(rolePermission);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AssignRoleToUserAsync(Guid tenantId, Guid userId, Guid roleId)
    {
        var exists = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (!exists)
        {
            var userRole = UserRole.Create(tenantId, userId, roleId);
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveRoleFromUserAsync(Guid tenantId, Guid userId, Guid roleId)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (userRole != null)
        {
            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
        }
    }
}
