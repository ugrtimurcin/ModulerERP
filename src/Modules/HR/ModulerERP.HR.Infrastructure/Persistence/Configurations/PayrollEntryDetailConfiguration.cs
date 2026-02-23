using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.HR.Domain.Entities;

namespace ModulerERP.HR.Infrastructure.Persistence.Configurations;

public class PayrollEntryDetailConfiguration : IEntityTypeConfiguration<PayrollEntryDetail>
{
    public void Configure(EntityTypeBuilder<PayrollEntryDetail> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.PayrollEntry).WithMany().HasForeignKey(e => e.PayrollEntryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Type).WithMany().HasForeignKey(e => e.EarningDeductionTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
    }
}
