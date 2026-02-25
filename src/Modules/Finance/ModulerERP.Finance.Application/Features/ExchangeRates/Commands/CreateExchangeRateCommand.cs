using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.ExchangeRates.Commands;

public record CreateExchangeRateCommand(Guid TenantId, CreateExchangeRateDto Dto, Guid UserId) : IRequest<Result<ExchangeRateDto>>;

public class CreateExchangeRateCommandHandler : IRequestHandler<CreateExchangeRateCommand, Result<ExchangeRateDto>>
{
    private readonly IRepository<ExchangeRate> _repository;
    private readonly IFinanceUnitOfWork _unitOfWork;

    public CreateExchangeRateCommandHandler(IRepository<ExchangeRate> repository, IFinanceUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ExchangeRateDto>> Handle(CreateExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        if (dto.RateDate.Kind == DateTimeKind.Unspecified)
            dto.RateDate = DateTime.SpecifyKind(dto.RateDate, DateTimeKind.Utc);
        else
            dto.RateDate = dto.RateDate.ToUniversalTime();

        var existing = await _repository.FirstOrDefaultAsync(
            r => r.FromCurrencyId == dto.FromCurrencyId && 
                 r.ToCurrencyId == dto.ToCurrencyId && 
                 r.RateDate == dto.RateDate, 
            cancellationToken);

        if (existing != null)
             return Result<ExchangeRateDto>.Failure($"Rate for this currency pair at {dto.RateDate} already exists.");

        var rate = ExchangeRate.Create(
            request.TenantId,
            dto.FromCurrencyId,
            dto.ToCurrencyId,
            dto.RateDate,
            dto.Rate,
            dto.BuyingRate,
            dto.SellingRate,
            ExchangeRateSource.Manual
        );

        await _repository.AddAsync(rate, cancellationToken);
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
