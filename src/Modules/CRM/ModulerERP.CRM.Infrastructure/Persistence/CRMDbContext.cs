using Microsoft.EntityFrameworkCore;
using ModulerERP.SharedKernel.Entities;
using ModulerERP.SharedKernel.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.CRM.Application.Interfaces;
using System.Text.Json;

namespace ModulerERP.CRM.Infrastructure.Persistence;

public class CRMDbContext : DbContext, ICRMUnitOfWork
{
    private readonly ICurrentUserService _currentUserService;

    public CRMDbContext(DbContextOptions<CRMDbContext> options, ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    // Core CRM entities
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<BusinessPartner> BusinessPartners => Set<BusinessPartner>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<TicketMessage> TicketMessages => Set<TicketMessage>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<EntityTag> EntityTags => Set<EntityTag>();

    // Sales & Commission entities
    public DbSet<SaleAgent> SaleAgents => Set<SaleAgent>();

    // Partner management
    public DbSet<BusinessPartnerGroup> BusinessPartnerGroups => Set<BusinessPartnerGroup>();

    // Lookups & Dimensions
    public DbSet<Territory> Territories => Set<Territory>();
    public DbSet<Competitor> Competitors => Set<Competitor>();
    public DbSet<RejectionReason> RejectionReasons => Set<RejectionReason>();

    // Shared Audit Log (mapped to system_core schema)
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // EF Core caches the model. For global query filters to work dynamically,
    // the filter expression must close over a field/property that changes per-request.
    // A local variable captured at OnModelCreating time would freeze the value.
    private Guid _tenantId => _currentUserService.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("crm");

        // Map AuditLog to system_core table
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs", "core", t => t.ExcludeFromMigrations());
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OldValues).HasColumnType("jsonb");
            entity.Property(e => e.NewValues).HasColumnType("jsonb");
        });

        // ── Global Query Filters: Soft Delete + Tenant Isolation ──
        // Referencing _tenantId (a property) ensures EF evaluates it per-query
        modelBuilder.Entity<Lead>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<Opportunity>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<BusinessPartner>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<Contact>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<Activity>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<SupportTicket>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<TicketMessage>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<Tag>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<EntityTag>().HasQueryFilter(e => e.TenantId == _tenantId);
        modelBuilder.Entity<SaleAgent>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<BusinessPartnerGroup>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        
        modelBuilder.Entity<Territory>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<Competitor>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<RejectionReason>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CRMDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>().ToList();
        var userId = _currentUserService.UserId;
        var tenantId = _currentUserService.TenantId;

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
                    entry.Entity.SetTenant(tenantId);
                }
                entry.Entity.SetCreator(userId);
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetUpdater(userId);
            }
            else if (entry.State == EntityState.Deleted)
            {
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
                if (property.Metadata.IsPrimaryKey()) continue;

                var propertyName = property.Metadata.Name;
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
                else if (entry.State == EntityState.Deleted)
                {
                    oldValues[propertyName] = originalValue;
                }
            }

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

        var result = await base.SaveChangesAsync(cancellationToken);

        if (auditEntries.Count > 0)
        {
            await AuditLogs.AddRangeAsync(auditEntries, cancellationToken);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    // ICRMUnitOfWork / IUnitOfWork implementation
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.RollbackTransactionAsync(cancellationToken);
    }
}
