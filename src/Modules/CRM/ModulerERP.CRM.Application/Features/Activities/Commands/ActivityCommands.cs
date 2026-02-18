using MediatR;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.CRM.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.CRM.Application.Features.Activities.Commands;

// ── Create ──
public record CreateActivityCommand(
    ActivityType Type, string Subject, string? Description,
    DateTime ActivityDate, string EntityType, Guid EntityId,
    bool IsScheduled = false) : IRequest<ActivityDto>;

public class CreateActivityCommandHandler : IRequestHandler<CreateActivityCommand, ActivityDto>
{
    private readonly IRepository<Activity> _repo;
    private readonly ICRMUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateActivityCommandHandler(IRepository<Activity> repo, ICRMUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task<ActivityDto> Handle(CreateActivityCommand r, CancellationToken ct)
    {
        var activity = Activity.Create(
            _currentUser.TenantId, r.Type, r.Subject, r.EntityType, r.EntityId,
            r.ActivityDate, _currentUser.UserId, r.Description, r.IsScheduled);

        await _repo.AddAsync(activity, ct);
        await _uow.SaveChangesAsync(ct);

        return new ActivityDto(activity.Id, activity.Type, activity.Subject, activity.Description,
            activity.ActivityDate, activity.EntityType, activity.EntityId,
            activity.IsScheduled, activity.IsCompleted, activity.CompletedAt,
            activity.CreatedAt, activity.CreatedBy);
    }
}

// ── Update ──
public record UpdateActivityCommand(
    Guid Id, string? Subject = null, string? Description = null,
    DateTime? ActivityDate = null, bool? IsCompleted = null) : IRequest<ActivityDto>;

public class UpdateActivityCommandHandler : IRequestHandler<UpdateActivityCommand, ActivityDto>
{
    private readonly IRepository<Activity> _repo;
    private readonly ICRMUnitOfWork _uow;

    public UpdateActivityCommandHandler(IRepository<Activity> repo, ICRMUnitOfWork uow) { _repo = repo; _uow = uow; }

    public async Task<ActivityDto> Handle(UpdateActivityCommand r, CancellationToken ct)
    {
        var activity = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Activity '{r.Id}' not found.");

        // Only update fields that are provided
        var subject = r.Subject ?? activity.Subject;
        var desc = r.Description ?? activity.Description;
        var date = r.ActivityDate ?? activity.ActivityDate;
        activity.Update(subject, desc, date);

        if (r.IsCompleted == true) activity.MarkAsCompleted();

        await _uow.SaveChangesAsync(ct);

        return new ActivityDto(activity.Id, activity.Type, activity.Subject, activity.Description,
            activity.ActivityDate, activity.EntityType, activity.EntityId,
            activity.IsScheduled, activity.IsCompleted, activity.CompletedAt,
            activity.CreatedAt, activity.CreatedBy);
    }
}

// ── Delete ──
public record DeleteActivityCommand(Guid Id) : IRequest;

public class DeleteActivityCommandHandler : IRequestHandler<DeleteActivityCommand>
{
    private readonly IRepository<Activity> _repo;
    private readonly ICRMUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteActivityCommandHandler(IRepository<Activity> repo, ICRMUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task Handle(DeleteActivityCommand request, CancellationToken ct)
    {
        var activity = await _repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Activity '{request.Id}' not found.");
        activity.Delete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}
