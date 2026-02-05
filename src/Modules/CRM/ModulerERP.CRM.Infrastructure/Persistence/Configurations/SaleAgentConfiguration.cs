using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.CRM.Domain.Entities;

namespace ModulerERP.CRM.Infrastructure.Persistence.Configurations;

public class SaleAgentConfiguration : IEntityTypeConfiguration<SaleAgent>
{
    public void Configure(EntityTypeBuilder<SaleAgent> builder)
    {
        builder.ToTable("SaleAgents");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.TenantId, x.EmployeeId }).IsUnique();

        builder.Property(x => x.CommissionRate)
            .HasPrecision(5, 2);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
