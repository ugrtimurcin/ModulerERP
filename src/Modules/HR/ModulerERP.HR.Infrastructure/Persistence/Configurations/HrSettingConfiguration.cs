using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.HR.Domain.Entities;

namespace ModulerERP.HR.Infrastructure.Persistence.Configurations;

public class HrSettingConfiguration : IEntityTypeConfiguration<HrSetting>
{
    public void Configure(EntityTypeBuilder<HrSetting> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Key).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Value).HasMaxLength(500);
        builder.HasIndex(e => new { e.TenantId, e.Key }).IsUnique();
    }
}
