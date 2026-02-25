using MediatR;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Features.LeavePolicies;

public record LeavePolicyDto(Guid Id, string Name, bool IsPaid, bool RequiresSgkMissingDayCode, int DefaultDays, bool IsActive);

public record GetLeavePoliciesQuery : IRequest<List<LeavePolicyDto>>;

public record CreateLeavePolicyCommand(string Name, bool IsPaid, bool RequiresSgkMissingDayCode, int DefaultDays) : IRequest<Guid>;
public class CreateLeavePolicyCommandHandler : IRequestHandler<CreateLeavePolicyCommand, Guid>
{
    private readonly IRepository<LeavePolicy> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateLeavePolicyCommandHandler(IRepository<LeavePolicy> repository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateLeavePolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = LeavePolicy.Create(_currentUserService.TenantId, _currentUserService.UserId, request.Name, request.IsPaid, request.RequiresSgkMissingDayCode, request.DefaultDays);
        await _repository.AddAsync(policy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return policy.Id;
    }
}

public record UpdateLeavePolicyCommand(Guid Id, string Name, bool IsPaid, bool RequiresSgkMissingDayCode, int DefaultDays, bool IsActive) : IRequest<Unit>;
public class UpdateLeavePolicyCommandHandler : IRequestHandler<UpdateLeavePolicyCommand, Unit>
{
    private readonly IRepository<LeavePolicy> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLeavePolicyCommandHandler(IRepository<LeavePolicy> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateLeavePolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (policy == null) throw new Exception("Leave Policy not found");

        policy.Update(request.Name, request.IsPaid, request.RequiresSgkMissingDayCode, request.DefaultDays, request.IsActive);
        _repository.Update(policy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public record DeleteLeavePolicyCommand(Guid Id) : IRequest<Unit>;
public class DeleteLeavePolicyCommandHandler : IRequestHandler<DeleteLeavePolicyCommand, Unit>
{
    private readonly IRepository<LeavePolicy> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteLeavePolicyCommandHandler(IRepository<LeavePolicy> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteLeavePolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (policy == null) throw new Exception("Leave Policy not found");

        _repository.Remove(policy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
