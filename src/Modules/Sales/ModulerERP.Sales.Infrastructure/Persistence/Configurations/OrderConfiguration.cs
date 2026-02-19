using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.Sales.Domain.Entities;

namespace ModulerERP.Sales.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.OrderNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.OrderNumber).IsUnique();

        builder.Property(x => x.ExchangeRate).HasPrecision(18, 6);
        builder.Property(x => x.SubTotal).HasPrecision(18, 4);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 4);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 4);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 4);

        // Dual currency
        builder.Property(x => x.LocalExchangeRate).HasPrecision(18, 6);
        builder.Property(x => x.LocalSubTotal).HasPrecision(18, 4);
        builder.Property(x => x.LocalTaxAmount).HasPrecision(18, 4);
        builder.Property(x => x.LocalTotalAmount).HasPrecision(18, 4);

        // Stopaj & Document Discount
        builder.Property(x => x.WithholdingTaxRate).HasPrecision(5, 2);
        builder.Property(x => x.WithholdingTaxAmount).HasPrecision(18, 4);
        builder.Property(x => x.DocumentDiscountRate).HasPrecision(5, 2);
        builder.Property(x => x.DocumentDiscountAmount).HasPrecision(18, 4);

        builder.Property(x => x.ShippingAddress).HasColumnType("jsonb");
        builder.Property(x => x.BillingAddress).HasColumnType("jsonb");

        // Relationships
        builder.HasMany(x => x.Lines)
               .WithOne(x => x.Order)
               .HasForeignKey(x => x.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Shipments)
               .WithOne()
               .HasForeignKey("OrderId")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Invoices)
               .WithOne(x => x.Order)
               .HasForeignKey(x => x.OrderId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Description).HasMaxLength(500);

        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 4);
        builder.Property(x => x.DiscountPercent).HasPrecision(5, 2);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 4);
        builder.Property(x => x.TaxPercent).HasPrecision(5, 2);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 4);
        builder.Property(x => x.ShippedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.InvoicedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.LineTotal).HasPrecision(18, 4);
    }
}
