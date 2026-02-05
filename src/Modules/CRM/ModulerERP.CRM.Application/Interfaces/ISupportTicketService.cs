using ModulerERP.CRM.Application.DTOs;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.CRM.Application.Interfaces;

public interface ISupportTicketService
{
    Task<PagedResult<SupportTicketListDto>> GetTicketsAsync(Guid tenantId, int page, int pageSize, int? status = null, int? priority = null, CancellationToken ct = default);
    Task<SupportTicketDetailDto?> GetTicketByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<SupportTicketDetailDto> CreateTicketAsync(Guid tenantId, CreateSupportTicketDto dto, Guid userId, CancellationToken ct = default);
    Task<SupportTicketDetailDto> UpdateTicketAsync(Guid tenantId, Guid id, UpdateSupportTicketDto dto, CancellationToken ct = default);
    Task DeleteTicketAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct = default);
    
    // Lifecycle actions
    Task<SupportTicketDetailDto> AssignTicketAsync(Guid tenantId, Guid id, Guid assignedUserId, CancellationToken ct = default);
    Task<SupportTicketDetailDto> ResolveTicketAsync(Guid tenantId, Guid id, string resolution, CancellationToken ct = default);
    Task<SupportTicketDetailDto> CloseTicketAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<SupportTicketDetailDto> ReopenTicketAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}
