using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.HR.Domain.Entities;

namespace ModulerERP.HR.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.HasOne<Employee>().WithMany().HasForeignKey(e => e.ManagerId).IsRequired(false);
    }
}

public class DailyAttendanceConfiguration : IEntityTypeConfiguration<DailyAttendance>
{
    public void Configure(EntityTypeBuilder<DailyAttendance> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne<Employee>().WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        // Date is just a date, usually mapped to date type in PG
        builder.Property(e => e.Date).HasColumnType("date");
    }
}

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne<Employee>().WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(e => e.Reason).HasMaxLength(500);
    }
}

public class PayrollConfiguration : IEntityTypeConfiguration<Payroll>
{
    public void Configure(EntityTypeBuilder<Payroll> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Period).IsRequired().HasMaxLength(7); // YYYY-MM
        builder.Property(e => e.Description).HasMaxLength(200);
        
        builder.HasMany(e => e.Entries)
               .WithOne()
               .HasForeignKey(e => e.PayrollId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PayrollEntryConfiguration : IEntityTypeConfiguration<PayrollEntry>
{
    public void Configure(EntityTypeBuilder<PayrollEntry> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne<Employee>().WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(e => e.BaseSalary).HasPrecision(18, 2);
        builder.Property(e => e.OvertimePay).HasPrecision(18, 2);
        builder.Property(e => e.CommissionPay).HasPrecision(18, 2);
        builder.Property(e => e.AdvanceDeduction).HasPrecision(18, 2);
            builder.Property(e => e.IncomeTax).HasColumnType("decimal(18,2)");
            builder.Property(e => e.SocialSecurityEmployee).HasColumnType("decimal(18,2)");
            builder.Property(e => e.ProvidentFundEmployee).HasColumnType("decimal(18,2)");
            builder.Property(e => e.SocialSecurityEmployer).HasColumnType("decimal(18,2)");
            builder.Property(e => e.ProvidentFundEmployer).HasColumnType("decimal(18,2)");
            builder.Property(e => e.UnemploymentInsuranceEmployer).HasColumnType("decimal(18,2)");
        builder.Property(e => e.NetPayable).HasPrecision(18, 2);
        builder.Property(e => e.ExchangeRate).HasPrecision(18, 4);
    }
}
public class WorkShiftConfiguration : IEntityTypeConfiguration<WorkShift>
{
    public void Configure(EntityTypeBuilder<WorkShift> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(100);
    }
}

public class AdvanceRequestConfiguration : IEntityTypeConfiguration<AdvanceRequest>
{
    public void Configure(EntityTypeBuilder<AdvanceRequest> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne<Employee>()
               .WithMany(e => e.AdvanceRequests)
               .HasForeignKey(e => e.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade);
               
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.Description).HasMaxLength(500);
    }
}
