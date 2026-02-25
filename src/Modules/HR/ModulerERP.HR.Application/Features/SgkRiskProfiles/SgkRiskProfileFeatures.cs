using MediatR;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Features.SgkRiskProfiles;

public record SgkRiskProfileDto(Guid Id, string Name, decimal EmployerSgkMultiplier, string Description);

public record GetSgkRiskProfilesQuery : IRequest<List<SgkRiskProfileDto>>;

public record CreateSgkRiskProfileCommand(string Name, decimal EmployerSgkMultiplier, string Description) : IRequest<Guid>;
public class CreateSgkRiskProfileCommandHandler : IRequestHandler<CreateSgkRiskProfileCommand, Guid>
{
    private readonly IRepository<SgkRiskProfile> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateSgkRiskProfileCommandHandler(IRepository<SgkRiskProfile> repository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateSgkRiskProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = SgkRiskProfile.Create(_currentUserService.TenantId, _currentUserService.UserId, request.Name, request.EmployerSgkMultiplier, request.Description);
        await _repository.AddAsync(profile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return profile.Id;
    }
}

public record UpdateSgkRiskProfileCommand(Guid Id, string Name, decimal EmployerSgkMultiplier, string Description) : IRequest<Unit>;
public class UpdateSgkRiskProfileCommandHandler : IRequestHandler<UpdateSgkRiskProfileCommand, Unit>
{
    private readonly IRepository<SgkRiskProfile> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSgkRiskProfileCommandHandler(IRepository<SgkRiskProfile> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateSgkRiskProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (profile == null) throw new Exception("Profile not found");

        profile.Update(request.Name, request.EmployerSgkMultiplier, request.Description);
        _repository.Update(profile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public record DeleteSgkRiskProfileCommand(Guid Id) : IRequest<Unit>;
public class DeleteSgkRiskProfileCommandHandler : IRequestHandler<DeleteSgkRiskProfileCommand, Unit>
{
    private readonly IRepository<SgkRiskProfile> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSgkRiskProfileCommandHandler(IRepository<SgkRiskProfile> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteSgkRiskProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (profile == null) throw new Exception("Profile not found");

        _repository.Remove(profile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
