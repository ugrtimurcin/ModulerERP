using Microsoft.EntityFrameworkCore;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.SharedKernel.Entities;
using ModulerERP.SharedKernel.Enums;
using ModulerERP.SharedKernel.Interfaces;
using System.Text.Json;

namespace ModulerERP.CRM.Infrastructure.Persistence;

/// <summary>
/// DbContext for CRM module entities.
/// </summary>
public class CRMDbContext : DbContext
{
    public DbSet<BusinessPartner> BusinessPartners => Set<BusinessPartner>();
    public DbSet<BusinessPartnerGroup> BusinessPartnerGroups => Set<BusinessPartnerGroup>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<SaleAgent> SaleAgents => Set<SaleAgent>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<TicketMessage> TicketMessages => Set<TicketMessage>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<EntityTag> EntityTags => Set<EntityTag>();
    public DbSet<PartnerBalance> PartnerBalances => Set<PartnerBalance>();
    
    // Shared Audit Log (mapped to system_core schema)
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    private readonly ICurrentUserService _currentUserService;

    public CRMDbContext(
        DbContextOptions<CRMDbContext> options,
        ICurrentUserService currentUserService) 
        : base(options) 
    {
        _currentUserService = currentUserService;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Set default schema
        modelBuilder.HasDefaultSchema("crm");

        // Map AuditLog to system_core table
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs", "core", t => t.ExcludeFromMigrations());
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OldValues).HasColumnType("jsonb");
            entity.Property(e => e.NewValues).HasColumnType("jsonb");
        });
        
        // BusinessPartner
        modelBuilder.Entity<BusinessPartner>(entity =>
        {
            entity.ToTable("BusinessPartners");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();
            entity.HasIndex(e => e.TenantId);
            
            entity.HasOne(e => e.Group)
                .WithMany()
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.ParentPartner)
                .WithMany(e => e.ChildPartners)
                .HasForeignKey(e => e.ParentPartnerId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        
        // BusinessPartnerGroup
        modelBuilder.Entity<BusinessPartnerGroup>(entity =>
        {
            entity.ToTable("BusinessPartnerGroups");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        
        // Contact
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.ToTable("Contacts");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Partner)
                .WithMany(e => e.Contacts)
                .HasForeignKey(e => e.PartnerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        
        // Lead
        modelBuilder.Entity<Lead>(entity =>
        {
            entity.ToTable("Leads");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        
        // Opportunity
        modelBuilder.Entity<Opportunity>(entity =>
        {
            entity.ToTable("Opportunities");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        
        // Activity
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.ToTable("Activities");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        
        // SupportTicket
        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.ToTable("SupportTickets");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        
        // PartnerBalance
        modelBuilder.Entity<PartnerBalance>(entity =>
        {
            entity.ToTable("PartnerBalances");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.PartnerId, e.CurrencyId }).IsUnique();
            entity.HasOne(e => e.Partner)
                .WithMany(e => e.Balances)
                .HasForeignKey(e => e.PartnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // TicketMessage
        modelBuilder.Entity<TicketMessage>(entity =>
        {
            entity.ToTable("TicketMessages");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Ticket)
                .WithMany()
                .HasForeignKey(e => e.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        
        // Tag
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("Tags");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        
        // EntityTag
        modelBuilder.Entity<EntityTag>(entity =>
        {
            entity.ToTable("EntityTags");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.TagId, e.EntityId, e.EntityType }).IsUnique();
            entity.HasOne(e => e.Tag)
                .WithMany(e => e.EntityTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _currentTransaction;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null) return;
        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try 
        {
            await SaveChangesAsync(cancellationToken);
            
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>().ToList();
        var userId = _currentUserService.UserId;
        var tenantId = _currentUserService.TenantId;

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
                else if (entry.State == EntityState.Deleted)
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
