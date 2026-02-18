using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.HR.Domain.Entities;

namespace ModulerERP.HR.Infrastructure.Persistence.Configurations;

public class SocialSecurityRuleConfiguration : IEntityTypeConfiguration<SocialSecurityRule>
{
    public void Configure(EntityTypeBuilder<SocialSecurityRule> builder)
    {
        builder.ToTable("SocialSecurityRules", "hr");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.CitizenshipType).HasConversion<string>();
        builder.Property(e => e.SocialSecurityType).HasConversion<string>();
        
        builder.Property(e => e.EmployeeDeductionRate).HasPrecision(5, 4);
        builder.Property(e => e.EmployerDeductionRate).HasPrecision(5, 4);
        builder.Property(e => e.ProvidentFundEmployeeRate).HasPrecision(5, 4);
        builder.Property(e => e.ProvidentFundEmployerRate).HasPrecision(5, 4);
        builder.Property(e => e.UnemploymentInsuranceEmployeeRate).HasPrecision(5, 4);
        builder.Property(e => e.UnemploymentInsuranceEmployerRate).HasPrecision(5, 4);
    }
}
