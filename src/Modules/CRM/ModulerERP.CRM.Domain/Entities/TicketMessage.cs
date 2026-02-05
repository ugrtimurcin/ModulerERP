using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.CRM.Domain.Entities;

/// <summary>
/// Conversation history within a support ticket.
/// </summary>
public class TicketMessage : BaseEntity
{
    /// <summary>FK to SupportTicket</summary>
    public Guid TicketId { get; private set; }
    
    /// <summary>Null if sent by Customer (External)</summary>
    public Guid? SenderUserId { get; private set; }
    
    /// <summary>Message content</summary>
    public string Message { get; private set; } = string.Empty;
    
    /// <summary>If true, invisible to customer (Internal Note)</summary>
    public bool IsInternal { get; private set; }
    
    // Navigation
    public SupportTicket? Ticket { get; private set; }

    private TicketMessage() { } // EF Core

    public static TicketMessage Create(
        Guid tenantId,
        Guid ticketId,
        string message,
        Guid createdByUserId,
        Guid? senderUserId = null,
        bool isInternal = false)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message is required", nameof(message));

        var ticketMessage = new TicketMessage
        {
            TicketId = ticketId,
            SenderUserId = senderUserId,
            Message = message,
            IsInternal = isInternal
        };

        ticketMessage.SetTenant(tenantId);
        ticketMessage.SetCreator(createdByUserId);
        return ticketMessage;
    }

    public void UpdateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message is required", nameof(message));
        Message = message;
    }
}
