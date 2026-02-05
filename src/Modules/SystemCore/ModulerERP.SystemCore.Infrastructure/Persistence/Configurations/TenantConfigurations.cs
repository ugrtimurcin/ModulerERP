using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.SystemCore.Domain.Entities;

namespace ModulerERP.SystemCore.Infrastructure.Persistence.Configurations;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Currencies");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(3);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.Symbol)
            .IsRequired()
            .HasMaxLength(5);
        
        builder.HasIndex(e => e.Code).IsUnique();
    }
}

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(e => e.Subdomain)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(e => e.DbSchema)
            .HasMaxLength(50);
        
        builder.Property(e => e.SubscriptionPlan)
            .HasMaxLength(50);
        
        builder.Property(e => e.TimeZone)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Europe/Nicosia");
        
        builder.HasIndex(e => e.Subdomain).IsUnique();
        
        builder.HasOne(e => e.BaseCurrency)
            .WithMany()
            .HasForeignKey(e => e.BaseCurrencyId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(e => e.Settings)
            .WithOne(s => s.Tenant)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(e => e.Features)
            .WithOne(f => f.Tenant)
            .HasForeignKey(f => f.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TenantSettingConfiguration : IEntityTypeConfiguration<TenantSetting>
{
    public void Configure(EntityTypeBuilder<TenantSetting> builder)
    {
        builder.ToTable("TenantSettings");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Key)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.Value)
            .IsRequired();
        
        builder.Property(e => e.DataType)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("String");
        
        builder.HasIndex(e => new { e.TenantId, e.Key }).IsUnique();
    }
}

public class TenantFeatureConfiguration : IEntityTypeConfiguration<TenantFeature>
{
    public void Configure(EntityTypeBuilder<TenantFeature> builder)
    {
        builder.ToTable("TenantFeatures");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.FeatureCode)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(e => new { e.TenantId, e.FeatureCode }).IsUnique();
    }
}
