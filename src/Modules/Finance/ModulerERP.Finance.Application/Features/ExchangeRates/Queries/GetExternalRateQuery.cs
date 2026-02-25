using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ModulerERP.Finance.Application.Features.ExchangeRates.Queries;

public record GetExternalRateQuery(DateTime Date, string CurrencyCode) : IRequest<Result<ExternalRateDto>>;

public class GetExternalRateQueryHandler : IRequestHandler<GetExternalRateQuery, Result<ExternalRateDto>>
{
    private readonly IServiceProvider _serviceProvider;

    public GetExternalRateQueryHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Result<ExternalRateDto>> Handle(GetExternalRateQuery request, CancellationToken cancellationToken)
    {
        var provider = _serviceProvider.GetService<IExchangeRateProvider>();
        if (provider == null) return Result<ExternalRateDto>.Failure("Exchange Rate Provider not configured");

        var ratesResult = await provider.GetDailyRatesAsync(request.Date, cancellationToken);
        if (!ratesResult.IsSuccess) return Result<ExternalRateDto>.Failure(ratesResult.Error ?? "Unknown external error");

        var rate = ratesResult.Value?.FirstOrDefault(r => r.CurrencyCode.Equals(request.CurrencyCode, StringComparison.OrdinalIgnoreCase));
        if (rate == null) return Result<ExternalRateDto>.Failure($"Rate not found for {request.CurrencyCode} on {request.Date:yyyy-MM-dd}");

        return Result<ExternalRateDto>.Success(rate);
    }
}
