using ModulerERP.SharedKernel.Entities;
using ModulerERP.CRM.Domain.Enums;

namespace ModulerERP.CRM.Domain.Entities;

/// <summary>
/// Basic customer service/helpdesk functionality.
/// </summary>
public class SupportTicket : BaseEntity
{
    public Guid? PartnerId { get; private set; }
    
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    
    public TicketPriority Priority { get; private set; } = TicketPriority.Medium;
    public TicketStatus Status { get; private set; } = TicketStatus.Open;
    
    /// <summary>Assigned support agent</summary>
    public Guid? AssignedUserId { get; private set; }
    
    public DateTime? ResolvedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    
    /// <summary>Resolution notes</summary>
    public string? Resolution { get; private set; }

    // Navigation
    public BusinessPartner? Partner { get; private set; }

    private SupportTicket() { } // EF Core

    public static SupportTicket Create(
        Guid tenantId,
        string title,
        string description,
        Guid createdByUserId,
        Guid? partnerId = null,
        TicketPriority priority = TicketPriority.Medium,
        Guid? assignedUserId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));

        var ticket = new SupportTicket
        {
            PartnerId = partnerId,
            Title = title,
            Description = description,
            Priority = priority,
            AssignedUserId = assignedUserId
        };

        ticket.SetTenant(tenantId);
        ticket.SetCreator(createdByUserId);
        return ticket;
    }

    public void UpdatePriority(TicketPriority priority) => Priority = priority;
    
    public void Assign(Guid userId) => AssignedUserId = userId;

    public void Resolve(string resolution)
    {
        Status = TicketStatus.Resolved;
        Resolution = resolution;
        ResolvedAt = DateTime.UtcNow;
    }

    public void Close()
    {
        Status = TicketStatus.Closed;
        ClosedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        Status = TicketStatus.Open;
        ResolvedAt = null;
        ClosedAt = null;
        Resolution = null;
    }
}
