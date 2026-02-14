using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.Inventory.Domain.Entities;

namespace ModulerERP.Inventory.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        
        builder.HasKey(e => e.Id);
        
        // Tenant + SKU must be unique
        builder.HasIndex(e => new { e.TenantId, e.Sku }).IsUnique();
        
        builder.Property(e => e.Sku).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        
        builder.HasOne(e => e.UnitOfMeasure)
            .WithMany()
            .HasForeignKey(e => e.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ParentProduct)
            .WithMany(e => e.Variants)
            .HasForeignKey(e => e.ParentProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Money Value Objects
        builder.OwnsOne(e => e.PurchasePrice, money =>
        {
            money.Property(p => p.Amount).HasColumnName("PurchasePriceAmount").HasPrecision(18, 2);
            money.Property(p => p.CurrencyCode).HasColumnName("PurchasePriceCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(e => e.SalesPrice, money =>
        {
            money.Property(p => p.Amount).HasColumnName("SalesPriceAmount").HasPrecision(18, 2);
            money.Property(p => p.CurrencyCode).HasColumnName("SalesPriceCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(e => e.CostPrice, money =>
        {
            money.Property(p => p.Amount).HasColumnName("CostPriceAmount").HasPrecision(18, 2);
            money.Property(p => p.CurrencyCode).HasColumnName("CostPriceCurrency").HasMaxLength(3);
        });

        // Soft delete filter
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}

public class ProductBarcodeConfiguration : IEntityTypeConfiguration<ProductBarcode>
{
    public void Configure(EntityTypeBuilder<ProductBarcode> builder)
    {
        builder.ToTable("ProductBarcodes");
        builder.HasKey(e => e.Id);
        
        // Barcode unique per tenant
        builder.HasIndex(e => new { e.TenantId, e.Barcode }).IsUnique();
        
        builder.HasOne(e => e.Product)
            .WithMany(e => e.Barcodes)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // No soft delete
    }
}

public class ProductPriceConfiguration : IEntityTypeConfiguration<ProductPrice>
{
    public void Configure(EntityTypeBuilder<ProductPrice> builder)
    {
        builder.ToTable("ProductPrices");
        builder.HasKey(e => e.Id);
        
        // One price list entry per product/currency/type
        builder.HasIndex(e => new { e.TenantId, e.ProductId, e.CurrencyId, e.PriceType }).IsUnique();

        builder.HasOne(e => e.Product)
            .WithMany(e => e.Prices)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // No soft delete
    }
}
