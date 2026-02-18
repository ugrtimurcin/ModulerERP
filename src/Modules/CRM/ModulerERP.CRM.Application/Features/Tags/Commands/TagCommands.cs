using MediatR;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.CRM.Application.Features.Tags.Commands;

public record CreateTagCommand(string Name, string ColorCode = "#3B82F6", string? EntityType = null) : IRequest<TagDetailDto>;

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, TagDetailDto>
{
    private readonly IRepository<Tag> _repo;
    private readonly ICRMUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateTagCommandHandler(IRepository<Tag> repo, ICRMUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task<TagDetailDto> Handle(CreateTagCommand r, CancellationToken ct)
    {
        var tag = Tag.Create(_currentUser.TenantId, r.Name, _currentUser.UserId, r.ColorCode, r.EntityType);
        await _repo.AddAsync(tag, ct);
        await _uow.SaveChangesAsync(ct);
        return new TagDetailDto(tag.Id, tag.Name, tag.ColorCode, tag.EntityType, tag.CreatedAt);
    }
}

public record UpdateTagCommand(Guid Id, string Name, string ColorCode, string? EntityType = null) : IRequest<TagDetailDto>;

public class UpdateTagCommandHandler : IRequestHandler<UpdateTagCommand, TagDetailDto>
{
    private readonly IRepository<Tag> _repo;
    private readonly ICRMUnitOfWork _uow;

    public UpdateTagCommandHandler(IRepository<Tag> repo, ICRMUnitOfWork uow) { _repo = repo; _uow = uow; }

    public async Task<TagDetailDto> Handle(UpdateTagCommand r, CancellationToken ct)
    {
        var tag = await _repo.GetByIdAsync(r.Id, ct) ?? throw new KeyNotFoundException($"Tag '{r.Id}' not found.");
        tag.Update(r.Name, r.ColorCode, r.EntityType);
        await _uow.SaveChangesAsync(ct);
        return new TagDetailDto(tag.Id, tag.Name, tag.ColorCode, tag.EntityType, tag.CreatedAt);
    }
}

public record DeleteTagCommand(Guid Id) : IRequest;

public class DeleteTagCommandHandler : IRequestHandler<DeleteTagCommand>
{
    private readonly IRepository<Tag> _repo;
    private readonly ICRMUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteTagCommandHandler(IRepository<Tag> repo, ICRMUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task Handle(DeleteTagCommand r, CancellationToken ct)
    {
        var tag = await _repo.GetByIdAsync(r.Id, ct) ?? throw new KeyNotFoundException($"Tag '{r.Id}' not found.");
        tag.Delete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}
