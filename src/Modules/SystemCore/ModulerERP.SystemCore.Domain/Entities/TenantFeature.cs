namespace ModulerERP.SystemCore.Domain.Entities;

/// <summary>
/// Feature flags to enable/disable modules per tenant.
/// Supports trial features with expiration dates.
/// </summary>
public class TenantFeature
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TenantId { get; private set; }
    
    /// <summary>Unique code (e.g., 'MOD_MANUFACTURING', 'MOD_HR')</summary>
    public string FeatureCode { get; private set; } = string.Empty;
    
    /// <summary>Feature toggle switch</summary>
    public bool IsEnabled { get; private set; }
    
    /// <summary>Expiration date for trial features</summary>
    public DateTime? ValidUntil { get; private set; }

    // Navigation
    public Tenant? Tenant { get; private set; }

    private TenantFeature() { } // EF Core

    public static TenantFeature Create(Guid tenantId, string featureCode, bool isEnabled = true, DateTime? validUntil = null)
    {
        if (string.IsNullOrWhiteSpace(featureCode))
            throw new ArgumentException("Feature code is required", nameof(featureCode));

        return new TenantFeature
        {
            TenantId = tenantId,
            FeatureCode = featureCode.ToUpperInvariant(),
            IsEnabled = isEnabled,
            ValidUntil = validUntil
        };
    }

    public void Enable() => IsEnabled = true;
    public void Disable() => IsEnabled = false;
    
    public void SetTrialPeriod(DateTime expiresAt)
    {
        ValidUntil = expiresAt;
    }

    public bool IsValid() => IsEnabled && (!ValidUntil.HasValue || ValidUntil.Value > DateTime.UtcNow);
}
