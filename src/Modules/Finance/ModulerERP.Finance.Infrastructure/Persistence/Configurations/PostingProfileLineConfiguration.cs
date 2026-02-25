using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.Finance.Domain.Entities;

namespace ModulerERP.Finance.Infrastructure.Persistence.Configurations;

public class PostingProfileLineConfiguration : IEntityTypeConfiguration<PostingProfileLine>
{
    public void Configure(EntityTypeBuilder<PostingProfileLine> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
