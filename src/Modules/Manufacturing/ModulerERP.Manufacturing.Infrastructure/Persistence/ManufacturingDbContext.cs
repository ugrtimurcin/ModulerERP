using Microsoft.EntityFrameworkCore;
using ModulerERP.Manufacturing.Domain.Entities;
using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Manufacturing.Infrastructure.Persistence;

public class ManufacturingDbContext : DbContext
{
    public DbSet<BillOfMaterials> BillOfMaterials => Set<BillOfMaterials>();
    public DbSet<BomComponent> BomComponents => Set<BomComponent>();
    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();
    public DbSet<ProductionOrderLine> ProductionOrderLines => Set<ProductionOrderLine>();
    public DbSet<ProductionRun> ProductionRuns => Set<ProductionRun>();
    public DbSet<ProductionRunItem> ProductionRunItems => Set<ProductionRunItem>();

    public ManufacturingDbContext(DbContextOptions<ManufacturingDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("manufacturing");

        // BillOfMaterials
        modelBuilder.Entity<BillOfMaterials>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.ProductId });
            entity.HasQueryFilter(e => !e.IsDeleted);
            
            entity.HasMany(e => e.Components)
                .WithOne(c => c.Bom)
                .HasForeignKey(c => c.BomId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // BomComponent (NOT BaseEntity)
        modelBuilder.Entity<BomComponent>(entity =>
        {
            entity.HasKey(e => e.Id);
            // No query filter - not a BaseEntity
        });

        // ProductionOrder
        modelBuilder.Entity<ProductionOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.OrderNumber }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.Status });
            entity.HasQueryFilter(e => !e.IsDeleted);
            
            entity.HasOne(e => e.Bom)
                .WithMany()
                .HasForeignKey(e => e.BomId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.ProductionOrder)
                .HasForeignKey(l => l.ProductionOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProductionOrderLine (NOT BaseEntity)
        modelBuilder.Entity<ProductionOrderLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            // No query filter - not a BaseEntity
        });

        // ProductionRun
        modelBuilder.Entity<ProductionRun>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProductionOrderId);
            entity.HasQueryFilter(e => !e.IsDeleted);
            
            entity.HasOne(e => e.ProductionOrder)
                .WithMany()
                .HasForeignKey(e => e.ProductionOrderId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(e => e.Items)
                .WithOne(i => i.ProductionRun)
                .HasForeignKey(i => i.ProductionRunId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProductionRunItem
        modelBuilder.Entity<ProductionRunItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
