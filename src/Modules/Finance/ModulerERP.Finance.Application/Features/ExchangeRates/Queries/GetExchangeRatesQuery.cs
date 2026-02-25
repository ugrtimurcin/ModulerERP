using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.ExchangeRates.Queries;

public record GetExchangeRatesQuery() : IRequest<Result<List<ExchangeRateDto>>>;

public class GetExchangeRatesQueryHandler : IRequestHandler<GetExchangeRatesQuery, Result<List<ExchangeRateDto>>>
{
    private readonly IRepository<ExchangeRate> _repository;

    public GetExchangeRatesQueryHandler(IRepository<ExchangeRate> repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ExchangeRateDto>>> Handle(GetExchangeRatesQuery request, CancellationToken cancellationToken)
    {
        var rates = await _repository.GetAllAsync(cancellationToken);
        
        var dtos = rates.Select(r => new ExchangeRateDto
        {
            Id = r.Id,
            FromCurrencyId = r.FromCurrencyId,
            ToCurrencyId = r.ToCurrencyId,
            RateDate = r.RateDate,
            Rate = r.Rate,
            BuyingRate = r.BuyingRate,
            SellingRate = r.SellingRate,
            Source = r.Source.ToString(),
        }).OrderByDescending(r => r.RateDate).ToList();

        return Result<List<ExchangeRateDto>>.Success(dtos);
    }
}
