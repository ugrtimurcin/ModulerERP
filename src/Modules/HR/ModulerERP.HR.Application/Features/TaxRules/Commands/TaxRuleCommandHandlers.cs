using MediatR;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.TaxRules.Commands;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.HR.Application.Interfaces;

namespace ModulerERP.HR.Application.Features.TaxRules.Commands;

public class TaxRuleCommandHandlers :
    IRequestHandler<CreateTaxRuleCommand, TaxRuleDto>,
    IRequestHandler<UpdateTaxRuleCommand, TaxRuleDto>,
    IRequestHandler<DeleteTaxRuleCommand>
{
    private readonly IRepository<TaxRule> _repository;
    private readonly IHRUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public TaxRuleCommandHandlers(
        IRepository<TaxRule> repository,
        IHRUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<TaxRuleDto> Handle(CreateTaxRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = TaxRule.Create(
            _currentUserService.TenantId,
            _currentUserService.UserId,
            request.Name,
            request.LowerLimit,
            request.UpperLimit,
            request.Rate,
            request.Order,
            request.EffectiveFrom.Kind == DateTimeKind.Utc ? request.EffectiveFrom : DateTime.SpecifyKind(request.EffectiveFrom, DateTimeKind.Utc)
        );

        await _repository.AddAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(rule);
    }

    public async Task<TaxRuleDto> Handle(UpdateTaxRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (rule == null) throw new KeyNotFoundException($"Tax rule with ID {request.Id} not found.");

        rule.Update(
            request.Name,
            request.LowerLimit,
            request.UpperLimit,
            request.Rate,
            request.Order,
            request.EffectiveFrom.Kind == DateTimeKind.Utc ? request.EffectiveFrom : DateTime.SpecifyKind(request.EffectiveFrom, DateTimeKind.Utc),
            request.EffectiveTo.HasValue 
                ? (request.EffectiveTo.Value.Kind == DateTimeKind.Utc ? request.EffectiveTo.Value : DateTime.SpecifyKind(request.EffectiveTo.Value, DateTimeKind.Utc))
                : null
        );

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(rule);
    }

    public async Task Handle(DeleteTaxRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (rule != null)
        {
            _repository.Remove(rule);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private static TaxRuleDto MapToDto(TaxRule rule) => new(
        rule.Id,
        rule.Name,
        rule.LowerLimit,
        rule.UpperLimit,
        rule.Rate,
        rule.Order,
        rule.EffectiveFrom,
        rule.EffectiveTo
    );
}
