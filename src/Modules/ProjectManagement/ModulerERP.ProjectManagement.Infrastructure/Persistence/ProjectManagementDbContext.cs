using Microsoft.EntityFrameworkCore;
using ModulerERP.ProjectManagement.Domain.Entities;
using ModulerERP.SharedKernel.Entities;
using ModulerERP.SharedKernel.Enums;
using ModulerERP.SharedKernel.Interfaces;
using System.Text.Json;

namespace ModulerERP.ProjectManagement.Infrastructure.Persistence;

public class ProjectManagementDbContext : DbContext, IUnitOfWork
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();
    public DbSet<ProjectTransaction> ProjectTransactions => Set<ProjectTransaction>();
    public DbSet<ProgressPayment> ProgressPayments => Set<ProgressPayment>();
    public DbSet<ProgressPaymentDetail> ProgressPaymentDetails => Set<ProgressPaymentDetail>();
    public DbSet<ProjectDocument> ProjectDocuments => Set<ProjectDocument>();
    public DbSet<BillOfQuantitiesItem> BillOfQuantitiesItems => Set<BillOfQuantitiesItem>();
    public DbSet<ProjectChangeOrder> ProjectChangeOrders => Set<ProjectChangeOrder>();
    public DbSet<ProjectResource> ProjectResources => Set<ProjectResource>();
    public DbSet<ProjectTaskResource> ProjectTaskResources => Set<ProjectTaskResource>();
    public DbSet<DailyLog> DailyLogs => Set<DailyLog>();
    public DbSet<DailyLogResourceUsage> DailyLogResourceUsages => Set<DailyLogResourceUsage>();
    public DbSet<DailyLogMaterialUsage> DailyLogMaterialUsages => Set<DailyLogMaterialUsage>();
    public DbSet<MaterialRequest> MaterialRequests => Set<MaterialRequest>();
    public DbSet<MaterialRequestItem> MaterialRequestItems => Set<MaterialRequestItem>();
    public DbSet<SubcontractorContract> SubcontractorContracts => Set<SubcontractorContract>();
    public DbSet<ResourceRateCard> ResourceRateCards => Set<ResourceRateCard>();

    // Shared Audit Log (mapped to system_core schema)
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    private readonly ICurrentUserService _currentUserService;

    public ProjectManagementDbContext(
        DbContextOptions<ProjectManagementDbContext> options,
        ICurrentUserService currentUserService) 
        : base(options) 
    {
        _currentUserService = currentUserService;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Set default schema
        modelBuilder.HasDefaultSchema("pm");

        // Map AuditLog to system_core table
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs", "core", t => t.ExcludeFromMigrations());
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OldValues).HasColumnType("jsonb");
            entity.Property(e => e.NewValues).HasColumnType("jsonb");
        });
        
        // Configure Project
        modelBuilder.Entity<Project>(entity =>
        {
            entity.OwnsOne(p => p.SiteAddress);
            entity.Property(e => e.ContractAmount).HasPrecision(18, 2);
            entity.Property(e => e.DefaultRetentionRate).HasPrecision(5, 4);
            entity.Property(e => e.DefaultWithholdingTaxRate).HasPrecision(5, 4);
        });

        // Configure Bill of Quantities (BoQ)
        modelBuilder.Entity<BillOfQuantitiesItem>(entity =>
        {
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.ContractUnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.EstimatedUnitCost).HasPrecision(18, 2);
        });

        // Configure Progress Payments
        modelBuilder.Entity<ProgressPayment>(entity =>
        {
            entity.Property(e => e.GrossWorkAmount).HasPrecision(18, 2);
            entity.Property(e => e.MaterialOnSiteAmount).HasPrecision(18, 2);
            entity.Property(e => e.CumulativeTotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.PreviousCumulativeAmount).HasPrecision(18, 2);
            entity.Property(e => e.PeriodDeltaAmount).HasPrecision(18, 2);
            entity.Property(e => e.RetentionAmount).HasPrecision(18, 2);
            entity.Property(e => e.WithholdingTaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.AdvanceDeductionAmount).HasPrecision(18, 2);
            entity.Property(e => e.NetPayableAmount).HasPrecision(18, 2);
            
            entity.Property(e => e.RetentionRate).HasPrecision(5, 4);
            entity.Property(e => e.WithholdingTaxRate).HasPrecision(5, 4);
        });

        // Configure Progress Payment Details
        modelBuilder.Entity<ProgressPaymentDetail>(entity =>
        {
            entity.Property(e => e.PreviousCumulativeQuantity).HasPrecision(18, 4);
            entity.Property(e => e.CumulativeQuantity).HasPrecision(18, 4);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
        });

        // Configure Project Resources
        modelBuilder.Entity<ProjectResource>(entity =>
        {
             entity.Property(e => e.HourlyCost).HasPrecision(18, 2);
        });

        // Configure Project Tasks & Resources
        modelBuilder.Entity<ProjectTask>(entity =>
        {
            entity.HasMany(t => t.Resources)
                  .WithOne(r => r.ProjectTask)
                  .HasForeignKey(r => r.ProjectTaskId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectTaskResource>(entity =>
        {
            // Composite unique index to prevent duplicate assignments
            entity.HasIndex(x => new { x.ProjectTaskId, x.ProjectResourceId }).IsUnique();
            entity.Property(e => e.AllocationPercent).HasPrecision(5, 2);
        });

        // Configure Daily Logs
        modelBuilder.Entity<DailyLogResourceUsage>(entity =>
        {
             entity.Property(e => e.HoursWorked).HasPrecision(18, 2);
        });
        modelBuilder.Entity<DailyLogMaterialUsage>(entity =>
        {
             entity.Property(e => e.Quantity).HasPrecision(18, 4);
        });
        
        // Configure Material Requests
        modelBuilder.Entity<MaterialRequestItem>(entity =>
        {
             entity.Property(e => e.Quantity).HasPrecision(18, 4);
        });

        // Configure Subcontractor Contracts
        modelBuilder.Entity<SubcontractorContract>(entity =>
        {
             entity.Property(e => e.ContractAmount).HasPrecision(18, 2);
        });

        // Configure Project Change Orders
        modelBuilder.Entity<ProjectChangeOrder>(entity =>
        {
            entity.Property(e => e.AmountChange).HasPrecision(18, 2);
        });

        // Configure Resource Rate Cards
        modelBuilder.Entity<ResourceRateCard>(entity =>
        {
             entity.Property(e => e.HourlyRate).HasPrecision(18, 2);
        });
      
        // Removed ProjectBudget Owned Entity configuration
        // modelBuilder.Entity<Project>(entity =>
        // {
        //    entity.OwnsOne(p => p.Budget);
        // });

        // Apply configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProjectManagementDbContext).Assembly);
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
                // Soft Delete Handling
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
                
                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey()) continue;

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

            // Skip if nothing changed in Update
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
