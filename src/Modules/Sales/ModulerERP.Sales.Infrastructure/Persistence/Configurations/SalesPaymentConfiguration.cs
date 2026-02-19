using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.Sales.Domain.Entities;

namespace ModulerERP.Sales.Infrastructure.Persistence.Configurations;

public class SalesPaymentConfiguration : IEntityTypeConfiguration<SalesPayment>
{
    public void Configure(EntityTypeBuilder<SalesPayment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AllocatedAmount).HasPrecision(18, 4);
        builder.Property(x => x.PaymentMethod).HasMaxLength(100);
        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
    }
}
