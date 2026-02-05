using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.Inventory.Domain.Entities;

namespace ModulerERP.Inventory.Infrastructure.Persistence.Configurations;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);
        
        builder.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
    }
}

public class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("UnitOfMeasures");
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);
        
        builder.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();
    }
}
