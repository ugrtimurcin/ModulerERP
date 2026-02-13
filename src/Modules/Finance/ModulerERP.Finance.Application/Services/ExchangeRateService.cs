using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ModulerERP.Finance.Application.Services;

public interface IExchangeRateService
{
    Task<Result<List<ExchangeRateDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<ExchangeRateDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ExchangeRateDto>> CreateAsync(Guid tenantId, CreateExchangeRateDto dto, Guid createdByUserId, CancellationToken cancellationToken = default);
    Task<Result<ExchangeRateDto>> UpdateAsync(Guid id, UpdateExchangeRateDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<int>> SyncRatesAsync(Guid tenantId, Guid createdByUserId, CancellationToken cancellationToken = default);
    Task<Result<ExternalRateDto>> GetExternalRateAsync(DateTime date, string currencyCode, CancellationToken cancellationToken = default);
    Task<Result<decimal>> GetExchangeRateAsync(Guid tenantId, Guid fromCurrencyId, Guid toCurrencyId, DateTime date, CancellationToken cancellationToken = default);
}

public class ExchangeRateService : IExchangeRateService, ModulerERP.SharedKernel.Interfaces.IExchangeRateService
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

    public async Task<Result<List<ExchangeRateDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all exchange rates...");
        var rates = await _repository.GetAllAsync(cancellationToken);
        _logger.LogInformation($"Retrieved {rates.Count} rates from repository.");
        
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

    public async Task<Result<ExchangeRateDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var r = await _repository.GetByIdAsync(id, cancellationToken);
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

    public async Task<Result<ExchangeRateDto>> CreateAsync(Guid tenantId, CreateExchangeRateDto dto, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        // Npgsql requires UTC for timestamptz
        if (dto.RateDate.Kind == DateTimeKind.Unspecified)
            dto.RateDate = DateTime.SpecifyKind(dto.RateDate, DateTimeKind.Utc);
        else
            dto.RateDate = dto.RateDate.ToUniversalTime();

        // Check duplicate
        var existing = await _repository.FirstOrDefaultAsync(
            r => r.FromCurrencyId == dto.FromCurrencyId && 
                 r.ToCurrencyId == dto.ToCurrencyId && 
                 r.RateDate == dto.RateDate, 
            cancellationToken);

        if (existing != null)
             return Result<ExchangeRateDto>.Failure($"Rate for this currency pair at {dto.RateDate} already exists.");

        var rate = ExchangeRate.Create(
            tenantId,
            dto.FromCurrencyId,
            dto.ToCurrencyId,
            dto.RateDate,
            dto.Rate,
            dto.BuyingRate,
            dto.SellingRate,
            ExchangeRateSource.Manual
        );

        _logger.LogInformation($"Creating exchange rate for tenant {tenantId}. {dto.FromCurrencyId}->{dto.ToCurrencyId}");
        
        await _repository.AddAsync(rate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation($"Exchange rate created successfully. ID: {rate.Id}");

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

    public async Task<Result<ExchangeRateDto>> UpdateAsync(Guid id, UpdateExchangeRateDto dto, CancellationToken cancellationToken = default)
    {
        var rate = await _repository.GetByIdAsync(id, cancellationToken);
        if (rate == null) return Result<ExchangeRateDto>.Failure("Rate not found");

        rate.UpdateRates(dto.Rate, dto.BuyingRate, dto.SellingRate);
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

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rate = await _repository.GetByIdAsync(id, cancellationToken);
        if (rate == null) return Result.Failure("Rate not found");

        _repository.Remove(rate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<int>> SyncRatesAsync(Guid tenantId, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        var provider = _serviceProvider.GetService(typeof(IExchangeRateProvider)) as IExchangeRateProvider;
        if (provider == null) return Result<int>.Failure("Exchange Rate Provider not configured");

        // 1. Get External Rates
        var ratesResult = await provider.GetDailyRatesAsync(null, cancellationToken);
        if (!ratesResult.IsSuccess) return Result<int>.Failure(ratesResult.Error ?? "Unknown external error");
        
        var externalRates = ratesResult.Value!;
        if (!externalRates.Any()) return Result<int>.Success(0);

        // 2. Get Active Currencies
        var currenciesResult = await _currencyLookupService.GetActiveCurrenciesAsync(cancellationToken);
        if (!currenciesResult.IsSuccess) return Result<int>.Failure(currenciesResult.Error);
        
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
                    tenantId,
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
    public async Task<Result<ExternalRateDto>> GetExternalRateAsync(DateTime date, string currencyCode, CancellationToken cancellationToken = default)
    {
        var provider = _serviceProvider.GetService(typeof(IExchangeRateProvider)) as IExchangeRateProvider;
        if (provider == null) return Result<ExternalRateDto>.Failure("Exchange Rate Provider not configured");

        var ratesResult = await provider.GetDailyRatesAsync(date, cancellationToken);
        if (!ratesResult.IsSuccess) return Result<ExternalRateDto>.Failure(ratesResult.Error ?? "Unknown external error");

        var rate = ratesResult.Value?.FirstOrDefault(r => r.CurrencyCode.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));
        if (rate == null) return Result<ExternalRateDto>.Failure($"Rate not found for {currencyCode} on {date:yyyy-MM-dd}");

        return Result<ExternalRateDto>.Success(rate);
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
