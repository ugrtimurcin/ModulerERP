using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.SystemCore.Domain.Entities;

/// <summary>
/// Outbound event notification for external systems.
/// </summary>
public class Webhook : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>Target URL for notifications</summary>
    public string Url { get; private set; } = string.Empty;
    
    /// <summary>Secret for HMAC signature verification</summary>
    public string Secret { get; private set; } = string.Empty;
    
    /// <summary>Events to subscribe to as JSON array (e.g., ["invoice.created", "order.shipped"])</summary>
    public string Events { get; private set; } = "[]";
    
    /// <summary>Number of consecutive failures</summary>
    public int FailureCount { get; private set; }
    
    /// <summary>Last successful delivery</summary>
    public DateTime? LastSuccessAt { get; private set; }

    private Webhook() { } // EF Core

    public static Webhook Create(Guid tenantId, string name, string url, string secret, string events, Guid createdByUserId)
    {
        var webhook = new Webhook
        {
            Name = name,
            Url = url,
            Secret = secret,
            Events = events
        };

        webhook.SetTenant(tenantId);
        webhook.SetCreator(createdByUserId);
        return webhook;
    }

    public void RecordSuccess()
    {
        LastSuccessAt = DateTime.UtcNow;
        FailureCount = 0;
    }

    public void RecordFailure() => FailureCount++;
    
    public bool IsHealthy => FailureCount < 5;
}
