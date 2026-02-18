using MediatR;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.CRM.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.CRM.Application.Features.SupportTickets.Commands;

// ── Create ──
public record CreateSupportTicketCommand(
    string Title, string Description, int Priority = 1,
    Guid? PartnerId = null, Guid? AssignedUserId = null) : IRequest<SupportTicketDetailDto>;

public class CreateSupportTicketCommandHandler : IRequestHandler<CreateSupportTicketCommand, SupportTicketDetailDto>
{
    private readonly IRepository<SupportTicket> _repo;
    private readonly ICRMUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateSupportTicketCommandHandler(IRepository<SupportTicket> repo, ICRMUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task<SupportTicketDetailDto> Handle(CreateSupportTicketCommand r, CancellationToken ct)
    {
        var priority = (TicketPriority)r.Priority;
        var ticket = SupportTicket.Create(
            _currentUser.TenantId, r.Title, r.Description, _currentUser.UserId,
            r.PartnerId, priority, r.AssignedUserId);

        await _repo.AddAsync(ticket, ct);
        await _uow.SaveChangesAsync(ct);

        return new SupportTicketDetailDto(ticket.Id, ticket.Title, ticket.Description,
            (int)ticket.Priority, ticket.Priority.ToString(), (int)ticket.Status, ticket.Status.ToString(),
            ticket.PartnerId, null, ticket.AssignedUserId, ticket.Resolution, ticket.ResolvedAt, ticket.ClosedAt, ticket.CreatedAt);
    }
}

// ── Resolve ──
public record ResolveSupportTicketCommand(Guid Id, string Resolution) : IRequest;

public class ResolveSupportTicketCommandHandler : IRequestHandler<ResolveSupportTicketCommand>
{
    private readonly IRepository<SupportTicket> _repo;
    private readonly ICRMUnitOfWork _uow;

    public ResolveSupportTicketCommandHandler(IRepository<SupportTicket> repo, ICRMUnitOfWork uow) { _repo = repo; _uow = uow; }

    public async Task Handle(ResolveSupportTicketCommand r, CancellationToken ct)
    {
        var ticket = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Ticket '{r.Id}' not found.");
        ticket.Resolve(r.Resolution);
        await _uow.SaveChangesAsync(ct);
    }
}

// ── Close ──
public record CloseSupportTicketCommand(Guid Id) : IRequest;

public class CloseSupportTicketCommandHandler : IRequestHandler<CloseSupportTicketCommand>
{
    private readonly IRepository<SupportTicket> _repo;
    private readonly ICRMUnitOfWork _uow;

    public CloseSupportTicketCommandHandler(IRepository<SupportTicket> repo, ICRMUnitOfWork uow) { _repo = repo; _uow = uow; }

    public async Task Handle(CloseSupportTicketCommand r, CancellationToken ct)
    {
        var ticket = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Ticket '{r.Id}' not found.");
        ticket.Close();
        await _uow.SaveChangesAsync(ct);
    }
}

// ── Delete ──
public record DeleteSupportTicketCommand(Guid Id) : IRequest;

public class DeleteSupportTicketCommandHandler : IRequestHandler<DeleteSupportTicketCommand>
{
    private readonly IRepository<SupportTicket> _repo;
    private readonly ICRMUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteSupportTicketCommandHandler(IRepository<SupportTicket> repo, ICRMUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task Handle(DeleteSupportTicketCommand r, CancellationToken ct)
    {
        var ticket = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Ticket '{r.Id}' not found.");
        ticket.Delete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}
