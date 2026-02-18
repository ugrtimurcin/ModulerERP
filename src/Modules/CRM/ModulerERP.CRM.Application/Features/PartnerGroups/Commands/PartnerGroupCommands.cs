using MediatR;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.CRM.Application.Features.PartnerGroups.Commands;

public record PartnerGroupDto(Guid Id, string Name, string? Description, DateTime CreatedAt);

public record CreatePartnerGroupCommand(string Name, string? Description = null) : IRequest<PartnerGroupDto>;

public class CreatePartnerGroupCommandHandler : IRequestHandler<CreatePartnerGroupCommand, PartnerGroupDto>
{
    private readonly IRepository<BusinessPartnerGroup> _repo;
    private readonly ICRMUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreatePartnerGroupCommandHandler(IRepository<BusinessPartnerGroup> repo, ICRMUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task<PartnerGroupDto> Handle(CreatePartnerGroupCommand r, CancellationToken ct)
    {
        var group = BusinessPartnerGroup.Create(_currentUser.TenantId, r.Name, _currentUser.UserId, r.Description);
        await _repo.AddAsync(group, ct);
        await _uow.SaveChangesAsync(ct);
        return new PartnerGroupDto(group.Id, group.Name, group.Description, group.CreatedAt);
    }
}

public record UpdatePartnerGroupCommand(Guid Id, string Name, string? Description = null) : IRequest<PartnerGroupDto>;

public class UpdatePartnerGroupCommandHandler : IRequestHandler<UpdatePartnerGroupCommand, PartnerGroupDto>
{
    private readonly IRepository<BusinessPartnerGroup> _repo;
    private readonly ICRMUnitOfWork _uow;

    public UpdatePartnerGroupCommandHandler(IRepository<BusinessPartnerGroup> repo, ICRMUnitOfWork uow) { _repo = repo; _uow = uow; }

    public async Task<PartnerGroupDto> Handle(UpdatePartnerGroupCommand r, CancellationToken ct)
    {
        var group = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Partner group '{r.Id}' not found.");
        group.Update(r.Name, r.Description);
        await _uow.SaveChangesAsync(ct);
        return new PartnerGroupDto(group.Id, group.Name, group.Description, group.CreatedAt);
    }
}

public record DeletePartnerGroupCommand(Guid Id) : IRequest;

public class DeletePartnerGroupCommandHandler : IRequestHandler<DeletePartnerGroupCommand>
{
    private readonly IRepository<BusinessPartnerGroup> _repo;
    private readonly ICRMUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeletePartnerGroupCommandHandler(IRepository<BusinessPartnerGroup> repo, ICRMUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task Handle(DeletePartnerGroupCommand r, CancellationToken ct)
    {
        var group = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Partner group '{r.Id}' not found.");
        group.Delete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}
