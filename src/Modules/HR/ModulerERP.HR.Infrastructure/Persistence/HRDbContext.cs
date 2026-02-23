using Microsoft.EntityFrameworkCore;
using ModulerERP.SharedKernel.Entities;
using ModulerERP.SharedKernel.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Application.Interfaces;
using System.Text.Json;

namespace ModulerERP.HR.Infrastructure.Persistence;

public class HRDbContext : DbContext, IHRUnitOfWork
{
    private readonly ICurrentUserService _currentUserService;

    public HRDbContext(DbContextOptions<HRDbContext> options, ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<WorkShift> WorkShifts => Set<WorkShift>();
    public DbSet<AttendanceLog> AttendanceLogs => Set<AttendanceLog>();
    public DbSet<DailyAttendance> DailyAttendances => Set<DailyAttendance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<PublicHoliday> PublicHolidays => Set<PublicHoliday>();
    public DbSet<CommissionRule> CommissionRules => Set<CommissionRule>();
    public DbSet<PeriodCommission> PeriodCommissions { get; set; }
    public DbSet<AdvanceRequest> AdvanceRequests { get; set; }
    public DbSet<Bonus> Bonuses { get; set; }
    public DbSet<Payroll> Payrolls => Set<Payroll>();
    public DbSet<PayrollEntry> PayrollEntries => Set<PayrollEntry>();
    public DbSet<SalaryHistory> SalaryHistory => Set<SalaryHistory>();
    public DbSet<MinimumWage> MinimumWages => Set<MinimumWage>();
    public DbSet<PayrollParameter> PayrollParameters => Set<PayrollParameter>();
    public DbSet<TaxRule> TaxRules => Set<TaxRule>();

    public DbSet<HrSetting> HrSettings => Set<HrSetting>();
    public DbSet<LeavePolicy> LeavePolicies => Set<LeavePolicy>();
    public DbSet<EarningDeductionType> EarningDeductionTypes => Set<EarningDeductionType>();
    public DbSet<SgkRiskProfile> SgkRiskProfiles => Set<SgkRiskProfile>();
    public DbSet<EmployeeCumulative> EmployeeCumulatives => Set<EmployeeCumulative>();
    public DbSet<LeaveAllocation> LeaveAllocations => Set<LeaveAllocation>();
    public DbSet<PayrollEntryDetail> PayrollEntryDetails => Set<PayrollEntryDetail>();

    // Shared Audit Log (mapped to system_core schema)
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // EF Core caches the model. For global query filters to work dynamically,
    // the filter expression must close over a field/property that changes per-request.
    private Guid _tenantId => _currentUserService.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("hr");

        // Map AuditLog to system_core table
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs", "core", t => t.ExcludeFromMigrations());
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OldValues).HasColumnType("jsonb");
            entity.Property(e => e.NewValues).HasColumnType("jsonb");
        });

        modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<Department>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<AttendanceLog>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<DailyAttendance>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<LeaveRequest>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<PublicHoliday>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<CommissionRule>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<PeriodCommission>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<AdvanceRequest>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<Payroll>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<PayrollEntry>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<WorkShift>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<SalaryHistory>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<MinimumWage>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<PayrollParameter>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        
        modelBuilder.Entity<HrSetting>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<LeavePolicy>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<EarningDeductionType>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<SgkRiskProfile>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<EmployeeCumulative>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<LeaveAllocation>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);
        modelBuilder.Entity<PayrollEntryDetail>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantId);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HRDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>().ToList();
        var userId = _currentUserService.UserId;
        var tenantId = _currentUserService.TenantId;

        var auditEntries = new List<AuditLog>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            // Handle TenantId and Audit fields
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.TenantId == Guid.Empty && tenantId != Guid.Empty)
                {
                    entry.Entity.SetTenant(tenantId);
                }
                entry.Entity.SetCreator(userId);
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetUpdater(userId);
            }
            else if (entry.State == EntityState.Deleted)
            {
                if (entry.Entity is ISoftDeletable softDeletable)
                {
                    entry.State = EntityState.Modified;
                    softDeletable.Delete(userId);
                }
            }

            // Prepare Audit Log
            var action = entry.State switch
            {
                EntityState.Added => AuditAction.Insert,
                EntityState.Modified => entry.Entity.IsDeleted ? AuditAction.SoftDelete : AuditAction.Update,
                EntityState.Deleted => AuditAction.HardDelete,
                _ => AuditAction.Update
            };

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();
            var affectedColumns = new List<string>();

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary) continue;
                if (property.Metadata.IsPrimaryKey()) continue;

                var propertyName = property.Metadata.Name;
                var originalValue = property.OriginalValue;
                var currentValue = property.CurrentValue;

                if (entry.State == EntityState.Modified)
                {
                    if (!Equals(originalValue, currentValue))
                    {
                        oldValues[propertyName] = originalValue;
                        newValues[propertyName] = currentValue;
                        affectedColumns.Add(propertyName);
                    }
                }
                else if (entry.State == EntityState.Added)
                {
                    newValues[propertyName] = currentValue;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    oldValues[propertyName] = originalValue;
                }
            }

            if (entry.State == EntityState.Modified && affectedColumns.Count == 0 && action != AuditAction.SoftDelete)
                continue;

            var auditLog = AuditLog.Create(
                entry.Entity.TenantId,
                userId,
                entry.Entity.GetType().Name,
                entry.Entity.Id,
                action,
                oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null,
                newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null,
                affectedColumns.Count > 0 ? string.Join(',', affectedColumns) : null
            );

            auditEntries.Add(auditLog);
        }

        var result = await base.SaveChangesAsync(cancellationToken);


        if (auditEntries.Count > 0)
        {
            await AuditLogs.AddRangeAsync(auditEntries, cancellationToken);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    public async Task<int> SaveChangesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // This overload might be from interface but typically we use ICurrentUserService.
        // If IUnitOfWork defines it, we map it.
        // Assuming IUnitOfWork just has SaveChangesAsync(CancellationToken) or similar.
        // If the interface has this specific signature, we use it.
        // InventoryDbContext didn't show this signature. It showed IUnitOfWork at class level.
        // I'll stick to standard SaveChangesAsync.
        return await SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.RollbackTransactionAsync(cancellationToken);
    }
}
