using MediatR;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.CRM.Application.Features.TicketMessages.Commands;

public record CreateTicketMessageCommand(Guid TicketId, string Message, bool IsInternal = false) : IRequest<TicketMessageListDto>;

public class CreateTicketMessageCommandHandler : IRequestHandler<CreateTicketMessageCommand, TicketMessageListDto>
{
    private readonly IRepository<TicketMessage> _repo;
    private readonly ICRMUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateTicketMessageCommandHandler(IRepository<TicketMessage> repo, ICRMUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task<TicketMessageListDto> Handle(CreateTicketMessageCommand r, CancellationToken ct)
    {
        var msg = TicketMessage.Create(_currentUser.TenantId, r.TicketId, r.Message,
            _currentUser.UserId, _currentUser.UserId, r.IsInternal);
        await _repo.AddAsync(msg, ct);
        await _uow.SaveChangesAsync(ct);
        return new TicketMessageListDto(msg.Id, msg.SenderUserId, null, msg.Message, msg.IsInternal, msg.CreatedAt);
    }
}
