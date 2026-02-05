using Microsoft.EntityFrameworkCore;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.CRM.Infrastructure.Persistence;

namespace ModulerERP.CRM.Infrastructure.Services;

public class TicketMessageService : ITicketMessageService
{
    private readonly CRMDbContext _context;

    public TicketMessageService(CRMDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TicketMessageListDto>> GetMessagesAsync(Guid tenantId, Guid ticketId, bool includeInternal = true, CancellationToken ct = default)
    {
        var query = _context.TicketMessages
            .Where(m => m.TenantId == tenantId && m.TicketId == ticketId);

        if (!includeInternal)
            query = query.Where(m => !m.IsInternal);

        return await query
            .OrderBy(m => m.CreatedAt)
            .Select(m => new TicketMessageListDto(
                m.Id,
                m.SenderUserId,
                null, // TODO: Join with Users for sender name
                m.Message,
                m.IsInternal,
                m.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<TicketMessageListDto> CreateMessageAsync(Guid tenantId, CreateTicketMessageDto dto, Guid userId, CancellationToken ct = default)
    {
        // Verify ticket exists
        var ticketExists = await _context.SupportTickets
            .AnyAsync(t => t.TenantId == tenantId && t.Id == dto.TicketId, ct);

        if (!ticketExists)
            throw new KeyNotFoundException("Ticket not found");

        var message = TicketMessage.Create(
            tenantId,
            dto.TicketId,
            dto.Message,
            userId,
            userId, // SenderUserId is the current user (internal message)
            dto.IsInternal);

        _context.TicketMessages.Add(message);
        await _context.SaveChangesAsync(ct);

        return new TicketMessageListDto(
            message.Id,
            message.SenderUserId,
            null, // TODO: Join with Users for sender name
            message.Message,
            message.IsInternal,
            message.CreatedAt);
    }

    public async Task DeleteMessageAsync(Guid tenantId, Guid messageId, Guid userId, CancellationToken ct = default)
    {
        var message = await _context.TicketMessages
            .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.Id == messageId, ct)
            ?? throw new KeyNotFoundException("Message not found");

        message.Delete(userId);
        await _context.SaveChangesAsync(ct);
    }
}
