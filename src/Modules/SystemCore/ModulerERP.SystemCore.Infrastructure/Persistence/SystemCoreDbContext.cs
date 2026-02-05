using Microsoft.EntityFrameworkCore;
using ModulerERP.SystemCore.Domain.Entities;
using ModulerERP.SystemCore.Infrastructure.Persistence.Seeding;
using ModulerERP.SharedKernel.Entities;
using ModulerERP.SharedKernel.Enums;
using ModulerERP.SharedKernel.Interfaces;
using System.Text.Json;

namespace ModulerERP.SystemCore.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for SystemCore module.
/// Implements multi-tenancy via global query filters.
/// </summary>
public class SystemCoreDbContext : DbContext
{
    private readonly Guid _tenantId;
    private readonly ICurrentUserService _currentUserService;

    public SystemCoreDbContext(
        DbContextOptions<SystemCoreDbContext> options, 
        ICurrentUserService currentUserService) 
        : base(options)
    {
        _currentUserService = currentUserService;
        _tenantId = currentUserService.TenantId;
    }

    // Global (non-tenant specific)
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Translation> Translations => Set<Translation>();

    // Tenant Management
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantSetting> TenantSettings => Set<TenantSetting>();
    public DbSet<TenantFeature> TenantFeatures => Set<TenantFeature>();

    // Identity & Security
    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<UserLoginHistory> UserLoginHistories => Set<UserLoginHistory>();

    // Authorization
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    // Centralized Assets
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<Address> Addresses => Set<Address>();

    // Integration
    public DbSet<QueuedJob> QueuedJobs => Set<QueuedJob>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<Webhook> Webhooks => Set<Webhook>();

    // Communication
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // Audit
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema
        modelBuilder.HasDefaultSchema("system_core");

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SystemCoreDbContext).Assembly);

        // Global query filters for multi-tenancy and soft delete
        ApplyGlobalFilters(modelBuilder);
    }

    private void ApplyGlobalFilters(ModelBuilder modelBuilder)
    {
        // Tenant filter for User
        modelBuilder.Entity<User>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        
        // Tenant filter for Role
        modelBuilder.Entity<Role>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        
        // Tenant filter for Tenant (only show own tenant)
        modelBuilder.Entity<Tenant>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);

        // Tenant filter for junction/child tables
        modelBuilder.Entity<UserSession>().HasQueryFilter(e => e.TenantId == _tenantId);
        modelBuilder.Entity<UserRole>().HasQueryFilter(e => e.TenantId == _tenantId);
        modelBuilder.Entity<TenantSetting>().HasQueryFilter(e => e.TenantId == _tenantId);
        modelBuilder.Entity<TenantFeature>().HasQueryFilter(e => e.TenantId == _tenantId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>().ToList();
        var userId = _currentUserService.UserId;
        var tenantId = _currentUserService.TenantId != Guid.Empty ? _currentUserService.TenantId : _tenantId;

        // separating new entries because we need to save first to get their IDs
        var auditEntries = new List<AuditLog>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            // Handle TenantId and Audit fields
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.TenantId == Guid.Empty && tenantId != Guid.Empty)
                {
                    entry.Property(nameof(BaseEntity.TenantId)).CurrentValue = tenantId;
                }
                entry.Entity.SetCreator(userId);
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetUpdater(userId);
            }
            else if (entry.State == EntityState.Deleted)
            {
                // Soft Delete Handling
                if (entry.Entity is ISoftDeletable softDeletable)
                {
                    entry.State = EntityState.Modified;
                    softDeletable.Delete(userId);
                }
            }

            // Prepare Audit Log
            var action = entry.State switch
            {
                EntityState.Added => AuditAction.Insert,
                EntityState.Modified => entry.Entity.IsDeleted ? AuditAction.SoftDelete : AuditAction.Update,
                EntityState.Deleted => AuditAction.HardDelete,
                _ => AuditAction.Update
            };

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();
            var affectedColumns = new List<string>();

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary) continue;

                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey()) continue;

                var originalValue = property.OriginalValue;
                var currentValue = property.CurrentValue;

                if (entry.State == EntityState.Modified)
                {
                    if (!Equals(originalValue, currentValue))
                    {
                        oldValues[propertyName] = originalValue;
                        newValues[propertyName] = currentValue;
                        affectedColumns.Add(propertyName);
                    }
                }
                else if (entry.State == EntityState.Added)
                {
                    newValues[propertyName] = currentValue;
                }
                else if (entry.State == EntityState.Deleted) // Hard delete
                {
                    oldValues[propertyName] = originalValue;
                }
            }

            // Skip if nothing changed in Update
            if (entry.State == EntityState.Modified && affectedColumns.Count == 0 && action != AuditAction.SoftDelete)
                continue;

            var auditLog = AuditLog.Create(
                entry.Entity.TenantId,
                userId,
                entry.Entity.GetType().Name,
                entry.Entity.Id,
                action,
                oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null,
                newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null,
                affectedColumns.Count > 0 ? string.Join(',', affectedColumns) : null
            );

            auditEntries.Add(auditLog);
        }

        // Save changes to generate IDs for new entities
        var result = await base.SaveChangesAsync(cancellationToken);

        // Now save audit logs
        if (auditEntries.Count > 0)
        {
            await AuditLogs.AddRangeAsync(auditEntries, cancellationToken);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}
