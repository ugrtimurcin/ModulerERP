using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.ExchangeRates.Commands;

public record UpdateExchangeRateCommand(Guid Id, UpdateExchangeRateDto Dto) : IRequest<Result<ExchangeRateDto>>;

public class UpdateExchangeRateCommandHandler : IRequestHandler<UpdateExchangeRateCommand, Result<ExchangeRateDto>>
{
    private readonly IRepository<ExchangeRate> _repository;
    private readonly IFinanceUnitOfWork _unitOfWork;

    public UpdateExchangeRateCommandHandler(IRepository<ExchangeRate> repository, IFinanceUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ExchangeRateDto>> Handle(UpdateExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var rate = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (rate == null) return Result<ExchangeRateDto>.Failure("Rate not found");

        rate.UpdateRates(request.Dto.Rate, request.Dto.BuyingRate, request.Dto.SellingRate);
        _repository.Update(rate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ExchangeRateDto>.Success(new ExchangeRateDto
        {
            Id = rate.Id,
            FromCurrencyId = rate.FromCurrencyId,
            ToCurrencyId = rate.ToCurrencyId,
            RateDate = rate.RateDate,
            Rate = rate.Rate,
            BuyingRate = rate.BuyingRate,
            SellingRate = rate.SellingRate,
            Source = rate.Source.ToString()
        });
    }
}
