using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.ExchangeRates.Queries;

public record GetExchangeRateByIdQuery(Guid Id) : IRequest<Result<ExchangeRateDto>>;

public class GetExchangeRateByIdQueryHandler : IRequestHandler<GetExchangeRateByIdQuery, Result<ExchangeRateDto>>
{
    private readonly IRepository<ExchangeRate> _repository;

    public GetExchangeRateByIdQueryHandler(IRepository<ExchangeRate> repository)
    {
        _repository = repository;
    }

    public async Task<Result<ExchangeRateDto>> Handle(GetExchangeRateByIdQuery request, CancellationToken cancellationToken)
    {
        var r = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (r == null) return Result<ExchangeRateDto>.Failure("Exchange Rate not found");

        return Result<ExchangeRateDto>.Success(new ExchangeRateDto
        {
            Id = r.Id,
            FromCurrencyId = r.FromCurrencyId,
            ToCurrencyId = r.ToCurrencyId,
            RateDate = r.RateDate,
            Rate = r.Rate,
            BuyingRate = r.BuyingRate,
            SellingRate = r.SellingRate,
            Source = r.Source.ToString()
        });
    }
}
