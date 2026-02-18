using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.CRM.Domain.Entities;

namespace ModulerERP.CRM.Infrastructure.Persistence.Configurations;

public class OpportunityConfiguration : IEntityTypeConfiguration<Opportunity>
{
    public void Configure(EntityTypeBuilder<Opportunity> builder)
    {
        builder.ToTable("Opportunities");
        
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);

        // Money Value Object — EstimatedValue
        builder.OwnsOne(e => e.EstimatedValue, money =>
        {
            money.Property(p => p.Amount).HasColumnName("EstimatedValue").HasPrecision(18, 2);
            money.Property(p => p.CurrencyCode).HasColumnName("EstimatedValueCurrency").HasMaxLength(3);
        });
    }
}

public class BusinessPartnerConfiguration : IEntityTypeConfiguration<BusinessPartner>
{
    public void Configure(EntityTypeBuilder<BusinessPartner> builder)
    {
        builder.ToTable("BusinessPartners");

        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);

        // Unique tenant + code
        builder.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();

        // Address Value Objects
        builder.OwnsOne(e => e.BillingAddress, addr =>
        {
            addr.Property(p => p.Street).HasColumnName("BillingStreet").HasMaxLength(500);
            addr.Property(p => p.District).HasColumnName("BillingDistrict").HasMaxLength(100);
            addr.Property(p => p.City).HasColumnName("BillingCity").HasMaxLength(100);
            addr.Property(p => p.ZipCode).HasColumnName("BillingZipCode").HasMaxLength(20);
            addr.Property(p => p.Country).HasColumnName("BillingCountry").HasMaxLength(100);
            addr.Property(p => p.Block).HasColumnName("BillingBlock").HasMaxLength(50);
            addr.Property(p => p.Parcel).HasColumnName("BillingParcel").HasMaxLength(50);
        });

        builder.OwnsOne(e => e.ShippingAddress, addr =>
        {
            addr.Property(p => p.Street).HasColumnName("ShippingStreet").HasMaxLength(500);
            addr.Property(p => p.District).HasColumnName("ShippingDistrict").HasMaxLength(100);
            addr.Property(p => p.City).HasColumnName("ShippingCity").HasMaxLength(100);
            addr.Property(p => p.ZipCode).HasColumnName("ShippingZipCode").HasMaxLength(20);
            addr.Property(p => p.Country).HasColumnName("ShippingCountry").HasMaxLength(100);
            addr.Property(p => p.Block).HasColumnName("ShippingBlock").HasMaxLength(50);
            addr.Property(p => p.Parcel).HasColumnName("ShippingParcel").HasMaxLength(50);
        });
    }
}

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts");

        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).HasMaxLength(100);

        // Address Value Object
        builder.OwnsOne(e => e.Address, addr =>
        {
            addr.Property(p => p.Street).HasColumnName("Street").HasMaxLength(500);
            addr.Property(p => p.District).HasColumnName("District").HasMaxLength(100);
            addr.Property(p => p.City).HasColumnName("City").HasMaxLength(100);
            addr.Property(p => p.ZipCode).HasColumnName("ZipCode").HasMaxLength(20);
            addr.Property(p => p.Country).HasColumnName("Country").HasMaxLength(100);
            addr.Property(p => p.Block).HasColumnName("Block").HasMaxLength(50);
            addr.Property(p => p.Parcel).HasColumnName("Parcel").HasMaxLength(50);
        });
    }
}
