using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.Sales.Domain.Entities;

namespace ModulerERP.Sales.Infrastructure.Persistence.Configurations;

public class PriceListConfiguration : IEntityTypeConfiguration<PriceList>
{
    public void Configure(EntityTypeBuilder<PriceList> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasMany(x => x.Items)
               .WithOne(x => x.PriceList)
               .HasForeignKey(x => x.PriceListId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PriceListItemConfiguration : IEntityTypeConfiguration<PriceListItem>
{
    public void Configure(EntityTypeBuilder<PriceListItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Price).HasPrecision(18, 4);
        builder.Property(x => x.MinQuantity).HasPrecision(18, 4);
    }
}
