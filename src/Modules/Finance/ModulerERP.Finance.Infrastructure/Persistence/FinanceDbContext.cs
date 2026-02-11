using Microsoft.EntityFrameworkCore;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Entities;
using ModulerERP.SharedKernel.Enums;
using System.Text.Json;

namespace ModulerERP.Finance.Infrastructure.Persistence;

public class FinanceDbContext : DbContext, IUnitOfWork
{
    private readonly Guid _tenantId;
    private readonly ICurrentUserService _currentUserService;

    public FinanceDbContext(
        DbContextOptions<FinanceDbContext> options,
        ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
        _tenantId = currentUserService.TenantId;
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<FiscalPeriod> FiscalPeriods { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<JournalEntryLine> JournalEntryLines { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<CostCenter> CostCenters { get; set; }
    public DbSet<ExchangeRate> ExchangeRates { get; set; }
    public DbSet<TaxRate> TaxRates { get; set; }
    public DbSet<Cheque> Cheques { get; set; }
    public DbSet<ChequeHistory> ChequeHistories { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("finance");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanceDbContext).Assembly);
        
        // Basic configuration if no separate config files yet
        modelBuilder.Entity<JournalEntry>()
            .HasMany(e => e.Lines)
            .WithOne(e => e.JournalEntry)
            .HasForeignKey(e => e.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Account>()
            .HasOne(e => e.ParentAccount)
            .WithMany(e => e.ChildAccounts)
            .HasForeignKey(e => e.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cheque>()
            .HasMany<ChequeHistory>()
            .WithOne(e => e.Cheque)
            .HasForeignKey(e => e.ChequeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Map AuditLog to shared system_core table
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs", "core", t => t.ExcludeFromMigrations());
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OldValues).HasColumnType("jsonb");
            entity.Property(e => e.NewValues).HasColumnType("jsonb");
        });
    }
    
    private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _currentTransaction;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null) return;
        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try 
        {
            await SaveChangesAsync(cancellationToken);
            
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>().ToList();
        var userId = _currentUserService.UserId;
        var tenantId = _currentUserService.TenantId != Guid.Empty ? _currentUserService.TenantId : _tenantId;

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
}
