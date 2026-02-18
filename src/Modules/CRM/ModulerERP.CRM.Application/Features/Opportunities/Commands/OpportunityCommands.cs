using MediatR;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using FluentValidation;

namespace ModulerERP.CRM.Application.Features.Opportunities.Commands;

// ── Create ──
public record CreateOpportunityCommand(
    string Title,
    decimal EstimatedValue,
    string CurrencyCode = "TRY",
    Guid? LeadId = null,
    Guid? PartnerId = null,
    Guid? CurrencyId = null,
    string Stage = "Discovery",
    DateTime? ExpectedCloseDate = null,
    Guid? AssignedUserId = null) : IRequest<OpportunityDetailDto>;

public class CreateOpportunityCommandValidator : AbstractValidator<CreateOpportunityCommand>
{
    public CreateOpportunityCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EstimatedValue).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CurrencyCode).NotEmpty().Length(3);
    }
}

public class CreateOpportunityCommandHandler : IRequestHandler<CreateOpportunityCommand, OpportunityDetailDto>
{
    private readonly IRepository<Opportunity> _repo;
    private readonly ICRMUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateOpportunityCommandHandler(IRepository<Opportunity> repo, ICRMUnitOfWork uow, ICurrentUserService currentUser)
    {
        _repo = repo;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<OpportunityDetailDto> Handle(CreateOpportunityCommand request, CancellationToken ct)
    {
        var opp = Opportunity.Create(
            _currentUser.TenantId, request.Title, request.EstimatedValue, request.CurrencyCode,
            _currentUser.UserId, request.LeadId, request.PartnerId, request.CurrencyId,
            request.AssignedUserId, request.ExpectedCloseDate);

        await _repo.AddAsync(opp, ct);
        await _uow.SaveChangesAsync(ct);

        return new OpportunityDetailDto(opp.Id, opp.Title, opp.LeadId, null, opp.PartnerId, null,
            opp.EstimatedValue.Amount, opp.CurrencyId, opp.EstimatedValue.CurrencyCode,
            opp.Stage.ToString(), opp.Probability, opp.WeightedValue,
            opp.ExpectedCloseDate, opp.AssignedUserId, null, opp.IsActive, opp.CreatedAt);
    }
}

// ── Update ──
public record UpdateOpportunityCommand(
    Guid Id,
    string Title,
    decimal EstimatedValue,
    string CurrencyCode = "TRY",
    Guid? PartnerId = null,
    Guid? CurrencyId = null,
    string? Stage = null,
    int? Probability = null,
    DateTime? ExpectedCloseDate = null,
    Guid? AssignedUserId = null,
    bool? IsActive = null) : IRequest<OpportunityDetailDto>;

public class UpdateOpportunityCommandHandler : IRequestHandler<UpdateOpportunityCommand, OpportunityDetailDto>
{
    private readonly IRepository<Opportunity> _repo;
    private readonly ICRMUnitOfWork _uow;

    public UpdateOpportunityCommandHandler(IRepository<Opportunity> repo, ICRMUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<OpportunityDetailDto> Handle(UpdateOpportunityCommand request, CancellationToken ct)
    {
        var opp = await _repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Opportunity '{request.Id}' not found.");

        if (!string.IsNullOrEmpty(request.Stage) && Enum.TryParse<Domain.Enums.OpportunityStage>(request.Stage, out var stage))
            opp.UpdateStage(stage);

        if (request.Probability.HasValue) opp.SetProbability(request.Probability.Value);
        opp.UpdateValue(request.EstimatedValue, request.CurrencyCode, request.CurrencyId);

        if (request.AssignedUserId.HasValue) opp.Assign(request.AssignedUserId.Value);
        if (request.IsActive.HasValue) { if (request.IsActive.Value) opp.Activate(); else opp.Deactivate(); }

        await _uow.SaveChangesAsync(ct);

        return new OpportunityDetailDto(opp.Id, opp.Title, opp.LeadId, null, opp.PartnerId, null,
            opp.EstimatedValue.Amount, opp.CurrencyId, opp.EstimatedValue.CurrencyCode,
            opp.Stage.ToString(), opp.Probability, opp.WeightedValue,
            opp.ExpectedCloseDate, opp.AssignedUserId, null, opp.IsActive, opp.CreatedAt);
    }
}

// ── Delete ──
public record DeleteOpportunityCommand(Guid Id) : IRequest;

public class DeleteOpportunityCommandHandler : IRequestHandler<DeleteOpportunityCommand>
{
    private readonly IRepository<Opportunity> _repo;
    private readonly ICRMUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteOpportunityCommandHandler(IRepository<Opportunity> repo, ICRMUnitOfWork uow, ICurrentUserService currentUser)
    {
        _repo = repo; _uow = uow; _currentUser = currentUser;
    }

    public async Task Handle(DeleteOpportunityCommand request, CancellationToken ct)
    {
        var opp = await _repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Opportunity '{request.Id}' not found.");
        opp.Delete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}

// ── Update Stage ──
public record UpdateOpportunityStageCommand(Guid Id, string Stage) : IRequest;

public class UpdateOpportunityStageCommandHandler : IRequestHandler<UpdateOpportunityStageCommand>
{
    private readonly IRepository<Opportunity> _repo;
    private readonly ICRMUnitOfWork _uow;

    public UpdateOpportunityStageCommandHandler(IRepository<Opportunity> repo, ICRMUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task Handle(UpdateOpportunityStageCommand request, CancellationToken ct)
    {
        if (!Enum.TryParse<Domain.Enums.OpportunityStage>(request.Stage, out var stage))
            throw new ArgumentException("Invalid stage");

        var opp = await _repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Opportunity '{request.Id}' not found.");
        opp.UpdateStage(stage);
        await _uow.SaveChangesAsync(ct);
    }
}
