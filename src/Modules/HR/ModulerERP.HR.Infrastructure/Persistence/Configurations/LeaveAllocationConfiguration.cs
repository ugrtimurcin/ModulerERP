using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.HR.Domain.Entities;

namespace ModulerERP.HR.Infrastructure.Persistence.Configurations;

public class LeaveAllocationConfiguration : IEntityTypeConfiguration<LeaveAllocation>
{
    public void Configure(EntityTypeBuilder<LeaveAllocation> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.LeavePolicy).WithMany().HasForeignKey(e => e.LeavePolicyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => new { e.EmployeeId, e.LeavePolicyId, e.Year }).IsUnique();
    }
}
