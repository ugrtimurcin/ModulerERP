namespace ModulerERP.SystemCore.Domain.Entities;

/// <summary>
/// Multi-device session management with refresh tokens.
/// Supports simultaneous login (Mobile + Web) and remote logout.
/// </summary>
public class UserSession
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public Guid TenantId { get; private set; }
    
    /// <summary>Secure opaque refresh token</summary>
    public string RefreshToken { get; private set; } = string.Empty;
    
    /// <summary>Token expiration</summary>
    public DateTime ExpiresAt { get; private set; }
    
    /// <summary>Audit trail - client IP</summary>
    public string? IpAddress { get; private set; }
    
    /// <summary>User Agent (e.g., 'iPhone 13 / iOS 15')</summary>
    public string? DeviceInfo { get; private set; }
    
    /// <summary>If true, session is killed</summary>
    public bool IsRevoked { get; private set; }
    
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public User? User { get; private set; }

    private UserSession() { } // EF Core

    public static UserSession Create(Guid tenantId, Guid userId, string refreshToken, DateTime expiresAt, string? ipAddress = null, string? deviceInfo = null)
    {
        return new UserSession
        {
            TenantId = tenantId,
            UserId = userId,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            IpAddress = ipAddress,
            DeviceInfo = deviceInfo
        };
    }

    public void Revoke() => IsRevoked = true;
    public bool IsValid => !IsRevoked && ExpiresAt > DateTime.UtcNow;
}
