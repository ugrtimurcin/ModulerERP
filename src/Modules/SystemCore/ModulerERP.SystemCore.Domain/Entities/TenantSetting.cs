namespace ModulerERP.SystemCore.Domain.Entities;

/// <summary>
/// Dynamic Key-Value storage for tenant configuration.
/// Prevents Tenant table pollution with config columns.
/// </summary>
public class TenantSetting
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TenantId { get; private set; }
    
    /// <summary>Config Key (e.g., 'SMTP_HOST', 'INVOICE_PREFIX')</summary>
    public string Key { get; private set; } = string.Empty;
    
    /// <summary>Config Value</summary>
    public string Value { get; private set; } = string.Empty;
    
    /// <summary>Data type: 'String', 'Int', 'Boolean', 'Json'</summary>
    public string DataType { get; private set; } = "String";
    
    /// <summary>If true, value is encrypted at rest</summary>
    public bool IsEncrypted { get; private set; }

    // Navigation
    public Tenant? Tenant { get; private set; }

    private TenantSetting() { } // EF Core

    public static TenantSetting Create(Guid tenantId, string key, string value, string dataType = "String", bool isEncrypted = false)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Setting key is required", nameof(key));

        return new TenantSetting
        {
            TenantId = tenantId,
            Key = key.ToUpperInvariant(),
            Value = value,
            DataType = dataType,
            IsEncrypted = isEncrypted
        };
    }

    public void UpdateValue(string value)
    {
        Value = value;
    }
}
