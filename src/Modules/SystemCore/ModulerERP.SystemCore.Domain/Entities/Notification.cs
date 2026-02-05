namespace ModulerERP.SystemCore.Domain.Entities;

/// <summary>
/// In-app alerts for users.
/// </summary>
public class Notification
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    
    public bool IsRead { get; private set; }
    
    /// <summary>Action URL when clicked</summary>
    public string? Link { get; private set; }
    
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public User? User { get; private set; }

    private Notification() { } // EF Core

    public static Notification Create(Guid tenantId, Guid userId, string title, string content, string? link = null)
    {
        return new Notification
        {
            TenantId = tenantId,
            UserId = userId,
            Title = title,
            Content = content,
            Link = link
        };
    }

    public void MarkAsRead() => IsRead = true;
    public void MarkAsUnread() => IsRead = false;
}
