using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.Sales.Domain.Entities;

namespace ModulerERP.Sales.Infrastructure.Persistence.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.ShipmentNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.ShipmentNumber).IsUnique();

        builder.Property(x => x.ShippingAddress).HasColumnType("jsonb");
        
        // İrsaliye fields
        builder.Property(x => x.WaybillNumber).HasMaxLength(50);
        builder.Property(x => x.DriverName).HasMaxLength(200);
        builder.Property(x => x.VehiclePlate).HasMaxLength(20);
        
        // Relationships
        builder.HasMany(x => x.Lines)
               .WithOne(x => x.Shipment)
               .HasForeignKey(x => x.ShipmentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ShipmentLineConfiguration : IEntityTypeConfiguration<ShipmentLine>
{
    public void Configure(EntityTypeBuilder<ShipmentLine> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Quantity).HasPrecision(18, 4);
    }
}
