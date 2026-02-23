using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.HR.Domain.Entities;

namespace ModulerERP.HR.Infrastructure.Persistence.Configurations;

public class EmployeeCumulativeConfiguration : IEntityTypeConfiguration<EmployeeCumulative>
{
    public void Configure(EntityTypeBuilder<EmployeeCumulative> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(e => e.YtdTaxBase).HasPrecision(18, 2);
        builder.Property(e => e.TotalSeveranceAccrual).HasPrecision(18, 2);
    }
}
