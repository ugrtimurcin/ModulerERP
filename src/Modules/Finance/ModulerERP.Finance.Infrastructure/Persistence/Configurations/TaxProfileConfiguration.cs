using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.Finance.Domain.Entities;

namespace ModulerERP.Finance.Infrastructure.Persistence.Configurations;

public class TaxProfileConfiguration : IEntityTypeConfiguration<TaxProfile>
{
    public void Configure(EntityTypeBuilder<TaxProfile> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);

        builder.HasMany(x => x.Lines)
            .WithOne(x => x.TaxProfile)
            .HasForeignKey(x => x.TaxProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
