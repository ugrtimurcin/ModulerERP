using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.HR.Domain.Entities;

namespace ModulerERP.HR.Infrastructure.Persistence.Configurations;

public class SgkRiskProfileConfiguration : IEntityTypeConfiguration<SgkRiskProfile>
{
    public void Configure(EntityTypeBuilder<SgkRiskProfile> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.EmployerSgkMultiplier).HasPrecision(18, 4);
    }
}
