using MediatR;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.CRM.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.CRM.Application.Features.CommissionRules.Commands;

public record CommissionRuleDto(
    Guid Id, string Name, Guid? UserId, Guid? PartnerGroupId, Guid? ProductCategoryId,
    string Basis, decimal Rate, int Priority,
    DateTime? ValidFrom, DateTime? ValidTo, DateTime CreatedAt);

public record CreateCommissionRuleCommand(
    string Name, string Basis, decimal Rate,
    Guid? UserId = null, Guid? PartnerGroupId = null, Guid? ProductCategoryId = null,
    int Priority = 0) : IRequest<CommissionRuleDto>;

public class CreateCommissionRuleCommandHandler : IRequestHandler<CreateCommissionRuleCommand, CommissionRuleDto>
{
    private readonly IRepository<CommissionRule> _repo;
    private readonly ICRMUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateCommissionRuleCommandHandler(IRepository<CommissionRule> repo, ICRMUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task<CommissionRuleDto> Handle(CreateCommissionRuleCommand r, CancellationToken ct)
    {
        var basis = Enum.Parse<CommissionBasis>(r.Basis);
        var rule = CommissionRule.Create(_currentUser.TenantId, r.Name, basis, r.Rate,
            _currentUser.UserId, r.UserId, r.PartnerGroupId, r.ProductCategoryId, r.Priority);
        await _repo.AddAsync(rule, ct);
        await _uow.SaveChangesAsync(ct);
        return new CommissionRuleDto(rule.Id, rule.Name, rule.UserId, rule.PartnerGroupId,
            rule.ProductCategoryId, rule.Basis.ToString(), rule.Rate, rule.Priority,
            rule.ValidFrom, rule.ValidTo, rule.CreatedAt);
    }
}

public record DeleteCommissionRuleCommand(Guid Id) : IRequest;

public class DeleteCommissionRuleCommandHandler : IRequestHandler<DeleteCommissionRuleCommand>
{
    private readonly IRepository<CommissionRule> _repo;
    private readonly ICRMUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteCommissionRuleCommandHandler(IRepository<CommissionRule> repo, ICRMUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task Handle(DeleteCommissionRuleCommand r, CancellationToken ct)
    {
        var rule = await _repo.GetByIdAsync(r.Id, ct) ?? throw new KeyNotFoundException($"Commission rule '{r.Id}' not found.");
        rule.Delete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}
