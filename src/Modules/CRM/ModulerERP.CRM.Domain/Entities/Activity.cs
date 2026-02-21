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
    
    public Guid? LeadId { get; private set; }
    public Guid? OpportunityId { get; private set; }
    public Guid? PartnerId { get; private set; }
    
    /// <summary>Is this a scheduled future activity?</summary>
    public bool IsScheduled { get; private set; }
    
    /// <summary>Has the activity been completed?</summary>
    public bool IsCompleted { get; private set; }
    
    public DateTime? CompletedAt { get; private set; }

    // Navigation
    public Lead? Lead { get; private set; }
    public Opportunity? Opportunity { get; private set; }
    public BusinessPartner? Partner { get; private set; }

    private Activity() { } // EF Core

    public static Activity Create(
        Guid tenantId,
        ActivityType type,
        string subject,
        DateTime activityDate,
        Guid createdByUserId,
        Guid? leadId = null,
        Guid? opportunityId = null,
        Guid? partnerId = null,
        string? description = null,
        bool isScheduled = false)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required", nameof(subject));

        // Enforce that exactly one parent entity is linked
        var parentCount = (leadId.HasValue ? 1 : 0) + (opportunityId.HasValue ? 1 : 0) + (partnerId.HasValue ? 1 : 0);
        if (parentCount != 1)
            throw new ArgumentException("An activity must be linked to exactly one entity (Lead, Opportunity, or Partner).");

        var activity = new Activity
        {
            Type = type,
            Subject = subject,
            Description = description,
            ActivityDate = activityDate,
            LeadId = leadId,
            OpportunityId = opportunityId,
            PartnerId = partnerId,
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
