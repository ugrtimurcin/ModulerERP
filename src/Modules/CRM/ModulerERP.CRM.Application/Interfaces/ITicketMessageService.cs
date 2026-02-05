using ModulerERP.CRM.Application.DTOs;

namespace ModulerERP.CRM.Application.Interfaces;

public interface ITicketMessageService
{
    Task<IEnumerable<TicketMessageListDto>> GetMessagesAsync(Guid tenantId, Guid ticketId, bool includeInternal = true, CancellationToken ct = default);
    Task<TicketMessageListDto> CreateMessageAsync(Guid tenantId, CreateTicketMessageDto dto, Guid userId, CancellationToken ct = default);
    Task DeleteMessageAsync(Guid tenantId, Guid messageId, Guid userId, CancellationToken ct = default);
}
