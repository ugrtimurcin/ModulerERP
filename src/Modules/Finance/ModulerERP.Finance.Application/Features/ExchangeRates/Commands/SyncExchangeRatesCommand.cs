using MediatR;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.ExchangeRates.Commands;

public record SyncExchangeRatesCommand(Guid TenantId, Guid UserId) : IRequest<Result<int>>;

public class SyncExchangeRatesCommandHandler : IRequestHandler<SyncExchangeRatesCommand, Result<int>>
{
    private readonly IRepository<ExchangeRate> _repository;
    private readonly IFinanceUnitOfWork _unitOfWork;
    private readonly IServiceProvider _serviceProvider;
    private readonly ICurrencyLookupService _currencyLookupService;

    public SyncExchangeRatesCommandHandler(
        IRepository<ExchangeRate> repository, 
        IFinanceUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ICurrencyLookupService currencyLookupService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _serviceProvider = serviceProvider;
        _currencyLookupService = currencyLookupService;
    }

    public async Task<Result<int>> Handle(SyncExchangeRatesCommand request, CancellationToken cancellationToken)
    {
        var provider = _serviceProvider.GetService<IExchangeRateProvider>();
        if (provider == null) return Result<int>.Failure("Exchange Rate Provider not configured");

        // 1. Get External Rates
        var ratesResult = await provider.GetDailyRatesAsync(null, cancellationToken);
        if (!ratesResult.IsSuccess) return Result<int>.Failure(ratesResult.Error ?? "Unknown external error");
        
        var externalRates = ratesResult.Value!;
        if (!externalRates.Any()) return Result<int>.Success(0);

        // 2. Get Active Currencies
        var currenciesResult = await _currencyLookupService.GetActiveCurrenciesAsync(cancellationToken);
        if (!currenciesResult.IsSuccess) return Result<int>.Failure(currenciesResult.Error ?? "Unknown error");
        
        var currencies = currenciesResult.Value!;
        var currencyMap = currencies.ToDictionary(c => c.Code.ToUpper(), c => c.Id);

        if (!currencyMap.TryGetValue("TRY", out var tryId))
        {
             return Result<int>.Failure("Base currency (TRY) not found in system");
        }

        int count = 0;
        var now = DateTime.UtcNow; // Full timestamp
        var today = now.Date;

        foreach (var extRate in externalRates)
        {
            if (!currencyMap.TryGetValue(extRate.CurrencyCode.ToUpper(), out var sourceCurrencyId)) continue;
            if (sourceCurrencyId == tryId) continue; 

            // Check if rate exists for today
            var existing = await _repository.FirstOrDefaultAsync(r => 
                r.FromCurrencyId == sourceCurrencyId && 
                r.ToCurrencyId == tryId && 
                r.RateDate.Date == today, 
                cancellationToken);

            if (existing != null)
            {
                existing.UpdateRates(extRate.Rate, extRate.BuyingRate, extRate.SellingRate); 
                _repository.Update(existing);
            }
            else
            {
                var newRate = ExchangeRate.Create(
                    request.TenantId,
                    sourceCurrencyId,
                    tryId, 
                    now, 
                    extRate.Rate,
                    extRate.BuyingRate, 
                    extRate.SellingRate, 
                    ExchangeRateSource.CentralBank
                );
                await _repository.AddAsync(newRate, cancellationToken);
            }
            count++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(count);
    }
}
