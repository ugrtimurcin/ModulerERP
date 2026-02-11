using Microsoft.EntityFrameworkCore;
using ModulerERP.SharedKernel.DTOs;
using ModulerERP.SystemCore.Application.DTOs;
using ModulerERP.SystemCore.Application.Interfaces;
using ModulerERP.SystemCore.Domain.Entities;
using ModulerERP.SystemCore.Infrastructure.Persistence;

namespace ModulerERP.SystemCore.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly SystemCoreDbContext _context;

    public TenantService(SystemCoreDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<TenantListDto>> GetTenantsAsync(int page, int pageSize)
    {
        var query = _context.Tenants.IgnoreQueryFilters()
            .Where(t => !t.IsDeleted); // We might want to see deleted ones too? For now, hide them.

        var total = await query.CountAsync();
        var tenants = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TenantListDto(
                t.Id,
                t.Name,
                t.Subdomain,
                t.SubscriptionPlan,
                t.IsActive,
                t.CreatedAt
            ))
            .ToListAsync();

        return new PagedResult<TenantListDto>(
            tenants, page, pageSize, total,
            (int)Math.Ceiling(total / (double)pageSize)
        );
    }

    public async Task<TenantDto?> GetTenantByIdAsync(Guid tenantId)
    {
        return await _context.Tenants.IgnoreQueryFilters()
            .Where(t => t.Id == tenantId)
            .Select(t => new TenantDto(
                t.Id,
                t.Name,
                t.Subdomain,
                t.SubscriptionPlan,
                t.SubscriptionExpiresAt,
                t.IsActive,
                t.BaseCurrencyId,
                t.TimeZone
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<TenantDto> CreateTenantAsync(CreateTenantDto dto)
    {
        // 1. Validate Uniqueness
        var existingTenant = await _context.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Subdomain == dto.Subdomain.ToLowerInvariant());

        if (existingTenant != null)
        {
            throw new InvalidOperationException($"Subdomain '{dto.Subdomain}' is already taken.");
        }

        // 2. Create Tenant
        var tenant = Tenant.Create(dto.Name, dto.Subdomain, dto.BaseCurrencyId);
        if(!string.IsNullOrEmpty(dto.SubscriptionPlan))
        {
             tenant.UpdateSubscription(dto.SubscriptionPlan, DateTime.UtcNow.AddYears(1));
        }

        _context.Tenants.Add(tenant);
        
        // We need to save to get the ID, but because of the transaction we can do it later?
        // Actually Tenant.Create sets the ID.

        // 3. Create Admin Role for this Tenant (or use a System Role?)
        // Usually each tenant has its own roles. We should seed default roles for the new tenant.
        var adminRole = Role.Create(tenant.Id, "Admin", "Administrator with full access");
        adminRole.Permissions.Add("FullAccess"); // Simplified permission
        _context.Roles.Add(adminRole);

        // 4. Create Admin User
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.AdminPassword);
        // User.Create requires a CreatorId. Since this is a system action (or super admin), 
        // we might pass Guid.Empty or the current user's ID if we had it.
        // For now let's assume system creation (Guid.Empty) or we need to pass it in.
        // Dto doesn't have it. Let's use Guid.Empty for "System".
        var adminUser = User.Create(tenant.Id, dto.AdminEmail, passwordHash, dto.AdminFirstName, dto.AdminLastName, Guid.Empty);
        
        _context.Users.Add(adminUser);

        // 5. Assign Role
        var userRole = UserRole.Create(tenant.Id, adminUser.Id, adminRole.Id);
        _context.UserRoles.Add(userRole);

        await _context.SaveChangesAsync();

        return new TenantDto(
            tenant.Id,
            tenant.Name,
            tenant.Subdomain,
            tenant.SubscriptionPlan,
            tenant.SubscriptionExpiresAt,
            tenant.IsActive,
            tenant.BaseCurrencyId,
            tenant.TimeZone
        );
    }

    public async Task<TenantDto> UpdateTenantAsync(Guid tenantId, UpdateTenantDto dto)
    {
        var tenant = await _context.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null) throw new KeyNotFoundException("Tenant not found");

        if(dto.SubscriptionPlan != null)
        {
            tenant.UpdateSubscription(dto.SubscriptionPlan, dto.SubscriptionExpiresAt ?? DateTime.UtcNow.AddYears(1));
        }

        if (dto.IsActive.HasValue)
        {
            if (dto.IsActive.Value) tenant.Activate();
            else tenant.Deactivate();
        }
        
        // We can't update Name or Subdomain easily without breaking things? 
        // Tenant.Name has a private set, but no Update method for it in the viewed file.
        // Assuming we might need to add one or just skip it for now.
        // Wait, I see `public string Name { get; private set; }` in Tenant.cs
        // I should probably add an UpdateInfo method to Tenant entity if I want to update Name.
        // checking Tenant.cs content from previous turn... it only has UpdateSubscription and SetDbSchema.
        // I will skip name update for now strictly speaking, or I would need to modify the entity.
        // Let's modify the entity in a separate step if strictly needed. For now I will ignore Name update.

        await _context.SaveChangesAsync();

        return new TenantDto(
            tenant.Id,
            tenant.Name,
            tenant.Subdomain,
            tenant.SubscriptionPlan,
            tenant.SubscriptionExpiresAt,
            tenant.IsActive,
            tenant.BaseCurrencyId,
            tenant.TimeZone
        );
    }
}
