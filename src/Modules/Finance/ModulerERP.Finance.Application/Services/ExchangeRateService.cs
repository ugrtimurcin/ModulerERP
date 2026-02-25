using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ModulerERP.Finance.Application.Services;

public class ExchangeRateService : ModulerERP.SharedKernel.Interfaces.IExchangeRateService
{
    private readonly IRepository<ExchangeRate> _repository;
    private readonly IFinanceUnitOfWork _unitOfWork;
    private readonly IServiceProvider _serviceProvider;
    private readonly ICurrencyLookupService _currencyLookupService;
    private readonly Microsoft.Extensions.Logging.ILogger<ExchangeRateService> _logger;

    public ExchangeRateService(
        IRepository<ExchangeRate> repository, 
        IFinanceUnitOfWork unitOfWork, 
        IServiceProvider serviceProvider,
        ICurrencyLookupService currencyLookupService,
        Microsoft.Extensions.Logging.ILogger<ExchangeRateService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _serviceProvider = serviceProvider;
        _currencyLookupService = currencyLookupService;
        _logger = logger;
    }



    public async Task<Result<decimal>> GetExchangeRateAsync(Guid tenantId, Guid fromCurrencyId, Guid toCurrencyId, DateTime date, CancellationToken cancellationToken = default)
    {
        if (fromCurrencyId == toCurrencyId)
            return Result<decimal>.Success(1.0m);

        // Try direct rate
        var directRates = await _repository.FindAsync(
            r => r.FromCurrencyId == fromCurrencyId && 
                 r.ToCurrencyId == toCurrencyId && 
                 r.RateDate.Date == date.Date,
            cancellationToken);

        var rate = directRates.OrderByDescending(r => r.RateDate).FirstOrDefault();

        if (rate != null)
            return Result<decimal>.Success(rate.Rate);

        // Try reverse rate
        var reverseRates = await _repository.FindAsync(
            r => r.FromCurrencyId == toCurrencyId && 
                 r.ToCurrencyId == fromCurrencyId && 
                 r.RateDate.Date == date.Date,
            cancellationToken);

        var reverseRate = reverseRates.OrderByDescending(r => r.RateDate).FirstOrDefault();

        if (reverseRate != null && reverseRate.Rate != 0)
            return Result<decimal>.Success(1.0m / reverseRate.Rate);

        return Result<decimal>.Failure($"Exchange rate not found for {date:d}");
    }
}
