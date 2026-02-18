using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.HR.Domain.Entities;

namespace ModulerERP.HR.Infrastructure.Persistence.Configurations;

public class TaxRuleConfiguration : IEntityTypeConfiguration<TaxRule>
{
    public void Configure(EntityTypeBuilder<TaxRule> builder)
    {
        builder.ToTable("TaxRules", "hr");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LowerLimit).HasPrecision(18, 2);
        builder.Property(e => e.UpperLimit).HasPrecision(18, 2);
        builder.Property(e => e.Rate).HasPrecision(5, 4); // 0.10, 0.3700
    }
}
