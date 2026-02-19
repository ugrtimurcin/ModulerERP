using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.Sales.Domain.Entities;

namespace ModulerERP.Sales.Infrastructure.Persistence.Configurations;

public class SalesReturnConfiguration : IEntityTypeConfiguration<SalesReturn>
{
    public void Configure(EntityTypeBuilder<SalesReturn> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReturnNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.ReturnNumber).IsUnique();

        builder.Property(x => x.ExchangeRate).HasPrecision(18, 6);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 4);
        builder.Property(x => x.RefundAmount).HasPrecision(18, 4);

        // Dual currency
        builder.Property(x => x.LocalExchangeRate).HasPrecision(18, 6);
        builder.Property(x => x.LocalTotalAmount).HasPrecision(18, 4);
        builder.Property(x => x.LocalRefundAmount).HasPrecision(18, 4);

        // Relationships
        builder.HasMany(x => x.Lines)
               .WithOne(x => x.SalesReturn)
               .HasForeignKey(x => x.SalesReturnId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SalesReturnLineConfiguration : IEntityTypeConfiguration<SalesReturnLine>
{
    public void Configure(EntityTypeBuilder<SalesReturnLine> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 4);
        builder.Property(x => x.LineTotal).HasPrecision(18, 4);
    }
}
