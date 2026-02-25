using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.Finance.Domain.Entities;

namespace ModulerERP.Finance.Infrastructure.Persistence.Configurations;

public class PostingProfileConfiguration : IEntityTypeConfiguration<PostingProfile>
{
    public void Configure(EntityTypeBuilder<PostingProfile> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);

        builder.HasMany(x => x.Lines)
            .WithOne(x => x.PostingProfile)
            .HasForeignKey(x => x.PostingProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
