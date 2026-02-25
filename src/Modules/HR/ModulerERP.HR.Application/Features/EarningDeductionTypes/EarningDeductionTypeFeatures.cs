using MediatR;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Features.EarningDeductionTypes;

public record EarningDeductionTypeDto(Guid Id, string Name, EarningDeductionCategory Category, bool IsTaxable, bool IsSgkExempt, decimal? MaxExemptAmount, bool IsActive);

public record GetEarningDeductionTypesQuery : IRequest<List<EarningDeductionTypeDto>>;

public record CreateEarningDeductionTypeCommand(string Name, EarningDeductionCategory Category, bool IsTaxable, bool IsSgkExempt, decimal? MaxExemptAmount) : IRequest<Guid>;
public class CreateEarningDeductionTypeCommandHandler : IRequestHandler<CreateEarningDeductionTypeCommand, Guid>
{
    private readonly IRepository<EarningDeductionType> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateEarningDeductionTypeCommandHandler(IRepository<EarningDeductionType> repository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateEarningDeductionTypeCommand request, CancellationToken cancellationToken)
    {
        var type = EarningDeductionType.Create(_currentUserService.TenantId, _currentUserService.UserId, request.Name, request.Category, request.IsTaxable, request.IsSgkExempt, request.MaxExemptAmount);
        await _repository.AddAsync(type, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return type.Id;
    }
}

public record UpdateEarningDeductionTypeCommand(Guid Id, string Name, EarningDeductionCategory Category, bool IsTaxable, bool IsSgkExempt, decimal? MaxExemptAmount, bool IsActive) : IRequest<Unit>;
public class UpdateEarningDeductionTypeCommandHandler : IRequestHandler<UpdateEarningDeductionTypeCommand, Unit>
{
    private readonly IRepository<EarningDeductionType> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEarningDeductionTypeCommandHandler(IRepository<EarningDeductionType> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateEarningDeductionTypeCommand request, CancellationToken cancellationToken)
    {
        var type = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (type == null) throw new Exception("Type not found");

        type.Update(request.Name, request.Category, request.IsTaxable, request.IsSgkExempt, request.MaxExemptAmount, request.IsActive);
        _repository.Update(type);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public record DeleteEarningDeductionTypeCommand(Guid Id) : IRequest<Unit>;
public class DeleteEarningDeductionTypeCommandHandler : IRequestHandler<DeleteEarningDeductionTypeCommand, Unit>
{
    private readonly IRepository<EarningDeductionType> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEarningDeductionTypeCommandHandler(IRepository<EarningDeductionType> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteEarningDeductionTypeCommand request, CancellationToken cancellationToken)
    {
        var type = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (type == null) throw new Exception("Type not found");

        _repository.Remove(type);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
