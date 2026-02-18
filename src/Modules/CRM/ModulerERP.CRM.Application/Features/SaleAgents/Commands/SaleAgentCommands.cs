using MediatR;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.CRM.Application.Features.SaleAgents.Commands;

// ── DTOs for commands (no separate DTO file needed for phantom entities) ──
public record SaleAgentDto(Guid Id, Guid EmployeeId, decimal CommissionRate, string CommissionType, DateTime CreatedAt);

// ── Create ──
public record CreateSaleAgentCommand(Guid EmployeeId, decimal CommissionRate) : IRequest<SaleAgentDto>;

public class CreateSaleAgentCommandHandler : IRequestHandler<CreateSaleAgentCommand, SaleAgentDto>
{
    private readonly IRepository<SaleAgent> _repo;
    private readonly ICRMUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateSaleAgentCommandHandler(IRepository<SaleAgent> repo, ICRMUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task<SaleAgentDto> Handle(CreateSaleAgentCommand r, CancellationToken ct)
    {
        var agent = SaleAgent.Create(_currentUser.TenantId, r.EmployeeId, r.CommissionRate, _currentUser.UserId);
        await _repo.AddAsync(agent, ct);
        await _uow.SaveChangesAsync(ct);
        return new SaleAgentDto(agent.Id, agent.EmployeeId, agent.CommissionRate, agent.CommissionType, agent.CreatedAt);
    }
}

// ── Delete ──
public record DeleteSaleAgentCommand(Guid Id) : IRequest;

public class DeleteSaleAgentCommandHandler : IRequestHandler<DeleteSaleAgentCommand>
{
    private readonly IRepository<SaleAgent> _repo;
    private readonly ICRMUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteSaleAgentCommandHandler(IRepository<SaleAgent> repo, ICRMUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task Handle(DeleteSaleAgentCommand r, CancellationToken ct)
    {
        var agent = await _repo.GetByIdAsync(r.Id, ct) ?? throw new KeyNotFoundException($"Sale agent '{r.Id}' not found.");
        agent.Delete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}
