using Microsoft.EntityFrameworkCore;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.CRM.Domain.Enums;
using ModulerERP.CRM.Infrastructure.Persistence;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.CRM.Infrastructure.Services;

public class SupportTicketService : ISupportTicketService
{
    private readonly CRMDbContext _context;

    public SupportTicketService(CRMDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<SupportTicketListDto>> GetTicketsAsync(Guid tenantId, int page, int pageSize, int? status = null, int? priority = null, CancellationToken ct = default)
    {
        var query = _context.SupportTickets
            .Include(t => t.Partner)
            .Where(t => t.TenantId == tenantId);

        if (status.HasValue)
            query = query.Where(t => (int)t.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(t => (int)t.Priority == priority.Value);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var data = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new SupportTicketListDto(
                t.Id,
                t.Title,
                (int)t.Priority,
                t.Priority.ToString(),
                (int)t.Status,
                t.Status.ToString(),
                t.PartnerId,
                t.Partner != null ? t.Partner.Name : null,
                t.AssignedUserId,
                t.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<SupportTicketListDto>(data, page, pageSize, totalCount, totalPages);
    }

    public async Task<SupportTicketDetailDto?> GetTicketByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var ticket = await _context.SupportTickets
            .Include(t => t.Partner)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id, ct);

        if (ticket == null) return null;

        return MapToDetailDto(ticket);
    }

    public async Task<SupportTicketDetailDto> CreateTicketAsync(Guid tenantId, CreateSupportTicketDto dto, Guid userId, CancellationToken ct = default)
    {
        var ticket = SupportTicket.Create(
            tenantId,
            dto.Title,
            dto.Description,
            userId,
            dto.PartnerId,
            (TicketPriority)dto.Priority,
            dto.AssignedUserId);

        _context.SupportTickets.Add(ticket);
        await _context.SaveChangesAsync(ct);

        return (await GetTicketByIdAsync(tenantId, ticket.Id, ct))!;
    }

    public async Task<SupportTicketDetailDto> UpdateTicketAsync(Guid tenantId, Guid id, UpdateSupportTicketDto dto, CancellationToken ct = default)
    {
        var ticket = await _context.SupportTickets
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id, ct)
            ?? throw new KeyNotFoundException("Ticket not found");

        // Update via reflection or add Update method to entity - using basic approach
        ticket.UpdatePriority((TicketPriority)dto.Priority);
        if (dto.AssignedUserId.HasValue)
            ticket.Assign(dto.AssignedUserId.Value);

        await _context.SaveChangesAsync(ct);
        return (await GetTicketByIdAsync(tenantId, id, ct))!;
    }

    public async Task DeleteTicketAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct = default)
    {
        var ticket = await _context.SupportTickets
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id, ct)
            ?? throw new KeyNotFoundException("Ticket not found");

        ticket.Delete(userId);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<SupportTicketDetailDto> AssignTicketAsync(Guid tenantId, Guid id, Guid assignedUserId, CancellationToken ct = default)
    {
        var ticket = await _context.SupportTickets
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id, ct)
            ?? throw new KeyNotFoundException("Ticket not found");

        ticket.Assign(assignedUserId);
        await _context.SaveChangesAsync(ct);
        return (await GetTicketByIdAsync(tenantId, id, ct))!;
    }

    public async Task<SupportTicketDetailDto> ResolveTicketAsync(Guid tenantId, Guid id, string resolution, CancellationToken ct = default)
    {
        var ticket = await _context.SupportTickets
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id, ct)
            ?? throw new KeyNotFoundException("Ticket not found");

        ticket.Resolve(resolution);
        await _context.SaveChangesAsync(ct);
        return (await GetTicketByIdAsync(tenantId, id, ct))!;
    }

    public async Task<SupportTicketDetailDto> CloseTicketAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var ticket = await _context.SupportTickets
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id, ct)
            ?? throw new KeyNotFoundException("Ticket not found");

        ticket.Close();
        await _context.SaveChangesAsync(ct);
        return (await GetTicketByIdAsync(tenantId, id, ct))!;
    }

    public async Task<SupportTicketDetailDto> ReopenTicketAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var ticket = await _context.SupportTickets
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id, ct)
            ?? throw new KeyNotFoundException("Ticket not found");

        ticket.Reopen();
        await _context.SaveChangesAsync(ct);
        return (await GetTicketByIdAsync(tenantId, id, ct))!;
    }

    private SupportTicketDetailDto MapToDetailDto(SupportTicket ticket)
    {
        return new SupportTicketDetailDto(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            (int)ticket.Priority,
            ticket.Priority.ToString(),
            (int)ticket.Status,
            ticket.Status.ToString(),
            ticket.PartnerId,
            ticket.Partner?.Name,
            ticket.AssignedUserId,
            ticket.Resolution,
            ticket.ResolvedAt,
            ticket.ClosedAt,
            ticket.CreatedAt);
    }
}
