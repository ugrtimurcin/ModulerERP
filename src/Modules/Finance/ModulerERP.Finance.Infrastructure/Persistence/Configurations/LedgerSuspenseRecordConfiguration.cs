using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.Finance.Domain.Entities;

namespace ModulerERP.Finance.Infrastructure.Persistence.Configurations;

public class LedgerSuspenseRecordConfiguration : IEntityTypeConfiguration<LedgerSuspenseRecord>
{
    public void Configure(EntityTypeBuilder<LedgerSuspenseRecord> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.SourceType).HasMaxLength(100).IsRequired();
        builder.Property(e => e.SourceNumber).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500);
        
        builder.Property(e => e.RawRequestPayload).IsRequired(); // Ideally JSONB in Postgres
        builder.Property(e => e.ErrorMessage).IsRequired();

        // Indexes for quickly finding unresolved records grouped by Type
        builder.HasIndex(e => new { e.TenantId, e.IsResolved });
        builder.HasIndex(e => e.TransactionType);
    }
}
