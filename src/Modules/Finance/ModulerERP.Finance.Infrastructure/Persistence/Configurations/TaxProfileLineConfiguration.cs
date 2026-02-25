using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.Finance.Domain.Entities;

namespace ModulerERP.Finance.Infrastructure.Persistence.Configurations;

public class TaxProfileLineConfiguration : IEntityTypeConfiguration<TaxProfileLine>
{
    public void Configure(EntityTypeBuilder<TaxProfileLine> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.TaxRate)
            .WithMany()
            .HasForeignKey(x => x.TaxRateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
