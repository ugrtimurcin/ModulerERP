namespace ModulerERP.SystemCore.Domain.Entities;

/// <summary>
/// Security audit log for login attempts (Success/Fail).
/// </summary>
public class UserLoginHistory
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public Guid TenantId { get; private set; }
    
    public DateTime LoginTime { get; private set; } = DateTime.UtcNow;
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public bool IsSuccessful { get; private set; }
    public string? FailureReason { get; private set; }

    // Navigation
    public User? User { get; private set; }

    private UserLoginHistory() { } // EF Core

    public static UserLoginHistory CreateSuccess(Guid tenantId, Guid userId, string? ipAddress, string? userAgent)
    {
        return new UserLoginHistory
        {
            TenantId = tenantId,
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            IsSuccessful = true
        };
    }

    public static UserLoginHistory CreateFailure(Guid tenantId, Guid userId, string? ipAddress, string? userAgent, string failureReason)
    {
        return new UserLoginHistory
        {
            TenantId = tenantId,
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            IsSuccessful = false,
            FailureReason = failureReason
        };
    }
}
