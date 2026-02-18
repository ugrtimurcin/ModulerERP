using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Infrastructure.Persistence;

namespace ModulerERP.CRM.Infrastructure.Features.TicketMessages.Queries;

public record GetTicketMessagesQuery(Guid TicketId) : IRequest<List<TicketMessageListDto>>;

public class GetTicketMessagesQueryHandler : IRequestHandler<GetTicketMessagesQuery, List<TicketMessageListDto>>
{
    private readonly CRMDbContext _context;
    public GetTicketMessagesQueryHandler(CRMDbContext context) => _context = context;

    public async Task<List<TicketMessageListDto>> Handle(GetTicketMessagesQuery r, CancellationToken ct)
    {
        return await _context.TicketMessages
            .Where(m => m.TicketId == r.TicketId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new TicketMessageListDto(m.Id, m.SenderUserId, null, m.Message, m.IsInternal, m.CreatedAt))
            .ToListAsync(ct);
    }
}
