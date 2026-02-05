using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.SystemCore.Domain.Entities;

/// <summary>
/// Tenant entity - Represents organizations using the SaaS platform.
/// Core multi-tenancy entity with TRNC-specific BaseCurrencyId for financial reporting.
/// </summary>
public class Tenant : BaseEntity
{
    /// <summary>Company Name</summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>Unique identifier for access (e.g., 'acme'.erp.com)</summary>
    public string Subdomain { get; private set; } = string.Empty;
    
    /// <summary>For database-level isolation strategies</summary>
    public string? DbSchema { get; private set; }
    
    /// <summary>The Reporting/Tax currency (e.g., TRY for TRNC)</summary>
    public Guid BaseCurrencyId { get; private set; }
    
    /// <summary>Subscription plan code (e.g., 'ENTERPRISE_PLUS')</summary>
    public string? SubscriptionPlan { get; private set; }
    
    /// <summary>Subscription expiration date</summary>
    public DateTime? SubscriptionExpiresAt { get; private set; }
    
    /// <summary>Timezone (e.g., 'Europe/Nicosia')</summary>
    public string TimeZone { get; private set; } = "Europe/Nicosia";

    // Navigation
    public Currency? BaseCurrency { get; private set; }
    public ICollection<TenantSetting> Settings { get; private set; } = new List<TenantSetting>();
    public ICollection<TenantFeature> Features { get; private set; } = new List<TenantFeature>();

    private Tenant() { } // EF Core

    public static Tenant Create(string name, string subdomain, Guid baseCurrencyId, string timeZone = "Europe/Nicosia")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(subdomain))
            throw new ArgumentException("Subdomain is required", nameof(subdomain));

        var tenant = new Tenant
        {
            Name = name,
            Subdomain = subdomain.ToLowerInvariant(),
            BaseCurrencyId = baseCurrencyId,
            TimeZone = timeZone
        };
        
        // Self-referencing TenantId for the Tenant entity itself
        tenant.SetTenant(tenant.Id);
        return tenant;
    }

    public void UpdateSubscription(string plan, DateTime expiresAt)
    {
        SubscriptionPlan = plan;
        SubscriptionExpiresAt = expiresAt;
    }

    public void SetDbSchema(string schema)
    {
        DbSchema = schema;
    }
}
