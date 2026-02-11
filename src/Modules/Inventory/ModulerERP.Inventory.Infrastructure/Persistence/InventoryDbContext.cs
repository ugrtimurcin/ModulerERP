using Microsoft.EntityFrameworkCore;
using ModulerERP.Inventory.Domain.Entities;
using ModulerERP.SharedKernel.Entities;
using ModulerERP.SharedKernel.Enums;
using ModulerERP.SharedKernel.Interfaces;
using System.Text.Json;

namespace ModulerERP.Inventory.Infrastructure.Persistence;

/// <summary>
/// DbContext for Inventory module entities.
/// </summary>
public class InventoryDbContext : DbContext, IUnitOfWork
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductBarcode> ProductBarcodes => Set<ProductBarcode>();
    public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();
    public DbSet<UnitOfMeasure> UnitOfMeasures => Set<UnitOfMeasure>();
    
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseLocation> WarehouseLocations => Set<WarehouseLocation>();
    
    public DbSet<StockLevel> StockLevels => Set<StockLevel>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();
    public DbSet<StockTransferLine> StockTransferLines => Set<StockTransferLine>();
    
    // New entities
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<UnitConversion> UnitConversions => Set<UnitConversion>();
    public DbSet<ProductSerial> ProductSerials => Set<ProductSerial>();
    public DbSet<ProductBatch> ProductBatches => Set<ProductBatch>();
    public DbSet<AttributeDefinition> AttributeDefinitions => Set<AttributeDefinition>();
    public DbSet<AttributeValue> AttributeValues => Set<AttributeValue>();

    // Shared Audit Log (mapped to system_core schema)
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    private readonly ICurrentUserService _currentUserService;

    public InventoryDbContext(
        DbContextOptions<InventoryDbContext> options,
        ICurrentUserService currentUserService) 
        : base(options) 
    {
        _currentUserService = currentUserService;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Set default schema
        modelBuilder.HasDefaultSchema("inventory");

        // Map AuditLog to system_core table
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs", "core", t => t.ExcludeFromMigrations());
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OldValues).HasColumnType("jsonb");
            entity.Property(e => e.NewValues).HasColumnType("jsonb");
        });
        
        // Apply configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
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
