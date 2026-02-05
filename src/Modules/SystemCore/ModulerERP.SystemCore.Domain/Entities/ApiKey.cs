using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.SystemCore.Domain.Entities;

/// <summary>
/// Inbound API access for external system integration.
/// </summary>
public class ApiKey : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>Hashed API key (never store plaintext)</summary>
    public string KeyHash { get; private set; } = string.Empty;
    
    /// <summary>Key prefix for identification (e.g., 'mk_live_')</summary>
    public string KeyPrefix { get; private set; } = string.Empty;
    
    /// <summary>Permissions as JSON array</summary>
    public string Permissions { get; private set; } = "[]";
    
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }

    private ApiKey() { } // EF Core

    public static ApiKey Create(Guid tenantId, string name, string keyHash, string keyPrefix, string permissions, Guid createdByUserId, DateTime? expiresAt = null)
    {
        var apiKey = new ApiKey
        {
            Name = name,
            KeyHash = keyHash,
            KeyPrefix = keyPrefix,
            Permissions = permissions,
            ExpiresAt = expiresAt
        };

        apiKey.SetTenant(tenantId);
        apiKey.SetCreator(createdByUserId);
        return apiKey;
    }

    public void RecordUsage() => LastUsedAt = DateTime.UtcNow;
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
}
