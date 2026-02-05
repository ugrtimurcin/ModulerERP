using ModulerERP.SharedKernel.Entities;
using ModulerERP.CRM.Domain.Enums;

namespace ModulerERP.CRM.Domain.Entities;

/// <summary>
/// Log interactions (Call, Email, Meeting, Note).
/// Polymorphic - can link to Lead, Opportunity, Partner.
/// </summary>
public class Activity : BaseEntity
{
    public ActivityType Type { get; private set; }
    
    /// <summary>Brief summary</summary>
    public string Subject { get; private set; } = string.Empty;
    
    /// <summary>Meeting notes, call summary, etc.</summary>
    public string? Description { get; private set; }
    
    /// <summary>When the activity happened/is scheduled</summary>
    public DateTime ActivityDate { get; private set; }
    
    /// <summary>Polymorphic entity type: Lead, Opportunity, Partner</summary>
    public string EntityType { get; private set; } = string.Empty;
    
    /// <summary>Related entity ID</summary>
    public Guid EntityId { get; private set; }
    
    /// <summary>Is this a scheduled future activity?</summary>
    public bool IsScheduled { get; private set; }
    
    /// <summary>Has the activity been completed?</summary>
    public bool IsCompleted { get; private set; }
    
    public DateTime? CompletedAt { get; private set; }

    private Activity() { } // EF Core

    public static Activity Create(
        Guid tenantId,
        ActivityType type,
        string subject,
        string entityType,
        Guid entityId,
        DateTime activityDate,
        Guid createdByUserId,
        string? description = null,
        bool isScheduled = false)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required", nameof(subject));

        var activity = new Activity
        {
            Type = type,
            Subject = subject,
            Description = description,
            ActivityDate = activityDate,
            EntityType = entityType,
            EntityId = entityId,
            IsScheduled = isScheduled
        };

        activity.SetTenant(tenantId);
        activity.SetCreator(createdByUserId);
        return activity;
    }

    public void MarkAsCompleted()
    {
        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
    }

    public void Update(string subject, string? description, DateTime activityDate)
    {
        Subject = subject;
        Description = description;
        ActivityDate = activityDate;
    }
}
