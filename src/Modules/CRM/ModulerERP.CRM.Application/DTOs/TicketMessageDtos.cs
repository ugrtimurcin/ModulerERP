namespace ModulerERP.CRM.Application.DTOs;

// TicketMessage DTOs
public record TicketMessageListDto(
    Guid Id, 
    Guid? SenderUserId, 
    string? SenderName,
    string Message, 
    bool IsInternal, 
    DateTime CreatedAt);

public record CreateTicketMessageDto(
    Guid TicketId,
    string Message,
    bool IsInternal = false);
