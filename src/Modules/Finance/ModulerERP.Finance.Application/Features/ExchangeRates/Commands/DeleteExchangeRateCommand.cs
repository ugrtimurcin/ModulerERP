using MediatR;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.ExchangeRates.Commands;

public record DeleteExchangeRateCommand(Guid Id) : IRequest<Result>;

public class DeleteExchangeRateCommandHandler : IRequestHandler<DeleteExchangeRateCommand, Result>
{
    private readonly IRepository<ExchangeRate> _repository;
    private readonly IFinanceUnitOfWork _unitOfWork;

    public DeleteExchangeRateCommandHandler(IRepository<ExchangeRate> repository, IFinanceUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var rate = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (rate == null) return Result.Failure("Rate not found");

        _repository.Remove(rate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
