using MediatR;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.TaxProfiles.Commands;

public record DeleteTaxProfileCommand(Guid Id) : IRequest<Result<Guid>>;

public class DeleteTaxProfileCommandHandler : IRequestHandler<DeleteTaxProfileCommand, Result<Guid>>
{
    private readonly IRepository<TaxProfile> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTaxProfileCommandHandler(
        IRepository<TaxProfile> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(DeleteTaxProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (profile == null)
            return Result<Guid>.Failure("Tax Profile not found.");

        _repository.Remove(profile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(request.Id);
    }
}
