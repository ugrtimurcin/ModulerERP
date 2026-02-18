using MediatR;
using ModulerERP.HR.Application.Features.CommissionRules.Commands;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Features.CommissionRules.Commands;

public class CommissionRuleCommandHandlers :
    IRequestHandler<CreateCommissionRuleCommand, Guid>,
    IRequestHandler<DeleteCommissionRuleCommand>
{
    private readonly IRepository<CommissionRule> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CommissionRuleCommandHandlers(
        IRepository<CommissionRule> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateCommissionRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = CommissionRule.Create(
            _currentUserService.TenantId,
            _currentUserService.UserId,
            request.Role,
            request.MinTargetAmount,
            request.Percentage,
            request.Basis
        );

        await _repository.AddAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return rule.Id;
    }

    public async Task Handle(DeleteCommissionRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (rule != null)
        {
            _repository.Remove(rule);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
