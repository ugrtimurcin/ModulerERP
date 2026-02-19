using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.Sales.Domain.Entities;

namespace ModulerERP.Sales.Infrastructure.Persistence.Configurations;

public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.QuoteNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.QuoteNumber).IsUnique();

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
               .WithOne(x => x.Quote)
               .HasForeignKey(x => x.QuoteId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class QuoteLineConfiguration : IEntityTypeConfiguration<QuoteLine>
{
    public void Configure(EntityTypeBuilder<QuoteLine> builder)
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
        builder.Property(x => x.LineTotal).HasPrecision(18, 4);
    }
}
