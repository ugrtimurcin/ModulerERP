using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.Inventory.Domain.Entities;

namespace ModulerERP.Inventory.Infrastructure.Persistence.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);
        
        builder.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();
    }
}

public class WarehouseLocationConfiguration : IEntityTypeConfiguration<WarehouseLocation>
{
    public void Configure(EntityTypeBuilder<WarehouseLocation> builder)
    {
        builder.ToTable("WarehouseLocations");
        builder.HasKey(e => e.Id);
        
        builder.HasIndex(e => new { e.TenantId, e.WarehouseId, e.Code }).IsUnique();

        builder.HasOne(e => e.Warehouse)
            .WithMany(e => e.Locations)
            .HasForeignKey(e => e.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
