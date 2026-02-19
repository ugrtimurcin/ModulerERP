using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.Sales.Domain.Entities;

namespace ModulerERP.Sales.Infrastructure.Persistence.Configurations;

public class CreditNoteConfiguration : IEntityTypeConfiguration<CreditNote>
{
    public void Configure(EntityTypeBuilder<CreditNote> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreditNoteNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.CreditNoteNumber).IsUnique();

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

        // Relationships
        builder.HasMany(x => x.Lines)
               .WithOne(x => x.CreditNote)
               .HasForeignKey(x => x.CreditNoteId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CreditNoteLineConfiguration : IEntityTypeConfiguration<CreditNoteLine>
{
    public void Configure(EntityTypeBuilder<CreditNoteLine> builder)
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
