using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.HR.Domain.Entities;

namespace ModulerERP.HR.Infrastructure.Persistence.Configurations;

public class EarningDeductionTypeConfiguration : IEntityTypeConfiguration<EarningDeductionType>
{
    public void Configure(EntityTypeBuilder<EarningDeductionType> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.MaxExemptAmount).HasPrecision(18, 2);
    }
}
